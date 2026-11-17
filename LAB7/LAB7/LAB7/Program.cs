using System;
using System.Collections.Generic;

namespace AqualyzerLab7
{
    // ============================================================
    // 1. ПАТТЕРН: КОМАНДА (Command)
    // ============================================================

    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class HydrationTracker
    {
        private readonly List<string> _records = new List<string>();
        private int _dailyGoalMl;

        public HydrationTracker(int initialGoalMl)
        {
            _dailyGoalMl = initialGoalMl;
        }

        public void AddRecord(string drinkName, int volumeMl)
        {
            string record = $"[{DateTime.Now:HH:mm}] {drinkName} — {volumeMl} мл";
            _records.Add(record);
            Console.WriteLine($"  [Tracker] Добавлена запись: {record}");
        }

        public void RemoveLastRecord()
        {
            if (_records.Count > 0)
            {
                string removed = _records[_records.Count - 1];
                _records.RemoveAt(_records.Count - 1);
                Console.WriteLine($"  [Tracker] Удалена запись: {removed}");
            }
            else
            {
                Console.WriteLine("  [Tracker] Нечего удалять — история пуста.");
            }
        }

        public void SetGoal(int goalMl)
        {
            Console.WriteLine($"  [Tracker] Цель изменена на {goalMl} мл.");
            _dailyGoalMl = goalMl;
        }

        public int GetGoal() => _dailyGoalMl;
        public int GetRecordsCount() => _records.Count;
    }

    public class AddDrinkCommand : ICommand
    {
        private readonly HydrationTracker _receiver;
        private readonly string _drinkName;
        private readonly int _volumeMl;

        public AddDrinkCommand(HydrationTracker receiver, string drinkName, int volumeMl)
        {
            _receiver = receiver;
            _drinkName = drinkName;
            _volumeMl = volumeMl;
        }

        public void Execute() => _receiver.AddRecord(_drinkName, _volumeMl);
        public void Undo() => _receiver.RemoveLastRecord();
    }

    public class ChangeGoalCommand : ICommand
    {
        private readonly HydrationTracker _receiver;
        private readonly int _newGoal;
        private int _oldGoal;

        public ChangeGoalCommand(HydrationTracker receiver, int newGoal)
        {
            _receiver = receiver;
            _newGoal = newGoal;
        }

        public void Execute()
        {
            _oldGoal = _receiver.GetGoal();
            _receiver.SetGoal(_newGoal);
        }

        public void Undo() => _receiver.SetGoal(_oldGoal);
    }

    public class CommandHistory
    {
        // 🔥 ИСПРАВЛЕНИЕ: Используем List вместо Stack, чтобы обойти ошибку среды
        private readonly List<ICommand> _history = new List<ICommand>();

        public void Push(ICommand command)
        {
            _history.Add(command); // Добавляем в конец (аналог Push)
        }

        public void UndoLast()
        {
            if (_history.Count > 0)
            {
                // Берем последний элемент и удаляем его (аналог Pop)
                int lastIndex = _history.Count - 1;
                ICommand command = _history[lastIndex];
                _history.RemoveAt(lastIndex);

                Console.WriteLine("  [History] Отмена последней операции...");
                command.Undo();
            }
            else
            {
                Console.WriteLine("  [History] История пуста — нечего отменять.");
            }
        }

        public int GetHistoryCount() => _history.Count;
    }


    // ============================================================
    // 2. ПАТТЕРН: ИТЕРАТОР (Iterator)
    // ============================================================

    public interface IIterator
    {
        bool HasNext();
        string Next();
        string Current { get; }
    }

    public interface IIterable
    {
        IIterator CreateIterator();
        IIterator CreateReverseIterator();
    }

    public class DrinkRecordCollection : IIterable
    {
        private readonly List<string> _records = new List<string>();

        public void Add(string record) => _records.Add(record);
        public int GetCount() => _records.Count;
        public string GetRecord(int index) => _records[index];

        public IIterator CreateIterator() => new DrinkRecordIterator(this, false);
        public IIterator CreateReverseIterator() => new DrinkRecordIterator(this, true);
    }

    public class DrinkRecordIterator : IIterator
    {
        private readonly DrinkRecordCollection _collection;
        private int _position;
        private readonly bool _reverse;

        public DrinkRecordIterator(DrinkRecordCollection collection, bool reverse)
        {
            _collection = collection;
            _reverse = reverse;
            _position = reverse ? collection.GetCount() - 1 : 0;
        }

        public string Current
        {
            get
            {
                if (_position < 0 || _position >= _collection.GetCount())
                    throw new InvalidOperationException("Позиция итератора выходит за границы коллекции.");
                return _collection.GetRecord(_position);
            }
        }

        public bool HasNext()
        {
            if (_reverse)
                return _position >= 0;
            return _position < _collection.GetCount();
        }

        public string Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("Элементов для перебора больше нет.");

            string result = Current;
            _position += _reverse ? -1 : 1;
            return result;
        }
    }


    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №7: Аквалайзер ===\n");

            // --- Демонстрация паттерна КОМАНДА ---
            Console.WriteLine("1. Демонстрация паттерна 'Команда':\n");

            HydrationTracker tracker = new HydrationTracker(2500);
            CommandHistory history = new CommandHistory();

            ICommand addWater = new AddDrinkCommand(tracker, "Вода", 500);
            addWater.Execute();
            history.Push(addWater);

            ICommand addTea = new AddDrinkCommand(tracker, "Зелёный чай", 300);
            addTea.Execute();
            history.Push(addTea);

            ICommand changeGoal = new ChangeGoalCommand(tracker, 3000);
            changeGoal.Execute();
            history.Push(changeGoal);

            Console.WriteLine($"\n  Текущая цель: {tracker.GetGoal()} мл");
            Console.WriteLine($"  Записей в истории: {tracker.GetRecordsCount()}");
            Console.WriteLine($"  Команд в истории: {history.GetHistoryCount()}\n");

            Console.WriteLine("--- Отмена последней операции (ChangeGoal) ---");
            history.UndoLast();
            Console.WriteLine($"  Цель после отмены: {tracker.GetGoal()} мл\n");

            Console.WriteLine("--- Отмена последней операции (AddTea) ---");
            history.UndoLast();
            Console.WriteLine($"  Записей после отмены: {tracker.GetRecordsCount()}\n");


            // --- Демонстрация паттерна ИТЕРАТОР ---
            Console.WriteLine("2. Демонстрация паттерна 'Итератор':\n");

            DrinkRecordCollection collection = new DrinkRecordCollection();
            collection.Add("08:00 — Вода, 250 мл");
            collection.Add("10:30 — Чай, 300 мл");
            collection.Add("13:00 — Вода, 400 мл");
            collection.Add("16:00 — Кофе, 200 мл");
            collection.Add("19:00 — Вода, 350 мл");

            Console.WriteLine("   Прямой обход (от старых к новым):");
            IIterator forwardIterator = collection.CreateIterator();
            while (forwardIterator.HasNext())
            {
                Console.WriteLine($"     -> {forwardIterator.Next()}");
            }

            Console.WriteLine("\n   Обратный обход (от новых к старым):");
            IIterator reverseIterator = collection.CreateReverseIterator();
            while (reverseIterator.HasNext())
            {
                Console.WriteLine($"     -> {reverseIterator.Next()}");
            }

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}