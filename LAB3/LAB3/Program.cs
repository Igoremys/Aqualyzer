using System;

namespace AqualyzerLab3
{
    // ============================================================
    // 1. ПАТТЕРН: МОСТ (Bridge)
    // Разделяем абстракцию (уведомление) и реализацию (способ отправки)
    // ============================================================

    // Реализация (Implementor) — интерфейс для способов отправки
    public interface INotificationSender
    {
        void Send(string message);
    }

    // Конкретная реализация — отправка в консоль
    public class ConsoleNotificationSender : INotificationSender
    {
        public void Send(string message)
        {
            Console.WriteLine($"[CONSOLE] {message}");
        }
    }

    // Конкретная реализация — отправка на email
    public class EmailNotificationSender : INotificationSender
    {
        private readonly string _recipientEmail;

        public EmailNotificationSender(string recipientEmail)
        {
            _recipientEmail = recipientEmail;
        }

        public void Send(string message)
        {
            Console.WriteLine($"[EMAIL -> {_recipientEmail}] {message}");
        }
    }

    // Абстракция (Abstraction) — базовый класс уведомления
    public abstract class Notification
    {
        protected INotificationSender _sender;

        protected Notification(INotificationSender sender)
        {
            _sender = sender;
        }

        public abstract void Notify();
    }

    // Уточненная абстракция — напоминание о необходимости попить воды
    public class HydrationReminderNotification : Notification
    {
        private readonly string _userName;
        private readonly int _targetMl;

        public HydrationReminderNotification(INotificationSender sender, string userName, int targetMl)
            : base(sender)
        {
            _userName = userName;
            _targetMl = targetMl;
        }

        public override void Notify()
        {
            string message = $"Привет, {_userName}! Пора выпить воды. Ваша дневная цель: {_targetMl} мл.";
            _sender.Send(message);
        }
    }


    // ============================================================
    // 2. ПАТТЕРН: АДАПТЕР (Adapter)
    // Интеграция стороннего фитнес-трекера в приложение
    // ============================================================

    // Целевой интерфейс (Target), который ожидает приложение
    public interface IActivitySource
    {
        string GetActivityData();
    }

    // Адаптируемый класс (Adaptee) — сторонний фитнес-трекер с несовместимым интерфейсом
    public class ExternalHealthTracker
    {
        public int GetSteps() => 8500;
        public int GetCalories() => 420;
    }

    // Адаптер (Adapter) — преобразует интерфейс фитнес-трекера к интерфейсу приложения
    public class HealthTrackerAdapter : IActivitySource
    {
        private readonly ExternalHealthTracker _adaptee;

        public HealthTrackerAdapter(ExternalHealthTracker tracker)
        {
            _adaptee = tracker;
        }

        public string GetActivityData()
        {
            // Преобразуем несовместимые методы в требуемый формат
            int steps = _adaptee.GetSteps();
            int calories = _adaptee.GetCalories();
            return $"Активность: пройдено {steps} шагов, сожжено {calories} ккал.";
        }
    }


    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №3: Аквалайзер ===\n");

            // --- Демонстрация паттерна МОСТ ---
            Console.WriteLine("1. Демонстрация паттерна 'Мост':");
            Console.WriteLine("   (Абстракция уведомления отделена от способа отправки)\n");

            // Создаем один и тот же тип уведомления, но отправляем разными способами
            Notification consoleReminder = new HydrationReminderNotification(
                new ConsoleNotificationSender(), "Мусхажиев Игорь", 2500);
            consoleReminder.Notify();

            Notification emailReminder = new HydrationReminderNotification(
                new EmailNotificationSender("muskhazhiev@example.com"), "Мусхажиев Игорь", 2500);
            emailReminder.Notify();

            Console.WriteLine();

            // --- Демонстрация паттерна АДАПТЕР ---
            Console.WriteLine("2. Демонстрация паттерна 'Адаптер':");
            Console.WriteLine("   (Интеграция стороннего фитнес-трекера)\n");

            // Сторонний фитнес-трекер с несовместимым интерфейсом
            ExternalHealthTracker fitnessTracker = new ExternalHealthTracker();
            Console.WriteLine("   Прямой вызов методов фитнес-трекера:");
            Console.WriteLine($"   -> Шаги: {fitnessTracker.GetSteps()}");
            Console.WriteLine($"   -> Калории: {fitnessTracker.GetCalories()}");

            // Используем адаптер, чтобы работать с ним через интерфейс приложения
            IActivitySource activitySource = new HealthTrackerAdapter(fitnessTracker);
            Console.WriteLine("\n   Вызов через адаптер (целевой интерфейс IActivitySource):");
            Console.WriteLine($"   -> {activitySource.GetActivityData()}");

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}