using System;
using System.Collections.Generic;

namespace AqualyzerLab10
{
    public interface IHydrationObserver
    {
        void Update(int consumedMl, int goalMl, string drinkName, int addedMl);
    }

    public interface IHydrationSubject
    {
        void Subscribe(IHydrationObserver observer);
        void Unsubscribe(IHydrationObserver observer);
    }

    public sealed class DrinkRecord
    {
        public string DrinkName { get; private set; }
        public int VolumeMl { get; private set; }

        public DrinkRecord(string drinkName, int volumeMl)
        {
            if (string.IsNullOrWhiteSpace(drinkName))
                throw new ArgumentException("Укажите название напитка.", "drinkName");
            if (volumeMl <= 0)
                throw new ArgumentOutOfRangeException("volumeMl", "Объем должен быть больше нуля.");
            DrinkName = drinkName;
            VolumeMl = volumeMl;
        }
    }

    public sealed class HydrationTracker : IHydrationSubject
    {
        private readonly int _dailyGoalMl;
        private readonly List<DrinkRecord> _records = new List<DrinkRecord>();
        private readonly List<IHydrationObserver> _observers = new List<IHydrationObserver>();

        public int ConsumedMl { get; private set; }

        public HydrationTracker(int dailyGoalMl)
        {
            if (dailyGoalMl <= 0)
                throw new ArgumentOutOfRangeException("dailyGoalMl");
            _dailyGoalMl = dailyGoalMl;
        }

        public void Subscribe(IHydrationObserver observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Unsubscribe(IHydrationObserver observer)
        {
            _observers.Remove(observer);
        }

        public void LogDrink(string drinkName, int volumeMl)
        {
            DrinkRecord record = new DrinkRecord(drinkName, volumeMl);
            _records.Add(record);
            ConsumedMl += volumeMl;

            foreach (IHydrationObserver observer in new List<IHydrationObserver>(_observers))
                observer.Update(ConsumedMl, _dailyGoalMl, record.DrinkName, record.VolumeMl);
        }
    }

    public sealed class HydrationHistoryView : IHydrationObserver
    {
        public void Update(int consumedMl, int goalMl, string drinkName, int addedMl)
        {
            Console.WriteLine("[История] {0}: +{1} мл, всего {2} из {3} мл.",
                drinkName, addedMl, consumedMl, goalMl);
        }
    }

    public interface IHydrationState
    {
        string GetMessage(int consumedMl, int goalMl);
    }

    public sealed class BelowGoalState : IHydrationState
    {
        public string GetMessage(int consumedMl, int goalMl)
        {
            return "До цели осталось " + (goalMl - consumedMl) + " мл.";
        }
    }

    public sealed class GoalReachedState : IHydrationState
    {
        public string GetMessage(int consumedMl, int goalMl)
        {
            return "Дневная цель достигнута!";
        }
    }

    public sealed class AboveGoalState : IHydrationState
    {
        public string GetMessage(int consumedMl, int goalMl)
        {
            return "Цель превышена на " + (consumedMl - goalMl) + " мл.";
        }
    }

    public sealed class HydrationStatusContext
    {
        private IHydrationState _state = new BelowGoalState();

        public void Update(int consumedMl, int goalMl)
        {
            if (consumedMl < goalMl)
                _state = new BelowGoalState();
            else if (consumedMl == goalMl)
                _state = new GoalReachedState();
            else
                _state = new AboveGoalState();

            Console.WriteLine("[Состояние] {0}", _state.GetMessage(consumedMl, goalMl));
        }
    }

    public sealed class GoalProgressNotifier : IHydrationObserver
    {
        private readonly HydrationStatusContext _status;

        public GoalProgressNotifier(HydrationStatusContext status)
        {
            if (status == null)
                throw new ArgumentNullException("status");
            _status = status;
        }

        public void Update(int consumedMl, int goalMl, string drinkName, int addedMl)
        {
            _status.Update(consumedMl, goalMl);
        }
    }

    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №10: Аквалайзер ===");
            Console.WriteLine("Паттерны: Наблюдатель и Состояние\n");

            HydrationTracker tracker = new HydrationTracker(2000);
            HydrationHistoryView history = new HydrationHistoryView();
            tracker.Subscribe(history);
            tracker.Subscribe(new GoalProgressNotifier(new HydrationStatusContext()));

            tracker.LogDrink("Вода", 750);
            Console.WriteLine();
            tracker.LogDrink("Зеленый чай", 750);
            Console.WriteLine();
            tracker.LogDrink("Вода", 500);
            Console.WriteLine("\nИстория отписывается от обновлений.");
            tracker.Unsubscribe(history);
            tracker.LogDrink("Вода", 250);
        }
    }
}
