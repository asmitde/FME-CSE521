using System;
using System.IO;
using System.Linq;

namespace FME
{
    class Program
    {
        private static double[][] matrix_A;
        private static double[][] vector_b;
        private static double[][] reducedMatrix_Ab;

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
            DisplayMatrix(matrix_A);
            Console.WriteLine("\nVector b:");
            DisplayMatrix(vector_b);

            /* Perform FME */
            reducedMatrix_Ab = DoFME(matrix_A, vector_b);

            /* Display FME reduced compound matrix [A|b]*/
            Console.WriteLine("\nFME Reduced Compound Matrix [A|b]:");
            DisplayMatrix(reducedMatrix_Ab);
        }

        private static double[][] DoFME(double[][] mat_A, double[][] vec_b)
        {
            /* Get matrix A dimension */
            int m = mat_A.GetLength(0);
            int n = mat_A[0].GetLength(0);

            /* Create new compound matrix [A|b]*/
            double[][] mat_Ab = new double[m][];

            /* Copy values from A and b */
            for (int i = 0; i < m; i++)
            {
                mat_Ab[i] = new double[n + 1];
                for (int j = 0; j < n; j++)
                {
                    mat_Ab[i][j] = mat_A[i][j];
                }

                mat_Ab[i][n] = vec_b[i][0];
            }

            /* Divide matrix based on the upper bounds, lower bounds, and zeroes */
            double[][] temp_Ab = mat_Ab.OrderBy(i => i[0]).ToArray();

            mat_Ab = (double[][])temp_Ab.Clone();
            return mat_Ab;
        }

        private static void DisplayMatrix(double[][] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix[i].GetLength(0); j++)
                {
                    Console.Write($"{matrix[i][j]}\t");
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

                matrix_A = new double[dim_n][];
                vector_b = new double[dim_n][];

                matrix_A[0] = new double[dim_n];
                for (int j = 0; j < dim_n; j++)
                {
                    matrix_A[0][j] = Convert.ToDouble(num_str[j]);
                }
                
                for (int i = 1; i < dim_n; i++)
                {
                    line = sr.ReadLine();
                    if (line == null)
                    {
                        throw new Exception("Invalid file content.");
                    }

                    num_str = line.Split();
                    matrix_A[i] = new double[dim_n];
                    for (int j = 0; j < dim_n; j++)
                    {
                        matrix_A[i][j] = Convert.ToDouble(num_str[j]);
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
                    vector_b[i] = new double[1];
                    vector_b[i][0] = Convert.ToDouble(num_str[0]);
                }
            }
        }
    }
}
