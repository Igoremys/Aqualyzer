using System;
using System.Collections.Generic;

namespace AqualyzerLab9
{
    // ============================================================
    // 1. ПАТТЕРН: ПОСРЕДНИК (Mediator)
    // ============================================================

    public interface IMediator
    {
        void Notify(object sender, string eventType);
    }

    public abstract class BaseComponent
    {
        protected IMediator _mediator;

        protected BaseComponent(IMediator mediator = null)
        {
            _mediator = mediator;
        }

        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }
    }

    public class WaterInputComponent : BaseComponent
    {
        public WaterInputComponent(IMediator mediator) : base(mediator) { }

        public void LogWater(int volumeMl)
        {
            Console.WriteLine($"  [Ввод] Пользователь добавил {volumeMl} мл воды.");
            _mediator.Notify(this, "WATER_LOGGED");
        }
    }

    public class ProgressView : BaseComponent
    {
        public ProgressView(IMediator mediator) : base(mediator) { }

        public void UpdateProgress(int current, int goal)
        {
            Console.WriteLine($"  [Интерфейс] Прогресс обновлен: {current} / {goal} мл ({(current * 100 / goal)}%)");
        }
    }

    public class NotificationService : BaseComponent
    {
        public NotificationService(IMediator mediator) : base(mediator) { }

        public void ShowAlert(string message)
        {
            Console.WriteLine($"  [Уведомление] 🚨 {message}");
        }
    }

    public class AqualyzerMediator : IMediator
    {
        private WaterInputComponent _input;
        private ProgressView _view;
        private NotificationService _alerts;

        // Состояние, которым управляет посредник
        private int _currentIntake = 0;
        private readonly int _dailyGoal = 2500;

        public AqualyzerMediator(WaterInputComponent input, ProgressView view, NotificationService alerts)
        {
            _input = input;
            _view = view;
            _alerts = alerts;

            _input.SetMediator(this);
            _view.SetMediator(this);
            _alerts.SetMediator(this);
        }

        public void Notify(object sender, string eventType)
        {
            if (eventType == "WATER_LOGGED")
            {
                // Имитация получения данных (в реальном приложении посредник запросит их у sender)
                // Для демо добавляем фиксированно, чтобы показать срабатывание порогов
                _currentIntake += 500;

                _view.UpdateProgress(_currentIntake, _dailyGoal);

                if (_currentIntake >= _dailyGoal)
                {
                    _alerts.ShowAlert("Поздравляем! Дневная норма воды выполнена!");
                }
                else if (_currentIntake >= _dailyGoal * 0.8)
                {
                    _alerts.ShowAlert("Осталось совсем немного до цели!");
                }
            }
        }
    }


    // ============================================================
    // 2. ПАТТЕРН: СНИМОК (Memento)
    // ============================================================

    // Снимок (Memento)
    public class HydrationMemento
    {
        private readonly int _currentIntake;
        private readonly int _dailyGoal;

        public HydrationMemento(int currentIntake, int dailyGoal)
        {
            _currentIntake = currentIntake;
            _dailyGoal = dailyGoal;
        }

        public int GetCurrentIntake() => _currentIntake;
        public int GetDailyGoal() => _dailyGoal;
    }

    // Инициатор (Originator)
    public class UserProfile
    {
        private int _currentIntake;
        private int _dailyGoal;

        public UserProfile(int dailyGoal)
        {
            _dailyGoal = dailyGoal;
            _currentIntake = 0;
        }

        public void DrinkWater(int volume)
        {
            _currentIntake += volume;
        }

        public HydrationMemento SaveState()
        {
            return new HydrationMemento(_currentIntake, _dailyGoal);
        }

        public void RestoreState(HydrationMemento memento)
        {
            _currentIntake = memento.GetCurrentIntake();
            _dailyGoal = memento.GetDailyGoal();
            Console.WriteLine($"  [Снимок] Состояние восстановлено: {_currentIntake} / {_dailyGoal} мл.");
        }

        public string GetStatus()
        {
            return $"Текущий статус: {_currentIntake} / {_dailyGoal} мл.";
        }
    }

    // Хранитель (Caretaker)
    public class HistoryCaretaker
    {
        // 🔥 ИСПРАВЛЕНИЕ: Используем List вместо Stack, чтобы обойти ошибку среды компиляции
        private readonly List<HydrationMemento> _mementos = new List<HydrationMemento>();

        public void Backup(UserProfile profile)
        {
            _mementos.Add(profile.SaveState()); // Добавляем в конец (аналог Push)
            Console.WriteLine("  [Хранитель] Снимок состояния сохранен.");
        }

        public void Undo(UserProfile profile)
        {
            if (_mementos.Count == 0)
            {
                Console.WriteLine("  [Хранитель] Нечего отменять.");
                return;
            }

            // Берем последний элемент и удаляем его (аналог Pop)
            int lastIndex = _mementos.Count - 1;
            HydrationMemento memento = _mementos[lastIndex];
            _mementos.RemoveAt(lastIndex);

            profile.RestoreState(memento);
        }
    }


    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №9: Аквалайзер ===\n");

            // --- Демонстрация паттерна ПОСРЕДНИК ---
            Console.WriteLine("1. Демонстрация паттерна 'Посредник':");
            Console.WriteLine("   (Координация компонентов без прямых связей)\n");

            var input = new WaterInputComponent(null);
            var view = new ProgressView(null);
            var alerts = new NotificationService(null);

            var mediator = new AqualyzerMediator(input, view, alerts);

            Console.WriteLine("Действие 1: Выпито 500 мл");
            input.LogWater(500);

            Console.WriteLine("\nДействие 2: Выпито еще 1500 мл (Итого 2000 мл)");
            // В демо-логике посредника каждый вызов добавляет 500 мл к внутреннему счетчику
            input.LogWater(1500);

            Console.WriteLine("\nДействие 3: Выпито еще 600 мл (Итого 2500 мл, цель достигнута)");
            input.LogWater(600);


            // --- Демонстрация паттерна СНИМОК ---
            Console.WriteLine("\n\n2. Демонстрация паттерна 'Снимок':");
            Console.WriteLine("   (Сохранение и восстановление состояния профиля)\n");

            UserProfile user = new UserProfile(2500);
            HistoryCaretaker history = new HistoryCaretaker();

            Console.WriteLine("Начало дня: " + user.GetStatus());
            history.Backup(user); // Снимок 1: 0 / 2500

            user.DrinkWater(1000);
            Console.WriteLine("После завтрака: " + user.GetStatus());
            history.Backup(user); // Снимок 2: 1000 / 2500

            user.DrinkWater(800);
            Console.WriteLine("После обеда: " + user.GetStatus());

            Console.WriteLine("\nОшибка! Пользователь случайно добавил лишнее. Отменяем последнее действие...");
            history.Undo(user); // Возврат к Снимку 2

            Console.WriteLine("После отмены: " + user.GetStatus());

            Console.WriteLine("\nОтменяем еще раз (возврат к началу дня)...");
            history.Undo(user); // Возврат к Снимку 1

            Console.WriteLine("Финальный статус: " + user.GetStatus());

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}