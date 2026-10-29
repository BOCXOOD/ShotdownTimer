using System;
using System.Diagnostics;
using System.Threading;

namespace ShutdownTimer
{
    class Program
    {
        static DateTime? _shutdownTime = null;

        static void Main(string[] args)
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

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        SetTimer(true);
                        break;
                    case "2":
                        SetTimer(false);
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

        static void SetTimer(bool minutes)
        {
            Console.Write(minutes ? "Введите количество минут: " : "Введите количество секунд: ");
            if (!int.TryParse(Console.ReadLine(), out int value) || value <= 0)
            {
                Console.WriteLine("Некорректное значение! Нажмите Enter...");
                Console.ReadLine();
                return;
            }

            int seconds = minutes ? value * 60 : value;

            // Отменяем старый таймер, если он был
            if (_shutdownTime != null)
            {
                RunCommand("shutdown /a");
            }

            // Запускаем новый
            RunCommand($"shutdown /s /t {seconds}");

            _shutdownTime = DateTime.Now.AddSeconds(seconds);

            Console.WriteLine($"\nТаймер установлен: выключение через {seconds} сек.");
            Console.WriteLine($"Точное время выключения: {_shutdownTime:HH:mm:ss}");
            Console.WriteLine("Нажмите Enter...");
            Console.ReadLine();
        }

        static void CancelTimer()
        {
            RunCommand("shutdown /a");
            _shutdownTime = null;
            Console.WriteLine("\nТаймер выключения отменён (если он был активен).");
            Console.WriteLine("Нажмите Enter...");
            Console.ReadLine();
        }

        static void ShowRemainingTime()
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
                TimeSpan remaining = _shutdownTime.Value - DateTime.Now;

                if (remaining.TotalSeconds <= 0)
                {
                    Console.WriteLine("\nВремя вышло! Компьютер выключается...");
                    _shutdownTime = null;
                    Console.WriteLine("Нажмите Enter...");
                    Console.ReadLine();
                    return;
                }

                Console.Write($"\rДо выключения осталось: {remaining:hh\\:mm\\:ss}   ");
                Thread.Sleep(200);
            }

            Console.ReadKey(true);
            Console.WriteLine();
        }

        static void RunCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Если это команда отмены (shutdown /a), выводим ошибку, только если таймера не было
                    if (!string.IsNullOrWhiteSpace(error) && !command.Contains("/a"))
                    {
                        Console.WriteLine($"Ошибка: {error.Trim()}");
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