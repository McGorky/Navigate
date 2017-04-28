using System;

namespace Mirea.Snar2017.Navigate
{
    public static class Filter
    {
        // TODO: поменять на калмана
        public static float[] Exponential(float[] previous, float[] current, float k)
        {
            float[] result = new float[3];
            for (int i = 0; i < 3; i++)
            {
                result[i] = k * current[i] + (1 - k) * previous[i];
            }

            return result;
        }

        public static Quaternion Exponential(Quaternion previous, Quaternion current, float k)
        {
            var result = new Quaternion();
            for (int i = 1; i <= 4; i++)
            {
                result[i] = k * current[i] + (1 - k) * previous[i];
            }

            return result;
        }

        public static Quaternion Calibrate(Matrix calibrationMatrix, Quaternion notCalibratedData)
        {
            var notCalibratedMatrix = new Matrix(4, 1);
            for (int i = 0; i < 3; i++)
            {
                notCalibratedMatrix[i, 0] = notCalibratedData[i+2];
            }

            notCalibratedMatrix[3, 0] = 1;

            var calibratedMatrix = calibrationMatrix * notCalibratedMatrix;
            return new Quaternion(0, calibratedMatrix[0, 0], calibratedMatrix[1, 0], calibratedMatrix[2, 0]);
        }

        // UNDONE: компенсация дрифта
        public static Quaternion Madgvic(Quaternion q, Quaternion g, Quaternion a, Quaternion m, float beta, float zeta, float dt)
        {
            if (!Storage.Current.GyroscopeDriftCompensationEnabled)
            {
                zeta = 0;
            }

            // TODO: проверить правильность
            q = q.Normalized();
            //g = g.Normalized();
            a = a.Normalized();
            m = m.Normalized();
            var h = q * m * q.Conjugated(); // q.Normalized() * m.Normalized() * q.Normalized().Conjugated();
            var b = new Quaternion(0, (float)Math.Sqrt(h[2] * h[2] + h[3] * h[3]), 0, h[4]).Normalized();

            var gr = GradF(q, a, b, m).Normalized();
            var qd = 0.5f * q * g - beta * gr;

            return q + qd * dt;
        }

        public static Quaternion Madgvic(Quaternion q, Quaternion g, Quaternion a, float beta, float dt)
        {
            q = q.Normalized();
            //g = g.Normalized();
            a = a.Normalized();

            var gr = GradF(q, a).Normalized();
            var qd = 0.5f * q * g - beta * gr;

            return q + qd * dt;
        }

        private static Quaternion GradF(Quaternion q, Quaternion a, Quaternion b, Quaternion m)
        {
            // TODO: проверить правильность
            var f = new Matrix(6, 1);
            f[0, 0] = 2 * (q[2] * q[4] - q[1] * q[3]) - a[2];
            f[1, 0] = 2 * (q[1] * q[2] + q[3] * q[4]) - a[3];
            f[2, 0] = 2 * (0.5f - q[2] * q[2] - q[3] * q[3]) - a[4];

            f[3, 0] = 2 * b[2] * (0.5f - q[3] * q[3] - q[4] * q[4]) + 2 * b[3] * (q[1] * q[4] + q[2] * q[3]) + 2 * b[4] * (q[2] * q[4] - q[1] * q[3]) - m[2];
            f[4, 0] = 2 * b[2] * (q[2] * q[3] - q[1] * q[4]) + 2 * b[3] * (0.5f - q[2] * q[2] - q[4] * q[4]) + 2 * b[4] * (q[1] * q[2] + q[3] * q[4]) - m[3];
            f[5, 0] = 2 * b[2] * (q[1] * q[3] + q[2] * q[4]) + 2 * b[3] * (q[3] * q[4] - q[1] * q[2]) + 2 * b[4] * (0.5f - q[2] * q[2] - q[3] * q[3]) - m[4];

            var JT = new Matrix(4, 6);
            JT[0, 0] = -2 * q[3];
            JT[0, 1] = 2 * q[2];
            JT[0, 2] = 0;
            JT[0, 3] = -2 * b[3] * q[4];
            JT[0, 4] = -2 * b[2] * q[4] + 2 * b[4] * q[2];
            JT[0, 5] = 2 * b[2] * q[3];

            JT[1, 0] = 2 * q[4];
            JT[1, 1] = 2 * q[1];
            JT[1, 2] = -4 * q[2];
            JT[1, 3] = 2 * b[4] * q[4];
            JT[1, 4] = 2 * b[2] * q[3] + 2 * b[4] * q[1];
            JT[1, 5] = 2 * b[2] * q[4] - 4 * b[4] * q[2];

            JT[2, 0] = -2 * q[1];
            JT[2, 1] = 2 * q[4];
            JT[2, 2] = -4 * q[3];
            JT[2, 3] = -4 * b[2] * q[3] - 2 * b[4] * q[1];
            JT[2, 4] = 2 * b[2] * q[2] + 2 * b[4] * q[4];
            JT[2, 5] = 2 * b[2] * q[1] - 4 * b[4] * q[3];

            JT[3, 0] = 2 * q[2];
            JT[3, 1] = 2 * q[3];
            JT[3, 2] = 0;
            JT[3, 3] = -4 * b[2] * q[4] + 2 * b[4] * q[2];
            JT[3, 4] = -2 * b[2] * q[1] + 2 * b[4] * q[3];
            JT[3, 5] = 2 * b[2] * q[2];

            Matrix grad = JT * f;

            return new Quaternion(grad[0, 0], grad[1, 0], grad[2, 0], grad[3, 0]);
        }

        private static Quaternion GradF(Quaternion q, Quaternion a)
        {
            var f = new Matrix(3, 1);
            f[0, 0] = 2.0f * (q[2] * q[4] - q[1] * q[3]) - a.X;
            f[1, 0] = 2.0f * (q[1] * q[2] + q[3] * q[4]) - a.Y;
            f[2, 0] = 2.0f * (0.5f - q[2] * q[2] - q[3] * q[3]) - a.Z;

            var JT = new Matrix(4, 3);
            JT[0, 0] = -2.0f * q[3];
            JT[0, 1] = 2.0f * q[2];
            JT[0, 2] = 0.0f;

            JT[1, 0] = 2.0f * q[4];
            JT[1, 1] = 2.0f * q[1];
            JT[1, 2] = -4.0f * q[3];

            JT[2, 0] = -2.0f * q[1];
            JT[2, 1] = 2.0f * q[4];
            JT[2, 2] = -4.0f * q[3];

            JT[3, 0] = 2.0f * q[2];
            JT[3, 1] = 2.0f * q[3];
            JT[3, 2] = 0.0f;

            Matrix grad = JT * f;

            return new Quaternion(grad[0, 0], grad[1, 0], grad[2, 0], grad[3, 0]);
        }

        public static float[] Integrate(float[] previous, float[] current, float[] condition, float dt)
        {
            float[] result = new float[3];
            for (int i = 0; i < 3; i++)
                result[i] = condition[i] + (current[i] + previous[i]) * 0.5f * dt;

            return result;
        }

        /*public static Quaternion Integrate(Quaternion previous, Quaternion current, Quaternion condition, float dt)
        {
            var result = new Quaternion();
            for (int i = 1; i < 4; i++)
                result[i] = condition[i] + (current[i] + previous[i]) * 0.5f * dt;

            return result;
        }

        public static (float x,float y,float z) Integrate((float x, float y, float z) previous, (float x, float y, float z) current, (float x, float y, float z) condition, float dt)
        {
            (float x, float y, float z) result;
            result.x = condition.x + (current.x + previous.x) * 0.5f * dt;
            result.y = condition.y + (current.y + previous.y) * 0.5f * dt;
            result.z = condition.z + (current.z + previous.z) * 0.5f * dt;

            return result;
        }*/
    }
}