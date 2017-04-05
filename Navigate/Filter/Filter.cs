using System;

namespace Mirea.Snar2017.Navigate
{
    // TODO: возможность опционального включения компенсации дрейфа нуля гироскопа
    // TODO: возможность опционального включения учета показаний магнитометра
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

        public static Quaternion Madgvic(Quaternion q, Quaternion g, Quaternion a, Quaternion m, float beta, float zeta, float dt)
        {
            var h = q * m.Normalized() * q.Conjugated(); // q.Normalized() * m.Normalized() * q.Normalized().Conjugated();
            var b = new Quaternion(0, (float)Math.Sqrt(h[2] * h[2] + h[3] * h[3]), 0, h[4]);

            var gr = GradF(q.Normalized(), a.Normalized(), b.Normalized(), m.Normalized()).Normalized();
            var qd = 0.5f * q * g - beta * gr;
            q = (q.Normalized() + qd * dt).Normalized();

            return q;
        }

        /// <summary>
        /// a = b * d. 
        /// находит d
        /// a и b - единичные кватернионы
        /// </summary>
        /// <param name="a">поворот из положения x в положение y</param>
        /// <param name="b">поворот из положения x в положение z</param>
        /// <returns>поворот из положения y в положение z></returns>
        public static Quaternion CalculateDifferenceQuaternion(Quaternion a, Quaternion b) => b.Conjugated() * a; // b.Normalized().Conjugated() * a.Normalized();

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

            Matrix g = JT * f;

            return new Quaternion(g[0, 0], g[1, 0], g[2, 0], g[3, 0]);
        }

        public static float[] Integrate(float[] previous, float[] current, float[] condition, float dt)
        {
            float[] result = new float[3];
            for (int i = 0; i < 3; i++)
                result[i] = condition[i] + (current[i] + previous[i]) * 0.5f * dt;

            return result;
        }
    }
}