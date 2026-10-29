using System;
using System.Diagnostics;
using System.Threading;

namespace ShutdownTimer
{
    // Имя класса с заглавной буквы (PascalCase)
    internal class Program
    {
        // Приватное поле: префикс _ и camelCase
        private static DateTime? _shutdownTime = null;

        // Метод с заглавной буквы (PascalCase), параметры - camelCase
        private static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Таймер выключения ПК ===");
                Console.WriteLine("1. Выключить через N минут");
                Console.WriteLine("2. Выключить через N секунд");
                Console.WriteLine("3. Отменить таймер выключения");
                Console.WriteLine("4. Показать оставшееся время (в реальном времени)");
                Console.WriteLine("0. Выход");
                Console.Write("\nВыберите действие: ");

                // Локальные переменные - camelCase
                string userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        SetTimer(isMinutes: true);
                        break;
                    case "2":
                        SetTimer(isMinutes: false);
                        break;
                    case "3":
                        CancelTimer();
                        break;
                    case "4":
                        ShowRemainingTime();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Нажмите Enter...");
                        Console.ReadLine();
                        break;
                }
            }
        }

        private static void SetTimer(bool isMinutes)
        {
            Console.Write(isMinutes ? "Введите количество минут: " : "Введите количество секунд: ");

            if (!int.TryParse(Console.ReadLine(), out int timeValue) || timeValue <= 0)
            {
                Console.WriteLine("Некорректное значение! Нажмите Enter...");
                Console.ReadLine();
                return;
            }

            int totalSeconds = isMinutes ? timeValue * 60 : timeValue;

            if (_shutdownTime != null)
            {
                RunCommand("shutdown /a");
            }

            RunCommand($"shutdown /s /t {totalSeconds}");

            _shutdownTime = DateTime.Now.AddSeconds(totalSeconds);

            Console.WriteLine($"\nТаймер установлен: выключение через {totalSeconds} сек. ({timeValue} {(isMinutes ? "мин." : "сек.")})");
            Console.WriteLine($"Точное время выключения: {_shutdownTime:HH:mm:ss}");
            Console.WriteLine("Нажмите Enter...");
            Console.ReadLine();
        }

        private static void CancelTimer()
        {
            RunCommand("shutdown /a");
            _shutdownTime = null;
            Console.WriteLine("\nТаймер выключения отменён (если он был активен).");
            Console.WriteLine("Нажмите Enter...");
            Console.ReadLine();
        }

        private static void ShowRemainingTime()
        {
            if (_shutdownTime == null)
            {
                Console.WriteLine("\nАктивный таймер не установлен.");
                Console.WriteLine("Нажмите Enter...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...\n");

            while (!Console.KeyAvailable)
            {
                TimeSpan remainingTime = _shutdownTime.Value - DateTime.Now;

                if (remainingTime.TotalSeconds <= 0)
                {
                    Console.WriteLine("\nВремя вышло! Компьютер выключается...");
                    _shutdownTime = null;
                    Console.WriteLine("Нажмите Enter...");
                    Console.ReadLine();
                    return;
                }

                Console.Write($"\rДо выключения осталось: {remainingTime:hh\\:mm\\:ss}   ");
                Thread.Sleep(200);
            }

            Console.ReadKey(true);
            Console.WriteLine();
        }

        private static void RunCommand(string command)
        {
            try
            {
                // Понятное имя переменной вместо psi
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(processStartInfo))
                {
                    string standardOutput = process.StandardOutput.ReadToEnd();
                    string standardError = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(standardError) && !command.Contains("/a"))
                    {
                        Console.WriteLine($"Ошибка: {standardError.Trim()}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Системная ошибка: {ex.Message}");
            }
        }
    }
}