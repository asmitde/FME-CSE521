using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace FME
{
    class Program
    {
        private static Matrix<double> matrix_A;
        private static Matrix<double> vector_b;
        private static Matrix<double> reducedMatrix_Ab;

        static void Main(string[] args)
        {
            Console.WriteLine("**** FOURIER-MOTZKIN ELIMINATION ****\n");

            /* Validate program usage format */
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:\n $ FME <input file> <output file>");
                System.Environment.Exit(1);
            }

            /* Open input file and populate matrices A and b */
            try
            {
                ReadMatrixDataFromFile(args[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {args[0]} - {ex.Message}");
                System.Environment.Exit(1);
            }

            /* Display Matrix data for A and b */
            Console.WriteLine("\nMatrix A:");
            Console.WriteLine(matrix_A.ToMatrixString());
            Console.WriteLine("\nVector b:");
            Console.WriteLine(vector_b.ToMatrixString());

            /* Perform FME */
            reducedMatrix_Ab = DoFME(matrix_A, vector_b);

            /* Display FME reduced compound matrix [A|b]*/
            Console.WriteLine("\nFME Reduced Compound Matrix [A|b]:");
            Console.WriteLine(reducedMatrix_Ab.ToMatrixString());
        }

        private static Matrix<double> DoFME(Matrix<double> mat_A, Matrix<double> vec_b)
        {
            /* Get matrix A dimension */
            int m = mat_A.RowCount;
            int n = mat_A.ColumnCount;

            /* Create compound matrix [A|b]*/
            Matrix<double> temp_Ab = Matrix<double>.Build.Dense(m, n + 1);
            mat_A.Append(vec_b, temp_Ab);

        #if false
            Console.WriteLine("\ntemp_Ab:");
            Console.WriteLine(temp_Ab.ToMatrixString());
        #endif

            /* Divide matrix based on the upper bounds, lower bounds, and zeroes */
            Vector<double> column0 = temp_Ab.Column(0);
            int[] permutation = Enumerable.Range(0, column0.Count).ToArray();
            Array.Sort(column0.ToArray(), permutation);
            temp_Ab.PermuteRows(new Permutation(permutation));

            int nLowerBounds = column0.Where(i => i < 0).Count();
            int nUpperBounds = column0.Where(i => i > 0).Count();
            int nExcludes = column0.Where(i => i == 0).Count();

            /* Normalize based on first column absolute value */
            for (int i = 0; i < nLowerBounds; i++)
            {
                double i0Val = Math.Abs(temp_Ab[i, 0]);
                for (int j = 0; j < temp_Ab.ColumnCount; j++)
                {
                    temp_Ab[i, j] = temp_Ab[i, j] / i0Val;
                }
            }
 
            for (int i = temp_Ab.RowCount - nUpperBounds; i < temp_Ab.RowCount; i++)
            {
                double i0Val = temp_Ab[i, 0];
                for (int j = 0; j < temp_Ab.ColumnCount; j++)
                {
                    temp_Ab[i, j] = temp_Ab[i, j] / i0Val;
                }
            }

            /* Create new compound matrix [A|b] from previous by projecting one variable */
            int new_m = nUpperBounds * nLowerBounds + nExcludes;
            int new_n = n - 1;
            if (new_m != 0)
            {
                Matrix<double> new_Ab = Matrix<double>.Build.Dense(new_m, new_n);

                /* Iterate through all the upper bound rows and add each to all lower bound rows */
                int rowIndex = 0;
                for (int i = temp_Ab.RowCount - nUpperBounds; i < temp_Ab.RowCount; i++)
                {
                    for (int j = 0; j < nLowerBounds; j++)
                    {
                        new_Ab.SetRow(rowIndex++, temp_Ab.Row(i, 1, new_n) + temp_Ab.Row(j, 1, new_n));
                    }
                }

                /* Add the exclude rows */
                new_Ab.SetSubMatrix(new_m - nExcludes, 0, temp_Ab.SubMatrix(nLowerBounds, nExcludes, 1, new_n));

                /* Obtain new matrix A and vector b */
                Matrix<double> new_A = new_Ab.RemoveColumn(new_n);
                Matrix<double> new_b = new_Ab.Column(new_n).ToColumnMatrix();
            }

            return temp_Ab;
        }

        private static void ReadMatrixDataFromFile(string inputfile)
        {
            if (!File.Exists(inputfile))
            {
                throw new FileNotFoundException();
            }

            using (StreamReader sr = File.OpenText(inputfile))
            {
                /* Read line 1: value of m and n */
                string line = sr.ReadLine();
                if (line == null)
                {
                    throw new Exception("Invalid file content.");
                }

                string[] num_str = line.Split().Where(s => s != string.Empty).ToArray();
                int m = Convert.ToUInt16(num_str[0]);
                int n = Convert.ToUInt16(num_str[1]);

                matrix_A = Matrix<double>.Build.Dense(m, n);
                vector_b = Matrix<double>.Build.Dense(m, 1);

                /* Read next m lines: each row of the matrix A (m X n) */
                for (int i = 0; i < m; i++)
                {
                    line = sr.ReadLine();
                    if (line == null)
                    {
                        throw new Exception("Invalid file content.");
                    }

                    num_str = line.Split().Where(s => s != string.Empty).ToArray();
                    for (int j = 0; j < n; j++)
                    {
                        matrix_A[i, j] = Convert.ToDouble(num_str[j]);
                    }
                }

                /* Read next n lines: the vector b (m X 1) */
                for (int i = 0; i < m; i++)
                {
                    line = sr.ReadLine();
                    if (line == null)
                    {
                        throw new Exception("Invalid file content.");
                    }

                    num_str = line.Split().Where(s => s != string.Empty).ToArray();
                    vector_b[i, 0] = Convert.ToDouble(num_str[0]);
                }
            }
        }
    }
}
