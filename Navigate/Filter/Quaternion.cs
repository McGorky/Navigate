using System;
using static System.Math;

namespace Mirea.Snar2017.Navigate
{
    public struct Quaternion
    {
        public float w, x, y, z;

        /// <summary>
        /// Нумерация идет с 1, что сделано для удобства работы с литературой
        /// </summary>
        /// <param name="index">От 1 до 4</param>
        /// <returns></returns>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 1:
                        return w;
                    case 2:
                        return x;
                    case 3:
                        return y;
                    case 4:
                        return z;
                    default:
                        throw new ArgumentException($"{nameof(index)} должен быть в пределах от 1 до 4");
                }
            }
            set
            {
                switch (index)
                {
                    case 1:
                    {
                        w = value;
                        return;
                    }
                    case 2:
                    {
                        x = value;
                        return;
                    }
                    case 3:
                    {
                        y = value;
                        return;
                    }
                    case 4:
                    {
                        z = value;
                        return;
                    }
                    default:
                        throw new ArgumentException($"{nameof(index)} должен быть в пределах от 1 до 4");
                }
            }
        }

        /*public float W { get => w; set => w = value; }
        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public float Z { get => z; set => z = value; }*/

        public Quaternion(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        #region Operators and casts
        // TODO: проверить правильность
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z,
                a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
                a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
                a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w);
        }

        public static Quaternion operator *(Quaternion q, float scalar) => new Quaternion(q.w * scalar, q.x * scalar, q.y * scalar, q.z * scalar);
        public static Quaternion operator *(float scalar, Quaternion q) => q * scalar;

        public static Quaternion operator +(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.w + b.w,
                a.x + b.x,
                a.y + b.y,
                a.z + b.z);
        }
        public static Quaternion operator -(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.w - b.w,
                a.x - b.x,
                a.y - b.y,
                a.z - b.z);
        }
        
        public static implicit operator Quaternion((float w, float x, float y, float z) tuple)
        {
            return new Quaternion(tuple.w, tuple.x, tuple.y, tuple.z);
        }

        public void Deconstruct(out float w, out float x, out float y, out float z)
        {
            w = this.w;
            x = this.x;
            y = this.y;
            z = this.z;
        }
        #endregion

        public Quaternion Normalized()
        {
            float norm = (float)Sqrt(w*w + x*x + y*y + z*z);
            return this * (1 / norm);
        }

        public Quaternion Conjugated() => new Quaternion(w, -x, -y, -z);

        public override string ToString() => $"{w},{x},{y},{z}";
    }
}