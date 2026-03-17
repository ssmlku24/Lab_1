using System;
using System.Collections.Generic;
using System.IO;

namespace JordanExceptionsLab
{
    class Program
    {
        static List<string> currentProtocol = new List<string>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool running = true;

            while (running)
            {
                currentProtocol.Clear();
                Console.WriteLine("\n=== ЛАБОРАТОРНА РОБОТА: ЗЖВ ===");
                Console.WriteLine("1. Пошук оберненої матриці");
                Console.WriteLine("2. Обчислення рангу матриці");
                Console.WriteLine("3. Розв'язання СЛАР");
                Console.WriteLine("0. Вихід");
                Console.Write("Оберіть дію: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        FindInverseMatrixMenu();
                        break;
                    case "2":
                        CalculateRankMenu();
                        break;
                    case "3":
                        SolveSLAEMenu();
                        break;
                    case "0":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                        break;
                }
            }
        }

        static void Log(string message, bool writeLine = true)
        {
            if (writeLine)
            {
                Console.WriteLine(message);
                currentProtocol.Add(message);
            }
            else
            {
                Console.Write(message);
                if (currentProtocol.Count == 0) currentProtocol.Add("");
                currentProtocol[currentProtocol.Count - 1] += message;
            }
        }

        static void LogMatrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                string rowStr = "";
                for (int j = 0; j < cols; j++)
                {
                    rowStr += $"{matrix[i, j],6:F2} ";
                }
                Log(rowStr);
            }
            Log("");
        }

        static void PromptSaveToFile()
        {
            Console.Write("\nБажаєте зберегти цей протокол у текстовий файл? (т/н): ");
            string answer = Console.ReadLine()?.Trim().ToLower();
            if (answer == "т" || answer == "y" || answer == "так")
            {
                string filename = $"Protocol_ZHV_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                File.WriteAllLines(filename, currentProtocol);
                Console.WriteLine($"\nФайл успішно збережено у папку з програмою під назвою: {filename}");
            }
        }

        static void FindInverseMatrixMenu()
        {
            Console.Write("Введіть розмірність квадратної матриці n: ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0)
            {
                Console.WriteLine("Некоректна розмірність.");
                return;
            }

            double[,] A = ReadMatrix(n, n);

            Log("\nЗнаходження оберненої матриці:");
            Log("Вхідна матриця:");
            LogMatrix(A);

            double[,] inverse = GetInverseMatrix(A);
            if (inverse != null)
            {
                Log("Обернена матриця:");
                LogMatrix(inverse);
            }
            PromptSaveToFile();
        }

        static void CalculateRankMenu()
        {
            Console.Write("Введіть кількість рядків n: ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0) return;

            Console.Write("Введіть кількість стовпців m: ");
            if (!int.TryParse(Console.ReadLine(), out int m) || m <= 0) return;

            double[,] A = ReadMatrix(n, m);

            Log("\nОбчислення рангу матриці:");
            Log("Вхідна матриця:");
            LogMatrix(A);

            int rank = CalculateRank(A);
            Log($"\nОстаточний ранг матриці: r = {rank}");

            PromptSaveToFile();
        }

        static void SolveSLAEMenu()
        {
            Console.Write("Введіть розмірність системи n: ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0) return;

            Console.WriteLine("Введіть матрицю коефіцієнтів A:");
            double[,] A = ReadMatrix(n, n);

            Console.WriteLine("Введіть вектор вільних членів B (через пробіл в один рядок або по одному):");
            double[] B = ReadVector(n);

            Log("\nЗгенерований протокол обчислення:");
            Log("Знаходження розвʼязків СЛАР 1-м методом (за допомогою оберненої матриці):");
            Log("Знаходження оберненої матриці:");
            Log("Вхідна матриця:");
            LogMatrix(A);

            double[,] invA = GetInverseMatrix(A);

            if (invA == null)
            {
                Log("Систему неможливо розв'язати цим методом (матриця вироджена).");
                PromptSaveToFile();
                return;
            }

            Log("Обернена матриця:");
            LogMatrix(invA);

            Log("Вхідний вектор В:");
            for (int i = 0; i < n; i++)
                Log($"{B[i]}");
            Log("");

            Log("Обчислення розвʼязків:");
            double[] X = new double[n];
            for (int i = 0; i < n; i++)
            {
                string equation = $"X[{i + 1}] = ";
                double sum = 0;
                for (int j = 0; j < n; j++)
                {
                    double valB = B[j];
                    double valInv = invA[i, j];
                    sum += valB * valInv;

                    string formattedInv = valInv < 0 ? $"({valInv:F2})" : $"{valInv:F2}";
                    equation += $"{valB:F2} * {formattedInv}";

                    if (j < n - 1) equation += " + ";
                }
                X[i] = sum;
                equation += $" = {X[i]:F2}";
                Log(equation);
            }
            PromptSaveToFile();
        }

        static double[,] DoJordanEliminationStep(double[,] matrix, int r, int s)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] next = new double[rows, cols];
            double pivot = matrix[r, s];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i == r && j == s)
                        next[i, j] = 1.0;
                    else if (i == r)
                        next[i, j] = -matrix[r, j];
                    else if (j == s)
                        next[i, j] = matrix[i, s];
                    else
                        next[i, j] = matrix[i, j] * pivot - matrix[i, s] * matrix[r, j];

                    next[i, j] /= pivot;
                }
            }
            return next;
        }

        static double[,] GetInverseMatrix(double[,] A)
        {
            int n = A.GetLength(0);
            double[,] currentA = (double[,])A.Clone();

            Queue<int> diagonals = new Queue<int>();
            for (int i = 0; i < n; i++) diagonals.Enqueue(i);

            int step = 1;
            int attempts = 0;

            Log("Протокол обчислення ЗЖВ:");

            while (diagonals.Count > 0 && attempts < diagonals.Count)
            {
                int k = diagonals.Dequeue();

                if (Math.Abs(currentA[k, k]) < 1e-9)
                {
                    diagonals.Enqueue(k);
                    attempts++;
                    continue;
                }
                attempts = 0;

                Log($"Крок #{step}");
                Log($"Розв'язувальний елемент: А[{k + 1}, {k + 1}] = {currentA[k, k]:F2}");

                currentA = DoJordanEliminationStep(currentA, k, k);

                Log("Матриця після виконання ЗЖВ:");
                LogMatrix(currentA);

                step++;
            }

            if (diagonals.Count > 0)
            {
                Log("Матриця вироджена (визначник = 0), оберненої не існує.");
                return null;
            }

            return currentA;
        }

        static int CalculateRank(double[,] A)
        {
            int n = A.GetLength(0);
            int m = A.GetLength(1);
            double[,] currentA = (double[,])A.Clone();

            int r = 0;
            int limit = Math.Min(n, m);

            Log("Протокол обчислення рангу:");

            for (int i = 0; i < limit; i++)
            {
                if (Math.Abs(currentA[i, i]) > 1e-9)
                {
                    Log($"Крок #{r + 1}");
                    Log($"Розв'язувальний елемент: А[{i + 1}, {i + 1}] = {currentA[i, i]:F2}");

                    currentA = DoJordanEliminationStep(currentA, i, i);
                    r++;

                    Log("Матриця після виконання ЗЖВ:");
                    LogMatrix(currentA);
                }
                else
                {
                    Log($"Елемент А[{i + 1}, {i + 1}] дорівнює 0. Пропускаємо цей крок.");
                }
            }
            return r;
        }

        static double[,] ReadMatrix(int rows, int cols)
        {
            double[,] matrix = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                while (true)
                {
                    Console.Write($"Рядок {i + 1}: ");
                    string[] inputs = Console.ReadLine().Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (inputs.Length == cols)
                    {
                        bool validRow = true;
                        for (int j = 0; j < cols; j++)
                        {
                            if (!double.TryParse(inputs[j].Replace('.', ','), out matrix[i, j]))
                            {
                                Console.WriteLine($"Помилка: '{inputs[j]}' не є числом.");
                                validRow = false;
                                break;
                            }
                        }
                        if (validRow) break;
                    }
                    else
                    {
                        Console.WriteLine($"Помилка: потрібно ввести рівно {cols} чисел. Ви ввели {inputs.Length}. Спробуйте ще раз.");
                    }
                }
            }
            return matrix;
        }

        static double[] ReadVector(int n)
        {
            double[] vector = new double[n];
            while (true)
            {
                string[] inputs = Console.ReadLine().Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (inputs.Length == n)
                {
                    bool valid = true;
                    for (int i = 0; i < n; i++)
                    {
                        if (!double.TryParse(inputs[i].Replace('.', ','), out vector[i]))
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (valid) return vector;
                }
                else if (inputs.Length == 1 && double.TryParse(inputs[0].Replace('.', ','), out vector[0]))
                {
                    bool validRest = true;
                    for (int i = 1; i < n; i++)
                    {
                        string nextInput = Console.ReadLine().Trim();
                        if (!double.TryParse(nextInput.Replace('.', ','), out vector[i]))
                        {
                            validRest = false;
                            break;
                        }
                    }
                    if (validRest) return vector;
                    else Console.WriteLine("Помилка під час введення. Почніть введення вектора з початку.");
                }
                else
                {
                    Console.WriteLine($"Помилка: потрібно ввести рівно {n} чисел (через пробіл). Спробуйте ще раз.");
                }
            }
        }
    }
}