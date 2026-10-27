using System;
using System.Collections.Generic;

namespace AqualyzerLab5
{
    // ============================================================
    // 1. ПАТТЕРН: ПРИСПОСОБЛЕНЕЦ (Flyweight)
    // Оптимизация памяти за счет разделения внутреннего состояния
    // ============================================================

    // Интерфейс приспособленца
    public interface IDrinkCategory
    {
        string Name { get; }
        double HydrationFactor { get; }
        string Icon { get; }
        void DisplayDetails();
    }

    // Конкретный приспособленец (внутреннее состояние)
    public class DrinkCategory : IDrinkCategory
    {
        public string Name { get; }
        public double HydrationFactor { get; }
        public string Icon { get; }

        public DrinkCategory(string name, double factor, string icon)
        {
            Name = name;
            HydrationFactor = factor;
            Icon = icon;
        }

        public void DisplayDetails()
        {
            Console.WriteLine($"  [{Icon}] {Name} (Коэф. гидратации: {HydrationFactor})");
        }
    }

    // Фабрика приспособленцев (кэширует объекты)
    public class DrinkCategoryFactory
    {
        private readonly Dictionary<string, IDrinkCategory> _categories = new Dictionary<string, IDrinkCategory>();

        public IDrinkCategory GetCategory(string name)
        {
            if (!_categories.ContainsKey(name))
            {
                Console.WriteLine($"  [Factory] Создание нового объекта категории: '{name}'");
                // Имитация сложной инициализации и загрузки данных
                double factor = name == "Вода" ? 1.0 : (name == "Чай" ? 0.9 : 0.7);
                string icon = name == "Вода" ? "💧" : (name == "Чай" ? "" : "☕");
                _categories[name] = new DrinkCategory(name, factor, icon);
            }
            else
            {
                Console.WriteLine($"  [Factory] Использование существующего объекта категории: '{name}'");
            }
            return _categories[name];
        }
    }

    // Контекст (внешнее состояние)
    public class DrinkRecord
    {
        public double VolumeMl { get; }
        public DateTime Timestamp { get; }
        public IDrinkCategory Category { get; }

        public DrinkRecord(IDrinkCategory category, double volumeMl, DateTime timestamp)
        {
            Category = category;
            VolumeMl = volumeMl;
            Timestamp = timestamp;
        }
    }


    // ============================================================
    // 2. ПАТТЕРН: ФАСАД (Facade)
    // Упрощение интерфейса сложной подсистемы
    // ============================================================

    // Подсистема 1: Трекер
    public class HydrationTracker
    {
        private readonly List<DrinkRecord> _records = new List<DrinkRecord>();

        public void AddRecord(DrinkRecord record) => _records.Add(record);

        public double GetTotalEffectiveHydration()
        {
            double total = 0;
            foreach (var r in _records) total += r.VolumeMl * r.Category.HydrationFactor;
            return total;
        }
    }

    // Подсистема 2: Уведомления
    public class NotificationService
    {
        public void SendGoalReachedAlert(double amount)
        {
            Console.WriteLine($"  [Уведомление] Поздравляем! Вы достигли цели: {amount} мл.");
        }
    }

    // Фасад
    public class AqualyzerFacade
    {
        private readonly DrinkCategoryFactory _factory;
        private readonly HydrationTracker _tracker;
        private readonly NotificationService _notifier;
        private readonly double _dailyGoalMl;

        public AqualyzerFacade(double dailyGoalMl)
        {
            _dailyGoalMl = dailyGoalMl;
            _factory = new DrinkCategoryFactory();
            _tracker = new HydrationTracker();
            _notifier = new NotificationService();
        }

        // Простой метод для клиента, скрывающий сложную логику
        public void LogDrink(string categoryName, double volumeMl)
        {
            Console.WriteLine($"\nЗапись приема жидкости: {categoryName} ({volumeMl} мл)");
            IDrinkCategory category = _factory.GetCategory(categoryName);
            DrinkRecord record = new DrinkRecord(category, volumeMl, DateTime.Now);
            _tracker.AddRecord(record);
        }

        public void ShowSummary()
        {
            double total = _tracker.GetTotalEffectiveHydration();
            Console.WriteLine($"\n--- Итог за день ---");
            Console.WriteLine($"Эффективная гидратация: {total} мл из {_dailyGoalMl} мл.");
            if (total >= _dailyGoalMl)
            {
                _notifier.SendGoalReachedAlert(total);
            }
        }
    }


    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №5: Аквалайзер ===\n");

            // Создаем фасад (клиенту не нужно знать о фабрике и трекере)
            AqualyzerFacade facade = new AqualyzerFacade(2500);

            // Демонстрация Приспособленца (Factory будет переиспользовать объекты)
            facade.LogDrink("Вода", 500);
            facade.LogDrink("Чай", 300);
            facade.LogDrink("Вода", 400); // Переиспользование объекта "Вода"
            facade.LogDrink("Кофе", 200);
            facade.LogDrink("Вода", 600); // Переиспользование объекта "Вода"

            // Демонстрация Фасада (простой интерфейс для получения итога)
            facade.ShowSummary();

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}