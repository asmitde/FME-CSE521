using System;
using System.IO;

namespace FME
{
    class Program
    {
        private static double[,] matrix_A;
        private static double[,] vector_b;
        private static double[,] reducedMatrix_Ab;

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
            DoFME();

            /* Display FME reduced compound matrix [A|b]*/
            Console.WriteLine("\nFME Reduced Compound Matrix [A|b]:");
            DisplayMatrix(reducedMatrix_Ab);
        }

        private static void DoFME()
        {
            /* Create new compound matrix [A|b]*/
            reducedMatrix_Ab = new double[matrix_A.GetLength(0), matrix_A.GetLength(1) + 1];

            /* Copy values from A and b */
            for (int i = 0; i < reducedMatrix_Ab.GetLength(0); i++)
            {
                for (int j = 0; j < reducedMatrix_Ab.GetLength(1) - 1; j++)
                {
                    reducedMatrix_Ab[i, j] = matrix_A[i, j];
                }

                reducedMatrix_Ab[i, reducedMatrix_Ab.GetLength(1) - 1] = vector_b[i, 0];
            }
        }

        private static void DisplayMatrix(double[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
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

                matrix_A = new double[dim_n, dim_n];
                vector_b = new double[dim_n, 1];

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
