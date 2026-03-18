using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JordanExceptionsLab
{
    class Program
    {
        // Зберігає всі виведені на екран рядки для подальшого запису у файл
        static List<string> currentProtocol = new List<string>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool running = true;

            while (running)
            {
                currentProtocol.Clear();
                Console.WriteLine("\n=== ЛАБОРАТОРНА РОБОТА:ЗЖВ та МЖВ ===");
                Console.WriteLine("--- ЧАСТИНА А ---");
                Console.WriteLine("1. Пошук оберненої матриці");
                Console.WriteLine("2. Обчислення рангу матриці");
                Console.WriteLine("3. Розв'язання СЛАР");
                Console.WriteLine("--- ЧАСТИНА Б ---");
                Console.WriteLine("4. Розв'язання ЗЛП (Симплекс-метод)");
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
                    case "4":
                        SolveLPPMenu();
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

        // Базові функції логування та виведення
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

        static void LogMatrixLPP(double[,] matrix, string[] rowHeaders, string[] colHeaders)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            string header = "      ";
            for (int j = 0; j < cols; j++) header += $"{colHeaders[j],6} ";
            Log(header);

            for (int i = 0; i < rows; i++)
            {
                string rowStr = $"{rowHeaders[i],4} =";
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
                Console.WriteLine($"\nФайл успішно збережено під назвою: {filename}");
            }
        }
        // Безпечне зчитування матриці з перевіркою коректності вводу
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
                    else Console.WriteLine($"Помилка: потрібно рівно {cols} чисел.");
                }
            }
            return matrix;
        }

        // Безпечне зчитування вектора (підтримує введення в рядок або в стовпчик)
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

        // ЧАСТИНА А: Базові операції ЗЖВ (Обернена матриця, Ранг, СЛАР)

        // Виконує один крок ЗЖВ
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
                    if (i == r && j == s) next[i, j] = 1.0; // Заміна розв'язувального елемента
                    else if (i == r) next[i, j] = -matrix[r, j];// Рядок розв'язувального елемента (зміна знаку)
                    else if (j == s) next[i, j] = matrix[i, s]; // Стовпець розв'язувального елемента (без змін)м
                    else next[i, j] = matrix[i, j] * pivot - matrix[i, s] * matrix[r, j];

                    next[i, j] /= pivot; // Ділення всього на розв'язувальний елемент
                }
            }
            return next;
        }
        // Демонструє процес знаходження оберненої матриці
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
        // Алгоритм обчислення оберненої матриці методом ЗЖВ
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

                // Якщо елемент на діагоналі дорівнює нулю, переносимо його в кінець черги
                if (Math.Abs(currentA[k, k]) < 1e-9)
                {
                    diagonals.Enqueue(k);
                    attempts++;
                    continue;
                }
                attempts = 0;
                Log($"Крок #{step}\nРозв'язувальний елемент: А[{k + 1}, {k + 1}] = {currentA[k, k]:F2}");
                currentA = DoJordanEliminationStep(currentA, k, k);
                Log("Матриця після виконання ЗЖВ:");
                LogMatrix(currentA);
                step++;
            }
            // Якщо залишилися необроблені діагональні елементи, матриця вироджена
            if (diagonals.Count > 0)
            {
                Log("Матриця вироджена (визначник = 0), оберненої не існує.");
                return null;
            }

            return currentA;
        }
        // Демонструє процес обчислення рангу матриці довільного розміру
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
        // Алгоритм обчислення рангу матриці
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
                // Виконуємо крок алгоритму лише для ненульових елементів
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
        // Демонструє розв'язання Системи Лінійних Алгебраїчних Рівнянь (СЛАР)
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
                // Обчислення коренів шляхом множення оберненої матриці на вектор вільних членів
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
        // ЧАСТИНА Б: Симплекс-метод (Задача Лінійного Програмування)
        // Спеціальний крок МЖВ для Симплекс-методу 
        static double[,] DoSimplexMJEStep(double[,] matrix, int r, int s)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] next = new double[rows, cols];
            double pivot = matrix[r, s];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i == r && j == s) next[i, j] = 1.0;
                    else if (i == r) next[i, j] = matrix[r, j]; // Рядок без змін
                    else if (j == s) next[i, j] = -matrix[i, s]; // Стовпець змінює знак
                    else next[i, j] = matrix[i, j] * pivot - matrix[i, s] * matrix[r, j];

                    next[i, j] /= pivot;
                }
            }
            return next;
        }
        // Допоміжний метод для заміни заголовків (згідно правила: стовпець отримує мінус)
        static void SwapLPPHeaders(ref string rowHeader, ref string colHeader)
        {
            string oldCol = colHeader;
            colHeader = "-" + rowHeader;
            rowHeader = oldCol.Replace("-", "");
        }
        static void PrintSolutionX(double[,] table, string[] rowHeaders, int n)
        {
            double[] X = new double[n];
            for (int j = 0; j < n; j++)
            {
                string varName = $"x{j + 1}";
                X[j] = 0; // За замовчуванням 0 (якщо змінна неопорна)
                for (int i = 0; i < rowHeaders.Length - 1; i++)
                {
                    if (rowHeaders[i] == varName)
                    {
                        X[j] = table[i, table.GetLength(1) - 1];
                        break;
                    }
                }
            }
            Log("X = (" + string.Join("; ", X.Select(v => $"{v:F2}")) + ")");
        }
        static void SolveLPPMenu()
        {
            Console.Write("Введіть кількість змінних (n): ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0) return;

            Console.Write("Введіть кількість обмежень (m): ");
            if (!int.TryParse(Console.ReadLine(), out int m) || m <= 0) return;

            Console.Write("Введіть тип задачі (1 - max, 2 - min): ");
            bool isMax = Console.ReadLine().Trim() == "1";

            Console.WriteLine("\nВведіть коефіцієнти цільової функції Z (через пробіл):");
            double[] Z_coeffs = ReadVector(n);

            double[,] table = new double[m + 1, n + 1];
            string[] rowHeaders = new string[m + 1];
            string[] colHeaders = new string[n + 1];

            for (int j = 0; j < n; j++) colHeaders[j] = $"-x{j + 1}";
            colHeaders[n] = "1";
            for (int i = 0; i < m; i++) rowHeaders[i] = $"y{i + 1}";
            rowHeaders[m] = "Z";

            Log("\nПостановка задачі:");
            string zFunc = "Z = " + string.Join(" + ", Z_coeffs.Select((val, idx) => $"{val}x{idx + 1}")).Replace("+ -", "- ") + (isMax ? " -> max" : " -> min");
            Log(zFunc);
            Log("при обмеженнях:");

            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"\nОбмеження {i + 1}:");
                Console.WriteLine("Введіть коефіцієнти при змінних (через пробіл):");
                double[] a = ReadVector(n);

                Console.Write("Знак (<= або >=): ");
                string sign = Console.ReadLine().Trim();

                Console.Write("Вільний член (b): ");
                double b = double.Parse(Console.ReadLine().Replace('.', ','));

                string constrStr = string.Join(" + ", a.Select((val, idx) => $"{val}x{idx + 1}")).Replace("+ -", "- ") + $" {sign} {b}";
                Log(constrStr);

                double multiplier = (sign == ">=") ? -1.0 : 1.0;
                for (int j = 0; j < n; j++) table[i, j] = a[j] * multiplier;
                table[i, n] = b * multiplier;
            }
            Log($"x[j] >= 0, j=1,{n}");

            // Заповнюємо рядок Z
            for (int j = 0; j < n; j++) table[m, j] = isMax ? -Z_coeffs[j] : Z_coeffs[j];
            table[m, n] = 0;

            Log("\nПерепишемо систему обмежень:");
            for (int i = 0; i < m; i++)
            {
                string eq = "";
                for (int j = 0; j < n; j++)
                {
                    double val = -table[i, j];
                    eq += $"({val:F2}) * X[{j + 1}] + ";
                }
                eq += $"{table[i, n]:F2} >= 0";
                Log(eq);
            }

            Log("\nВхідна симплекс-таблиця:");
            LogMatrixLPP(table, rowHeaders, colHeaders);

            // ФАЗА 1: Пошук опорного розв'язку
            Log("\nПошук опорного розв'язку:");
            while (true)
            {
                int r = -1; // шукаємо від'ємний елемент у стовпці вільних членів
                for (int i = 0; i < m; i++)
                {
                    if (table[i, n] < -1e-9) { r = i; break; }
                }

                if (r == -1)
                {
                    Log("Опорний розв'язок знайдено.");
                    break;
                }

                int s = -1; // шукаємо від'ємний елемент у знайденому рядку
                for (int j = 0; j < n; j++)
                {
                    if (table[r, j] < -1e-9) { s = j; break; }
                }

                if (s == -1)
                {
                    Log("Система обмежень є суперечливою.");
                    PromptSaveToFile();
                    return;
                }

                // Шукаємо мінімальне невід'ємне відношення для вибору розв'язувального рядка
                int pivotRow = -1;
                double minRatio = double.MaxValue;
                for (int i = 0; i < m; i++)
                {
                    if (Math.Abs(table[i, s]) > 1e-9)
                    {
                        double ratio = table[i, n] / table[i, s];
                        if (ratio >= 0 && ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                if (pivotRow == -1) pivotRow = r;

                Log($"Розв'язувальний рядок: {rowHeaders[pivotRow]}");
                Log($"Розв'язувальний стовпець: {colHeaders[s]}");

                table = DoSimplexMJEStep(table, pivotRow, s);
                SwapLPPHeaders(ref rowHeaders[pivotRow], ref colHeaders[s]);
                LogMatrixLPP(table, rowHeaders, colHeaders);
            }

            PrintSolutionX(table, rowHeaders, n);

            // ФАЗА 2: Пошук оптимального розв'язку
            Log("\nПошук оптимального розв'язку:");
            while (true)
            {
                int s = -1;
                for (int j = 0; j < n; j++)
                {
                    if (table[m, j] < -1e-9) { s = j; break; } // шукаємо від'ємний у Z-рядку
                }

                if (s == -1)
                {
                    Log("Оптимальний розв'язок знайдено.");
                    break;
                }

                int pivotRow = -1;
                double minRatio = double.MaxValue;
                for (int i = 0; i < m; i++)
                {
                    if (table[i, s] > 1e-9) 
                    {
                        double ratio = table[i, n] / table[i, s];
                        if (ratio >= 0 && ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                if (pivotRow == -1)
                {
                    Log("Функція мети не обмежена зверху.");
                    PromptSaveToFile();
                    return;
                }

                Log($"Розв'язувальний рядок: {rowHeaders[pivotRow]}");
                Log($"Розв'язувальний стовпець: {colHeaders[s]}");

                table = DoSimplexMJEStep(table, pivotRow, s);
                SwapLPPHeaders(ref rowHeaders[pivotRow], ref colHeaders[s]);
                LogMatrixLPP(table, rowHeaders, colHeaders);
            }

            PrintSolutionX(table, rowHeaders, n);
            double finalZ = isMax ? table[m, n] : -table[m, n];
            Log($"{(isMax ? "Max" : "Min")} (Z) = {finalZ:F2}");

            PromptSaveToFile();
        }
    }
}