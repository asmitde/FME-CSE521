# CSE521 Project: Fourier-Motzkin Elimination
This program will solve using FME a system of inequalities of the form Ax <= b. It takes as input the coefficient matrix A and the vector b, and will generate all integer solutions for the vector x that satisfy the inequality.

## Prerequisites
The input to the program should be provided in a text file. An example input file `inp.txt` is provided. The program output is printed on screen as well as written to a text file. An example output file `out.txt` is provided. The program binary takes in the input and output file names as command line parameters. The input file must be present and in the correct format. The output file need not be present initially. The program will generate the output file as per the file name provided when running the program.

### Input File format
Example of equations used:
```
3x + 4y >= 16
4x + 7y <= 56
4x - 7y <= 20
2x - 3y >= -9
```
The first line of the file must contain the dimensions of matrix A as two integer numbers m and n, separated by whitespaces. m denotes the number of equations and n denotes the number of variables for the system. For the above system of equations, m is 4 and n is 2.  So the first line of the input file should be
```
4   2
```
In the next m lines (m = # of equations), the matrix A should be entered. The order of the columns in matrix A should be in the order in which the variables are to be eliminated. For the above system of equations, if the order of elimination is x then y, then the next 4 lines should be
```
-3  -4
 4   7
 4  -7
-2   3
```
In the next m lines (m = # of equations), the vector b should be entered. For the above system of equations, the next 4 lines should be
```
-16
 56
 20
  9
```

### Output File format
Example of equations used:
```
3x + 4y >= 16
4x + 7y <= 56
4x - 7y <= 20
2x - 3y >= -9
```
After the program is run with the input file, the output file will be generated containing all the solutions for the solution vector x. The same output will also be displyed on screen. Each line of the output represents one integer solution vector that satisfies the system of inequalities. The order of the numbers of each solution vector is in the order of the elimination as provided in matrix A. If there are no integer solutions, or if there is no finite solution space for the entire system of equations, an error message will be shown as output. Following will be the output file for the above system of equations
```
Solutions to Vector x (each row represents a solution vector):

4	1	
5	1	
6	1	
3	2	
4	2	
5	2	
6	2	
7	2	
8	2	
2	3	
3	3	
4	3	
5	3	
6	3	
7	3	
8	3	
2	4	
3	4	
4	4	
5	4	
6	4	
7	4	
3	5	
4	5	
5	5	
```

## Running the program
The program cam be run using the provided binaries. The package is self-contained, i.e., no extra dependencies should be required to run the binaries. Binaries are provided for Windows x64 and Linux x64.

### Running the program on Windows
The program should run for any version of Windows x64. It is tested to be working on Windows 10 64-bit. To run the program, follow these steps:
1. Navigate to the folder: `FME-CSE521\release\win-x64\`
2. Shift + right click to open the extended context menu and click on Open Command Prompt here (or Open Powershell Window here). You can also open command prompt or powershell and navigate to the above folder.
3. At the prompt, issue the following command:
```
.\FME.exe <input file> <output file>
```

### Running the program on Linux
The program should run for any distribution of Linux x64. It is tested to be working on Ubuntu 16.04 64-bit. To run the program, follow these steps:
1. Navigate to the folder: `FME-CSE521\release\linux-x64\`
2. Open open a shell at the above folder location
3. At the prompt, issue the following command:
```
./FME <input file> <output file>
```

### Compiling and running the program from source
The program can be compiled cross platform on Windows, Linux or macOS using .NET Core. The .NET Core SDK can be downloaded from [here](https://www.microsoft.com/net/download/). To compile the program from source, follow the following steps:
1. Navigate to the source folder: `FME-CSE521\source\`
2. Open open a shell or command prompt at the above folder location.
3. Make sure .NET Core version 2.x is installed
```
dotnet --version
```
4. Issue the following command to resolve all dependencies:
```
dotnet restore
```
5. Issue the following command to build and run the program:
```
dotnet run <input file> <output file>
```
**Note:** This creates a `FME.dll` file that can be run on any platform where the .NET SDK or the .NET CLR (Common Language Runtime) is installed. The `FME.dll` can be run on any platform with the following command: `dotnet FME.dll <input file> <output file>`.

## Technologies used
* Framework: [.NET Core](https://www.microsoft.com/net/) - A cross-platform open-source subset of the Microsoft .NET Framework.
* Language: [C#](https://docs.microsoft.com/en-us/dotnet/csharp/) - An object-oriented strongly-typed multi-paradigm programming language by Microsoft.
* Libraries/Packages:
    1. [Math.NET Numerics](https://numerics.mathdotnet.com/) - A numerical class library used for handling Matrices and Vectors.
    2. [Microsoft.Packaging.Tools.Trimming](https://www.nuget.org/packages/Microsoft.Packaging.Tools.Trimming/1.1.0-preview1-25818-01) - A packaging tool to remove unused dependency files when generating the self-contained binary package.
* Code editor: [Visual Studio Code](https://code.visualstudio.com/) - A cross-platform open-source code editor by Microsoft.

## GitHub source
[https://github.com/asmitde/FME-CSE521.git](https://github.com/asmitde/FME-CSE521.git)
