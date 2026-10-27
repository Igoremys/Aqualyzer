using System;
using System.Collections.Generic;

namespace AqualyzerLab2
{
    // ============================================================
    // 1. ПАТТЕРН: ФАБРИЧНЫЙ МЕТОД (Factory Method)
    // ============================================================

    // Продукт
    public interface IDrink
    {
        string Name { get; }
        double VolumeMl { get; }
        double HydrationCoefficient { get; } // Коэффициент усвоения (вода = 1.0, кофе = 0.8)
        double GetEffectiveHydration();
    }

    // Конкретные продукты
    public class WaterDrink : IDrink
    {
        public string Name => "Чистая вода";
        public double VolumeMl { get; }
        public double HydrationCoefficient => 1.0;

        public WaterDrink(double volumeMl) { VolumeMl = volumeMl; }
        public double GetEffectiveHydration() => VolumeMl * HydrationCoefficient;
    }

    public class TeaDrink : IDrink
    {
        public string Name => "Зеленый чай";
        public double VolumeMl { get; }
        public double HydrationCoefficient => 0.9;

        public TeaDrink(double volumeMl) { VolumeMl = volumeMl; }
        public double GetEffectiveHydration() => VolumeMl * HydrationCoefficient;
    }

    public class CoffeeDrink : IDrink
    {
        public string Name => "Кофе";
        public double VolumeMl { get; }
        public double HydrationCoefficient => 0.7; // Кофе слегка обезвоживает

        public CoffeeDrink(double volumeMl) { VolumeMl = volumeMl; }
        public double GetEffectiveHydration() => VolumeMl * HydrationCoefficient;
    }

    // Создатель (Creator)
    public interface IDrinkFactory
    {
        IDrink CreateDrink(double volumeMl);
    }

    // Конкретные создатели
    public class WaterFactory : IDrinkFactory
    {
        public IDrink CreateDrink(double volumeMl) => new WaterDrink(volumeMl);
    }

    public class TeaFactory : IDrinkFactory
    {
        public IDrink CreateDrink(double volumeMl) => new TeaDrink(volumeMl);
    }

    public class CoffeeFactory : IDrinkFactory
    {
        public IDrink CreateDrink(double volumeMl) => new CoffeeDrink(volumeMl);
    }


    // ============================================================
    // 2. ПАТТЕРН: ПРОТОТИП (Prototype)
    // ============================================================

    public interface IHydrationPreset
    {
        IHydrationPreset Clone();
    }

    public class HydrationPreset : IHydrationPreset
    {
        public string PresetName { get; set; }
        public int TargetMl { get; set; }
        public int ReminderIntervalMin { get; set; }

        public HydrationPreset(string presetName, int targetMl, int reminderIntervalMin)
        {
            PresetName = presetName;
            TargetMl = targetMl;
            ReminderIntervalMin = reminderIntervalMin;
        }

        // Реализация клонирования
        public IHydrationPreset Clone()
        {
            // MemberwiseClone создает поверхностную копию. 
            // Для значимых типов (int, string) этого достаточно для демонстрации паттерна.
            return (HydrationPreset)this.MemberwiseClone();
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"  Пресет: '{PresetName}' | Цель: {TargetMl} мл | Напоминания: каждые {ReminderIntervalMin} мин.");
        }
    }


    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №2: Аквалайзер ===\n");

            // --- Демонстрация Фабричного метода ---
            Console.WriteLine("1. Демонстрация паттерна 'Фабричный метод':");

            IDrinkFactory waterFactory = new WaterFactory();
            IDrinkFactory teaFactory = new TeaFactory();
            IDrinkFactory coffeeFactory = new CoffeeFactory();

            IDrink morningWater = waterFactory.CreateDrink(500);
            IDrink afternoonTea = teaFactory.CreateDrink(300);
            IDrink morningCoffee = coffeeFactory.CreateDrink(200);

            List<IDrink> dailyLog = new List<IDrink> { morningWater, afternoonTea, morningCoffee };

            double totalEffectiveHydration = 0;
            foreach (var drink in dailyLog)
            {
                Console.WriteLine($"   Выпито: {drink.Name}, {drink.VolumeMl} мл. " +
                                  $"Эффективная гидратация: {drink.GetEffectiveHydration()} мл.");
                totalEffectiveHydration += drink.GetEffectiveHydration();
            }
            Console.WriteLine($"   -> ИТОГО эффективной гидратации за день: {totalEffectiveHydration} мл.\n");


            // --- Демонстрация Прототипа ---
            Console.WriteLine("2. Демонстрация паттерна 'Прототип':");

            // Базовый пресет
            HydrationPreset standardPreset = new HydrationPreset("Обычный рабочий день", 2000, 60);
            Console.WriteLine("   Исходный пресет:");
            standardPreset.DisplayInfo();

            // Клонируем пресет для дня тренировки, изменяя только нужные поля
            HydrationPreset workoutPreset = (HydrationPreset)standardPreset.Clone();
            workoutPreset.PresetName = "День интенсивной тренировки";
            workoutPreset.TargetMl = 3500;
            workoutPreset.ReminderIntervalMin = 45;

            Console.WriteLine("\n   Клонированный и модифицированный пресет:");
            workoutPreset.DisplayInfo();

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}