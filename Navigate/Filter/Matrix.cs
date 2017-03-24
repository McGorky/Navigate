using System.Text;

namespace Mirea.Snar2017.Navigate
{
    public class Matrix
    {
        private float[,] matrix;

        public int Rows { get; }
        public int Columns { get; }

        public float this[int row, int column]
        {
            get => matrix[row, column];
            set => matrix[row, column] = value;
        }

        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;

            matrix = new float[rows, columns];
        }

        public Matrix Transposed()
        {
            var output = new Matrix(Columns, Rows);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    output.matrix[j, i] = matrix[i, j];
                }
            }

            return output;
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
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

        public override string ToString()
        {
            var output = new StringBuilder();

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    output.Append($"{matrix[i, j]}" + (j < Columns - 1 ? " " : ""));
                }
                output.Append(i < Rows - 1 ? "\n" : "");
            }

            return output.ToString();
        }
    }
}