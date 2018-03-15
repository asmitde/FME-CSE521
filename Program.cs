using System;
using System.IO;
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
            Matrix<double> new_Ab = Matrix<double>.Build.Dense(nUpperBounds * nLowerBounds + nExcludes, n);

            /* Iterate through all the upper bound rows and add each to all lower bound rows */
            for (int i = temp_Ab.RowCount - nUpperBounds; i < temp_Ab.RowCount; i++)
            {
                for (int j = 0; j < nLowerBounds; j++)
                {
                    
                }
            }

            return temp_Ab;
        }

        private static void DisplayMatrix(Matrix<double> matrix)
        {
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    Console.Write($"{matrix[i, j]}\t");
                }
                Console.WriteLine();
            }
        }

        private static void ReadMatrixDataFromFile(string inputfile)
        {
            if (!File.Exists(inputfile))
            {
                throw new FileNotFoundException();
            }

            using (StreamReader sr = File.OpenText(inputfile))
            {
                string line = sr.ReadLine();
                if (line == null)
                {
                    throw new Exception("Invalid file content.");
                }

                string[] num_str = line.Split();
                int dim_n = num_str.Length;

                matrix_A = Matrix<double>.Build.Dense(dim_n, dim_n);
                vector_b = Matrix<double>.Build.Dense(dim_n, 1);

                for (int j = 0; j < dim_n; j++)
                {
                    matrix_A[0, j] = Convert.ToDouble(num_str[j]);
                }
                
                for (int i = 1; i < dim_n; i++)
                {
                    line = sr.ReadLine();
                    if (line == null)
                    {
                        throw new Exception("Invalid file content.");
                    }

                    num_str = line.Split();
                    for (int j = 0; j < dim_n; j++)
                    {
                        matrix_A[i, j] = Convert.ToDouble(num_str[j]);
                    }
                }

                for (int i = 0; i < dim_n; i++)
                {
                    line = sr.ReadLine();
                    if (line == null)
                    {
                        throw new Exception("Invalid file content.");
                    }

                    num_str = line.Split();
                    vector_b[i, 0] = Convert.ToDouble(num_str[0]);
                }
            }
        }
    }
}
