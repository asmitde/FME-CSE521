using System;
using System.Collections.Generic;
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
            List<List<double>> allSolutions = DoFME(matrix_A, vector_b);

            /* Display solutions for vector x */
            PrintAllSolutions(allSolutions);

            /* Write solutions to file */
            WriteSolutionsToFile(args[1], allSolutions);
        }

        private static void PrintAllSolutions(List<List<double>> allSolutions)
        {
            if (allSolutions == null)
            {
                Console.WriteLine("No finite solution exists.");

                return;
            }

            Console.WriteLine("\nSolutions to Vector x (each row represents a solution vector):\n");

            foreach (var solution in allSolutions)
            {
                foreach (var solutionVector in solution)
                {
                    Console.Write(solutionVector + "\t");
                }

                Console.WriteLine();
            }
        }

        private static List<List<double>> DoFME(Matrix<double> mat_A, Matrix<double> vec_b)
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
            int[] permutationKey = Enumerable.Range(0, column0.Count).ToArray();
            Array.Sort(column0.ToArray(), permutationKey);
            int[] permutation = Enumerable.Range(0, column0.Count).ToArray();
            Array.Sort(permutationKey, permutation);
            temp_Ab.PermuteRows(new Permutation(permutation));

        #if true
            Console.WriteLine("\nSorted temp_Ab:");
            Console.WriteLine(temp_Ab.ToMatrixString());
        #endif

            int nLowerBounds = column0.Where(i => i < 0).Count();
            int nUpperBounds = column0.Where(i => i > 0).Count();
            int nExcludes = column0.Where(i => i == 0).Count();

            /* Return null if there are no upper or lower bounds - no closed solution space exists */
            if (nLowerBounds == 0 || nUpperBounds == 0)
            {
                return null;
            }

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

        #if true
            Console.WriteLine("\nnormalized temp_Ab:");
            Console.WriteLine(temp_Ab.ToMatrixString());
        #endif

            /* If last variable, then find integer upper and lower bounds */
            if (temp_Ab.ColumnCount == 2)
            {

                int lowerBound = (int) ((-1) * Math.Floor(temp_Ab.Column(temp_Ab.ColumnCount - 1, 0, nLowerBounds).Minimum())); // Minimum, since coeffs are negative
                int upperBound = (int) Math.Floor(temp_Ab.Column(temp_Ab.ColumnCount - 1, temp_Ab.RowCount - nUpperBounds, nUpperBounds).Minimum());

                List<List<double>> firstSolution = new List<List<double>>();
                for (int i = lowerBound; i <= upperBound; i++)
                {
                    firstSolution.Add(new List<double>(){i});
                }

                return firstSolution;
            }

            /* Create new compound matrix [A|b] from previous by projecting one variable */
            int new_m = nUpperBounds * nLowerBounds + nExcludes;
            int new_n = temp_Ab.ColumnCount - 1;

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
            if (nExcludes > 0)
            {
                new_Ab.SetSubMatrix(new_m - nExcludes, 0, temp_Ab.SubMatrix(nLowerBounds, nExcludes, 1, new_n));
            }

            /* Obtain new matrix A and vector b */
            Matrix<double> new_A = new_Ab.RemoveColumn(new_n - 1);
            Matrix<double> new_b = new_Ab.Column(new_n - 1).ToColumnMatrix();

        #if true
            Console.WriteLine("\nnew_A:");
            Console.WriteLine(new_A.ToMatrixString());
            Console.WriteLine("\nnew_b:");
            Console.WriteLine(new_b.ToMatrixString());
        #endif

            /* Recursively call FME with the projected matrix and receive a list of solution vectors */
            List<List<double>> solutions = DoFME(new_A, new_b);

            if (solutions == null)
            {
                return null;
            }           

            /* Calculate new bounds on current variable */
            int n_sols = solutions.Count;
            int variablesSolved = solutions[0].Count;

            Matrix<double> mat_x = Matrix<double>.Build.Dense(variablesSolved, n_sols);
            for (int col = 0; col < solutions.Count; col++)
            {
                mat_x.SetColumn(col, solutions[col].ToArray());
            }

        #if true
            Console.WriteLine("\nmat_x:");
            Console.WriteLine(mat_x.ToMatrixString());
        #endif

            Matrix<double> mat_Atimesx = temp_Ab.SubMatrix(0, temp_Ab.RowCount, 1, variablesSolved) * mat_x;

        #if true
            Console.WriteLine("\nmat_Atimesx:");
            Console.WriteLine(mat_Atimesx.ToMatrixString());
        #endif

            /* Iterate over all the existing solution vectors and create new solution vectors by adding new variable solutions */
            List<List<double>> newSolutions = new List<List<double>>();
            for (int col = 0; col < n_sols; col++)
            {
                Vector<double> preBoundsList = temp_Ab.Column(temp_Ab.ColumnCount - 1) - mat_Atimesx.Column(col);

                int lowerBound = (int) ((-1) * Math.Floor(preBoundsList.SubVector(0, nLowerBounds).Minimum())); // Minimum, since coeffs are negative
                int upperBound = (int) Math.Floor(preBoundsList.SubVector(temp_Ab.RowCount - nUpperBounds, nUpperBounds).Minimum());

                /* Add new variable solution to the previous solution vector */
                for (int i = lowerBound; i <= upperBound; i++)
                {
                    List<double> newSolutionVector = new List<double>(solutions[col]);
                    newSolutionVector.Insert(0, i);

                    newSolutions.Add(newSolutionVector);
                }
            }

            return newSolutions;
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

        
        private static void WriteSolutionsToFile(string outputfile, List<List<double>> allSolutions)
        {
            using (StreamWriter sw = new StreamWriter(outputfile))
            {
                if (allSolutions == null)
                {
                    sw.WriteLine("No finite solution exists.");

                    return;
                }

                sw.WriteLine("\nSolutions to Vector x (each row represents a solution vector):\n");

                foreach (var solution in allSolutions)
                {
                    foreach (var solutionVector in solution)
                    {
                        sw.Write(solutionVector + "\t");
                    }

                    sw.WriteLine();
                }
            }
        }
    }
}
