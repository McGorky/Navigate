using System;

namespace Mirea.Snar2017.Navigate
{
    // TODO: калибровка акселерометра
    // TODO: возможность опционального включения компенсации дрейфа нуля гироскопа
    // TODO: возможность опционального включения учета показаний магнитометра
    // TODO: фильтр для определения смещения по акселерометру
    public static class Filter
    {
        // TODO: поменять на калмана
        public static float[] Exponential(float[] previous, float[] current, float k1, float k2)
        {
            float[] result = new float[3];
            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(current[i] - previous[i]) < k2)
                {
                    continue;
                }

                result[i] = k1 * current[i] + (1 - k1) * previous[i];
            }

            return result;
        }

        public static (float psi, float theta, float phi) Madgvic(ref Quaternion q, Quaternion s, Quaternion a, Quaternion m, float beta, float dt)
        {
            /*var h = qPrev.Normalized().Multiply(m.Normalized()).Multiply(qPrev.Normalized().Inversed());
            var b = new Quaternion(0, (float)Math.Sqrt(h[2] * h[2] + h[3] * h[3]), 0, h[4]);

            var g = GradF(qPrev.Normalized(), a.Normalized(), b.Normalized(), m.Normalized()).Normalized();
            var qd = qPrev.Multiply(so).Multiply(0.5f).Add(g.Mul(-beta));
            var q = qPrev.Normalized().Add(qd.Mul(deltaTime));
            qPrev = q;*/

            var h = q.Normalized() * m.Normalized() * q.Normalized().Inversed();
            var b = new Quaternion(0, (float)Math.Sqrt(h[2] * h[2] + h[3] * h[3]), 0, h[4]);

            var g = GradF(q.Normalized(), a.Normalized(), b.Normalized(), m.Normalized()).Normalized();
            var qd = 0.5f * q * s - beta * g;
            q = q.Normalized() + qd * dt;

            var psi = (float)Math.Atan2(2 * q[2] * q[3] - 2 * q[1] * q[4], 2 * q[1] * q[1] + 2 * q[2] * q[2] - 1);
            var theta = (float)-Math.Asin(2 * q[2] * q[4] + 2 * q[1] * q[3]);
            var phi = (float)Math.Atan2(2 * q[3] * q[4] - 2 * q[1] * q[2], 2 * q[1] * q[1] + 2 * q[4] * q[4] - 1);

            return (psi, theta, phi);
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

            Matrix g = JT * f;

            return new Quaternion(g[0, 0], g[1, 0], g[2, 0], g[3, 0]);
        }
    }
}