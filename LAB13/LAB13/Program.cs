using System;
using System.Collections.Generic;
using System.Linq;

namespace AqualyzerLab13
{
    // ============================================================
    // 1. DTOs (Объекты передачи данных)
    // ============================================================

    public class DrinkRecord
    {
        public string DrinkName { get; set; }
        public int VolumeMl { get; set; }
        public DateTime Timestamp { get; set; }

        public DrinkRecord(string drinkName, int volumeMl, DateTime timestamp)
        {
            DrinkName = drinkName;
            VolumeMl = volumeMl;
            Timestamp = timestamp;
        }
    }

    public class ProgressDto
    {
        public int CurrentMl { get; set; }
        public int GoalMl { get; set; }
        public double Percent { get; set; }
    }


    // ============================================================
    // 2. COMMANDS (Команды — операции записи)
    // ============================================================

    public interface ICommand { }

    public class AddDrinkCommand : ICommand
    {
        public string DrinkName { get; }
        public int VolumeMl { get; }
        public DateTime Timestamp { get; }

        public AddDrinkCommand(string drinkName, int volumeMl)
        {
            DrinkName = drinkName;
            VolumeMl = volumeMl;
            Timestamp = DateTime.Now;
        }
    }

    public class ChangeGoalCommand : ICommand
    {
        public int NewGoalMl { get; }

        public ChangeGoalCommand(int newGoalMl)
        {
            NewGoalMl = newGoalMl;
        }
    }


    // ============================================================
    // 3. QUERIES (Запросы — операции чтения)
    // ============================================================

    public interface IQuery { }

    public class GetProgressQuery : IQuery { }

    public class GetHistoryQuery : IQuery
    {
        public DateTime? Date { get; }

        public GetHistoryQuery(DateTime? date = null)
        {
            Date = date;
        }
    }

    public class GetStatisticsQuery : IQuery { }


    // ============================================================
    // 4. STORES (Хранилища — Write Side и Read Side)
    // ============================================================

    // Write Store — оптимизирован для записи
    public class WriteStore
    {
        private readonly List<DrinkRecord> _records = new List<DrinkRecord>();
        private int _currentGoalMl = 2500;

        public void AddRecord(DrinkRecord record)
        {
            _records.Add(record);
            Console.WriteLine($"  [WriteStore] Запись добавлена: {record.DrinkName} — {record.VolumeMl} мл");
        }

        public void SetGoal(int goalMl)
        {
            _currentGoalMl = goalMl;
            Console.WriteLine($"  [WriteStore] Цель изменена на {goalMl} мл");
        }

        public List<DrinkRecord> GetRecords() => new List<DrinkRecord>(_records);
        public int GetGoal() => _currentGoalMl;
    }

    // Read Store — оптимизирован для чтения (кэширует данные)
    public class ReadStore
    {
        private ProgressDto _cachedProgress;
        private List<DrinkRecord> _cachedHistory;
        private bool _isDirty = true;

        public void UpdateFromWriteStore(WriteStore writeStore)
        {
            var records = writeStore.GetRecords();
            int totalMl = records.Sum(r => r.VolumeMl);
            int goal = writeStore.GetGoal();

            _cachedProgress = new ProgressDto
            {
                CurrentMl = totalMl,
                GoalMl = goal,
                Percent = goal > 0 ? (double)totalMl / goal * 100 : 0
            };

            _cachedHistory = records;
            _isDirty = false;
            Console.WriteLine("  [ReadStore] Кэш обновлён из WriteStore");
        }

        public ProgressDto GetProgress()
        {
            if (_isDirty) throw new InvalidOperationException("ReadStore не синхронизирован. Вызовите UpdateFromWriteStore().");
            return _cachedProgress;
        }

        public List<DrinkRecord> GetHistory()
        {
            if (_isDirty) throw new InvalidOperationException("ReadStore не синхронизирован. Вызовите UpdateFromWriteStore().");
            return _cachedHistory;
        }
    }


    // ============================================================
    // 5. HANDLERS (Обработчики команд и запросов)
    // ============================================================

    public interface ICommandHandler
    {
        void Handle(ICommand command);
    }

    public interface IQueryHandler
    {
        object Handle(IQuery query);
    }

    // Обработчик команды добавления напитка
    public class AddDrinkCommandHandler : ICommandHandler
    {
        private readonly WriteStore _writeStore;
        private readonly ReadStore _readStore;

        public AddDrinkCommandHandler(WriteStore writeStore, ReadStore readStore)
        {
            _writeStore = writeStore;
            _readStore = readStore;
        }

        public void Handle(ICommand command)
        {
            var cmd = (AddDrinkCommand)command;
            var record = new DrinkRecord(cmd.DrinkName, cmd.VolumeMl, cmd.Timestamp);
            _writeStore.AddRecord(record);
            _readStore.UpdateFromWriteStore(_writeStore); // Синхронизация Read Side
        }
    }

    // Обработчик команды изменения цели
    public class ChangeGoalCommandHandler : ICommandHandler
    {
        private readonly WriteStore _writeStore;
        private readonly ReadStore _readStore;

        public ChangeGoalCommandHandler(WriteStore writeStore, ReadStore readStore)
        {
            _writeStore = writeStore;
            _readStore = readStore;
        }

        public void Handle(ICommand command)
        {
            var cmd = (ChangeGoalCommand)command;
            _writeStore.SetGoal(cmd.NewGoalMl);
            _readStore.UpdateFromWriteStore(_writeStore); // Синхронизация Read Side
        }
    }

    // Обработчик запроса прогресса
    public class GetProgressQueryHandler : IQueryHandler
    {
        private readonly ReadStore _readStore;

        public GetProgressQueryHandler(ReadStore readStore)
        {
            _readStore = readStore;
        }

        public object Handle(IQuery query)
        {
            var progress = _readStore.GetProgress();
            Console.WriteLine($"  [QueryHandler] Прогресс: {progress.CurrentMl} / {progress.GoalMl} мл ({progress.Percent:F1}%)");
            return progress;
        }
    }

    // Обработчик запроса истории
    public class GetHistoryQueryHandler : IQueryHandler
    {
        private readonly ReadStore _readStore;

        public GetHistoryQueryHandler(ReadStore readStore)
        {
            _readStore = readStore;
        }

        public object Handle(IQuery query)
        {
            var history = _readStore.GetHistory();
            Console.WriteLine($"  [QueryHandler] История записей ({history.Count} записей):");
            foreach (var record in history)
            {
                Console.WriteLine($"    - [{record.Timestamp:HH:mm}] {record.DrinkName} — {record.VolumeMl} мл");
            }
            return history;
        }
    }


    // ============================================================
    // 6. MEDIATOR (Посредник для маршрутизации команд и запросов)
    // ============================================================

    public class CqrsMediator
    {
        private readonly Dictionary<Type, ICommandHandler> _commandHandlers;
        private readonly Dictionary<Type, IQueryHandler> _queryHandlers;

        public CqrsMediator()
        {
            _commandHandlers = new Dictionary<Type, ICommandHandler>();
            _queryHandlers = new Dictionary<Type, IQueryHandler>();
        }

        public void RegisterCommandHandler<TCommand>(ICommandHandler handler) where TCommand : ICommand
        {
            _commandHandlers[typeof(TCommand)] = handler;
        }

        public void RegisterQueryHandler<TQuery>(IQueryHandler handler) where TQuery : IQuery
        {
            _queryHandlers[typeof(TQuery)] = handler;
        }

        public void Send(ICommand command)
        {
            var commandType = command.GetType();
            if (_commandHandlers.TryGetValue(commandType, out var handler))
            {
                Console.WriteLine($"\n[Mediator] Отправка команды: {commandType.Name}");
                handler.Handle(command);
            }
            else
            {
                throw new InvalidOperationException($"Обработчик для команды {commandType.Name} не найден.");
            }
        }

        public object Query(IQuery query)
        {
            var queryType = query.GetType();
            if (_queryHandlers.TryGetValue(queryType, out var handler))
            {
                Console.WriteLine($"\n[Mediator] Отправка запроса: {queryType.Name}");
                return handler.Handle(query);
            }
            else
            {
                throw new InvalidOperationException($"Обработчик для запроса {queryType.Name} не найден.");
            }
        }
    }


    // ============================================================
    // 7. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №13: Аквалайзер (CQRS) ===\n");

            // 1. Создаём хранилища (Write Side и Read Side)
            WriteStore writeStore = new WriteStore();
            ReadStore readStore = new ReadStore();

            // 2. Создаём обработчики
            ICommandHandler addDrinkHandler = new AddDrinkCommandHandler(writeStore, readStore);
            ICommandHandler changeGoalHandler = new ChangeGoalCommandHandler(writeStore, readStore);
            IQueryHandler getProgressHandler = new GetProgressQueryHandler(readStore);
            IQueryHandler getHistoryHandler = new GetHistoryQueryHandler(readStore);

            // 3. Создаём Mediator и регистрируем обработчики
            CqrsMediator mediator = new CqrsMediator();
            mediator.RegisterCommandHandler<AddDrinkCommand>(addDrinkHandler);
            mediator.RegisterCommandHandler<ChangeGoalCommand>(changeGoalHandler);
            mediator.RegisterQueryHandler<GetProgressQuery>(getProgressHandler);
            mediator.RegisterQueryHandler<GetHistoryQuery>(getHistoryHandler);

            // 4. Демонстрация работы CQRS

            // --- Операции записи (Commands) ---
            Console.WriteLine(">>> Операции записи (Commands):");
            mediator.Send(new AddDrinkCommand("Вода", 500));
            mediator.Send(new AddDrinkCommand("Зелёный чай", 300));
            mediator.Send(new AddDrinkCommand("Кофе", 200));

            // --- Операции чтения (Queries) ---
            Console.WriteLine("\n>>> Операции чтения (Queries):");
            mediator.Query(new GetProgressQuery());
            mediator.Query(new GetHistoryQuery());

            // --- Ещё операции записи ---
            Console.WriteLine("\n>>> Ещё операции записи:");
            mediator.Send(new AddDrinkCommand("Вода", 600));
            mediator.Send(new ChangeGoalCommand(3000));

            // --- Ещё операции чтения ---
            Console.WriteLine("\n>>> Ещё операции чтения:");
            mediator.Query(new GetProgressQuery());
            mediator.Query(new GetHistoryQuery());

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}