﻿using System.Text;
using System;
using System.Threading;
using System.Globalization;

namespace Mirea.Snar2017.Navigate
{
    public class Matrix
    {
        private float[][] matrix;

        public int Rows { get; }
        public int Columns { get; }

        public float this[int row, int column]
        {
            get => matrix[row][column];
            set => matrix[row][column] = value;
        }

        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;

            matrix = new float[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                matrix[i] = new float[Columns];
            }
        }

        public Matrix(int rows, int columns, MatrixInitializationValue initializationValue)
            : this(rows, columns)
        {
            switch (initializationValue)
            {
                case MatrixInitializationValue.Identity:
                {
                    if (rows != columns)
                        throw new ArgumentException($"Number of {nameof(rows)} are not equal to number of {nameof(columns)}\nMatrix should be square to be identity matrix");

                    for (int i = 0; i < Rows; i++)
                        matrix[i][i] = 1;

                    return;
                }
                case MatrixInitializationValue.Zeros:
                {
                    return;
                }
                case MatrixInitializationValue.Ones:
                {
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            matrix[i][j] = 1;
                        }
                    }
                    return;
                }
                default:
                    throw new ArgumentException();
            }
        }

        #region Operators and casts
        public static Matrix operator *(Matrix a, Matrix b)
        {
            // TODO: проверка размеров
            var output = new Matrix(a.Rows, b.Columns);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < b.Columns; j++)
                {
                    for (int k = 0; k < a.Columns; k++)
                    {
                        output[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return output;
        }

        public static Matrix operator *(Matrix a, float scalar)
        {
            var output = new Matrix(a.Rows, a.Columns);
            for (int i = 0; i < output.Rows; i++)
            {
                for (int j = 0; j < output.Columns; j++)
                {
                    output[i, j] = a[i, j] * scalar;
                }
            }
            return output;
        }

        public static Matrix operator *(float scalar, Matrix a) => a * scalar;

        public static Matrix operator /(Matrix a, float scalar) => a * (1.0f / scalar);

        public static Matrix operator +(Matrix a, Matrix b)
        {
            // TODO: проверка размеров
            throw new NotImplementedException();
        }
        #endregion

        public Matrix Transposed()
        {
            var output = new Matrix(Columns, Rows);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    output.matrix[j][i] = matrix[i][j];
                }
            }

            return output;
        }

        public Matrix Inversed()
        {
            if (Rows != Columns)
                throw new InvalidOperationException();

            var matrix = new float[Rows][];
            for (int i = 0; i < Rows; i++)
                matrix[i] = new float[Columns];
            this.matrix.CopyTo(matrix, 0);

            var result = new Matrix(Rows, Columns);
            ref float[][] identity = ref result.matrix;
            for (int i = 0; i < Rows; i++)
                identity[i][i] = 1;

            void SwapRows(float[][] a, float[][] b, int i1, int i2)
            {
                float[] buf1 = a[i1];
                float[] buf2 = b[i1];

                a[i1] = a[i2];
                b[i1] = b[i2];

                a[i2] = buf1;
                b[i2] = buf2;
            }
            
            for (int i = 0; i < Columns; i++) // Прямой ход
            {
                if (matrix[i][i] == 0)
                {
                    for (int j = i + 1; j < Rows; j++)
                    {
                        if (matrix[j][i] != 0)
                        {
                            SwapRows(matrix, identity, i, j);
                            break;
                        }
                    }
                }

                float tmp = matrix[i][i];
                for (int j = 0; j < Columns; j++) // Делим строку на эл-т, стоящий на главной диагонали этой строки
                {
                    matrix[i][j] /= tmp;
                    identity[i][j] /= tmp;
                }

                for (int j = i + 1; j < Rows; j++) // Вычитаем из каждой нижней строки i-ую строку, умноженную на первый эл-т нижней строки
                {
                    tmp = matrix[j][i];
                    for (int k = 0; k < Columns; k++)
                    {
                        matrix[j][k] -= tmp * matrix[i][k];
                        identity[j][k] -= tmp * identity[i][k];
                    }
                }
            }

            for (int i = Columns - 1; i > 0; i--) // Обратный ход
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    float tmp = matrix[j][i];
                    for (int k = 0; k < Columns; k++)
                    {
                        matrix[j][k] -= tmp * matrix[i][k];
                        identity[j][k] -= tmp * identity[i][k];
                    }
                }
            }

            return result;
        }

        public override string ToString()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var output = new StringBuilder(Rows * Columns * 7);

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    output.Append($"{this[i, j]}" + (j < Columns - 1 ? "," : ""));
                }
                output.Append(i < Rows - 1 ? "\n" : "");
            }

            return output.ToString();
        }
    }
}