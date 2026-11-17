using System;
using System.Collections.Generic;

namespace AqualyzerLab10
{
    // ============================================================
    // 1. ПАТТЕРН: НАБЛЮДАТЕЛЬ (Observer)
    // ============================================================

    public interface IObserver
    {
        void Update(string message);
    }

    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void Notify(string message);
    }

    public class ProgressView : IObserver
    {
        public void Update(string message)
        {
            Console.WriteLine($"  [UI Прогресс] {message}");
        }
    }

    public class AlertService : IObserver
    {
        public void Update(string message)
        {
            // Реагируем только на критические или целевые события
            if (message.Contains("достигнута") || message.Contains("Обезвоживание"))
            {
                Console.WriteLine($"  [🚨 ВАЖНОЕ УВЕДОМЛЕНИЕ] {message}");
            }
        }
    }

    public class Logger : IObserver
    {
        public void Update(string message)
        {
            Console.WriteLine($"  [Лог] {DateTime.Now:HH:mm:ss} - {message}");
        }
    }


    // ============================================================
    // 2. ПАТТЕРН: СОСТОЯНИЕ (State)
    // ============================================================

    public interface IHydrationState
    {
        void CheckState(HydrationContext context);
        string GetStatusName();
    }

    public class DehydratedState : IHydrationState
    {
        public void CheckState(HydrationContext context)
        {
            // Если выпито 50% или более от цели, переходим в нормальное состояние
            if (context.CurrentVolume >= context.DailyGoal * 0.5)
            {
                context.SetState(new NormalState());
            }
        }

        public string GetStatusName() => "Критическое обезвоживание";
    }

    public class NormalState : IHydrationState
    {
        public void CheckState(HydrationContext context)
        {
            if (context.CurrentVolume < context.DailyGoal * 0.5)
            {
                context.SetState(new DehydratedState());
            }
            else if (context.CurrentVolume >= context.DailyGoal)
            {
                context.SetState(new HydratedState());
            }
        }

        public string GetStatusName() => "Нормальный уровень";
    }

    public class HydratedState : IHydrationState
    {
        public void CheckState(HydrationContext context)
        {
            // Если объем по какой-то причине уменьшился (например, сброс дня)
            if (context.CurrentVolume < context.DailyGoal)
            {
                context.SetState(new NormalState());
            }
        }

        public string GetStatusName() => "Цель достигнута!";
    }


    // Контекст, объединяющий оба паттерна
    public class HydrationContext : ISubject
    {
        private IHydrationState _state;
        // Используем List для гарантии совместимости со всеми версиями компилятора
        private readonly List<IObserver> _observers = new List<IObserver>();

        public int CurrentVolume { get; private set; }
        public int DailyGoal { get; }

        public HydrationContext(int dailyGoal)
        {
            DailyGoal = dailyGoal;
            CurrentVolume = 0;
            _state = new DehydratedState(); // Начальное состояние при 0 мл
        }

        public void SetState(IHydrationState state)
        {
            _state = state;
            Notify($"Состояние изменено на: {_state.GetStatusName()}");
        }

        public void Attach(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(string message)
        {
            foreach (var observer in _observers)
            {
                observer.Update(message);
            }
        }

        public void AddWater(int volume)
        {
            Console.WriteLine($"\n--- Добавление {volume} мл воды ---");
            CurrentVolume += volume;

            // Делегируем проверку состояния текущему объекту состояния
            _state.CheckState(this);

            // Уведомляем всех наблюдателей об обновлении
            Notify($"Объем: {CurrentVolume} мл из {DailyGoal} мл. Статус: {_state.GetStatusName()}");
        }
    }


    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №10: Аквалайзер ===\n");

            // 1. Создаем контекст с дневной целью 2000 мл
            HydrationContext context = new HydrationContext(2000);

            // 2. Создаем наблюдателей (подписчиков)
            IObserver progressView = new ProgressView();
            IObserver alertService = new AlertService();
            IObserver logger = new Logger();

            // 3. Подписываем наблюдателей на контекст
            context.Attach(progressView);
            context.Attach(alertService);
            context.Attach(logger);

            // 4. Демонстрация работы: добавляем воду порциями
            context.AddWater(300);  // 300 / 2000 (15%) -> Остается Dehydrated
            context.AddWater(800);  // 1100 / 2000 (55%) -> Переход в Normal
            context.AddWater(500);  // 1600 / 2000 (80%) -> Остается Normal
            context.AddWater(500);  // 2100 / 2000 (105%) -> Переход в Hydrated

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}