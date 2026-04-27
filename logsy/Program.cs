
using System.Diagnostics;
using Serilog;
using Serilog.Formatting.Compact;

namespace TaskManager
{
    class Program
    {
        private static readonly List<TaskItem> Tasks = new List<TaskItem>();

        private static readonly TraceSource Ts = new TraceSource("TaskManager");

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .WriteTo.File("logs/task-manager-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Ts.Switch.Level = SourceLevels.All;
            Ts.Listeners.Add(new TextWriterTraceListener("logs/trace.log")
            {
                TraceOutputOptions = TraceOptions.DateTime
            });

            Log.Information("=== Приложение TaskManager запущено ===");
            Ts.TraceEvent(TraceEventType.Start, 0, "Сеанс начат");

            try
            {
                RunMainLoop();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Необработанная ошибка");
                Console.WriteLine("Критическая ошибка. Приложение будет закрыто. Подробнее см. логи.");
            }
            finally
            {
                Log.Information("Программа завершается");
                Ts.TraceEvent(TraceEventType.Stop, 0, "Сеанс завершён");
                Ts.Flush();
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Главный цикл обработки команд пользователя
        /// </summary>
        private static void RunMainLoop()
        {
            Console.WriteLine("Добро пожаловать в TaskManager!");
            Console.WriteLine("Доступные команды: add, remove, list, exit");

            while (true)
            {
                Console.Write("\n> ");
                var command = Console.ReadLine()?.Trim().ToLower();

                switch (command)
                {
                    case "add":
                        AddTask();
                        break;
                    case "remove":
                        RemoveTask();
                        break;
                    case "list":
                        ListTasks();
                        break;
                    case "exit":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда. Попробуйте: add, remove, list, exit");
                        break;
                }
            }
        }

        private static void AddTask()
        {
            var sw = Stopwatch.StartNew();
            Ts.TraceEvent(TraceEventType.Start, 1, "AddTask начата");

            try
            {
                Console.Write("Введите название задачи: ");
                var title = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(title))
                {
                    Log.Error("Попытка добавить задачу с пустым названием");
                    Console.WriteLine("Ошибка: название не может быть пустым. Операция отменена.");
                    return;
                }

                Console.Write("Введите описание (можно пропустить): ");
                var description = Console.ReadLine()?.Trim() ?? "";

                Console.Write("Приоритет (Low/Medium/High, по умолчанию Medium): ");
                var priorityInput = Console.ReadLine()?.Trim();
                Priority priority;
                if (!Enum.TryParse(priorityInput, true, out priority) || !Enum.IsDefined(typeof(Priority), priority))
                {
                    priority = Priority.Medium;
                    Console.WriteLine($"Приоритет установлен в {priority}");
                }

                var task = new TaskItem(title, description, priority);
                Tasks.Add(task);

                Log.Information("Добавлена задача {TaskTitle} {@Task}", task.Title, task);
                Console.WriteLine($"Задача \"{task.Title}\" успешно добавлена (ID: {task.Id}).");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при добавлении задачи");
                Console.WriteLine("Произошла ошибка. Подробнее см. логи.");
            }
            finally
            {
                sw.Stop();
                Ts.TraceEvent(TraceEventType.Stop, 1, $"AddTask завершена за {sw.ElapsedMilliseconds} мс");
            }
        }

        private static void RemoveTask()
        {
            var sw = Stopwatch.StartNew();
            Ts.TraceEvent(TraceEventType.Start, 2, "RemoveTask начата");

            try
            {
                Console.Write("Введите название задачи для удаления: ");
                var title = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(title))
                {
                    Console.WriteLine("Название не может быть пустым.");
                    return;
                }

                var task = Tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (task == null)
                {
                    Log.Error("Ошибка: задача не найдена. Поиск по названию: {Title}", title);
                    Console.WriteLine("Задача с таким названием не найдена.");
                    return;
                }

                Tasks.Remove(task);
                Log.Information("Задача \"{TaskTitle}\" удалена", task.Title);
                Console.WriteLine($"Задача \"{task.Title}\" удалена.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при удалении задачи");
                Console.WriteLine("Произошла ошибка. Подробнее см. логи.");
            }
            finally
            {
                sw.Stop();
                Ts.TraceEvent(TraceEventType.Stop, 2, $"RemoveTask завершена за {sw.ElapsedMilliseconds} мс");
            }
        }

        private static void ListTasks()
        {
            var sw = Stopwatch.StartNew();
            Ts.TraceEvent(TraceEventType.Start, 3, "ListTasks начата");

            try
            {
                if (Tasks.Count == 0)
                {
                    Console.WriteLine("Список задач пуст.");
                }
                else
                {
                    Console.WriteLine("ID   | Название             | Приор. | Описание");
                    Console.WriteLine("-----+----------------------+--------+----------------");
                    foreach (var task in Tasks)
                    {
                        Console.WriteLine(task.ToString());
                    }
                }

                Log.Information("Показан список из {Count} задач", Tasks.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при выводе списка задач");
                Console.WriteLine("Произошла ошибка. Подробнее см. логи.");
            }
            finally
            {
                sw.Stop();
                Ts.TraceEvent(TraceEventType.Stop, 3, $"ListTasks завершена за {sw.ElapsedMilliseconds} мс");
            }
        }
    }
}