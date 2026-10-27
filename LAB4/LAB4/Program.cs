using System;
using System.Collections.Generic;

namespace AqualyzerLab4
{
    // ============================================================
    // 1. ПАТТЕРН: ДЕКОРАТОР (Decorator)
    // Динамическое расширение свойств напитков
    // ============================================================

    // Компонент (Component) — интерфейс напитка
    public interface IDrink
    {
        double VolumeMl { get; }
        double Calories { get; }
        string GetDescription();
    }

    // Конкретный компонент (ConcreteComponent) — базовый напиток
    public class BaseDrink : IDrink
    {
        private readonly string _name;
        public double VolumeMl { get; }
        public double Calories { get; }

        public BaseDrink(string name, double volumeMl, double calories)
        {
            _name = name;
            VolumeMl = volumeMl;
            Calories = calories;
        }

        public virtual string GetDescription() => $"{_name} ({VolumeMl} мл)";
    }

    // Базовый декоратор (Decorator)
    public abstract class DrinkDecorator : IDrink
    {
        protected readonly IDrink _drink;

        protected DrinkDecorator(IDrink drink)
        {
            _drink = drink;
        }

        public double VolumeMl => _drink.VolumeMl;
        public virtual double Calories => _drink.Calories;
        public abstract string GetDescription();
    }

    // Конкретный декоратор — с лимоном (добавляет витамин C и калории)
    public class LemonDecorator : DrinkDecorator
    {
        public LemonDecorator(IDrink drink) : base(drink) { }

        public override double Calories => _drink.Calories + 5; // Лимон добавляет 5 ккал
        public override string GetDescription() => $"{_drink.GetDescription()} + лимон (витамин C)";
    }

    // Конкретный декоратор — с электролитами (для спортсменов)
    public class ElectrolytesDecorator : DrinkDecorator
    {
        public ElectrolytesDecorator(IDrink drink) : base(drink) { }

        public override double Calories => _drink.Calories + 15; // Электролиты добавляют 15 ккал
        public override string GetDescription() => $"{_drink.GetDescription()} + электролиты (Na, K, Mg)";
    }

    // Конкретный декоратор — охлаждённый
    public class ChilledDecorator : DrinkDecorator
    {
        public ChilledDecorator(IDrink drink) : base(drink) { }

        public override string GetDescription() => $"{_drink.GetDescription()} [охлаждённый]";
    }


    // ============================================================
    // 2. ПАТТЕРН: КОМПОНОВЩИК (Composite)
    // Иерархия задач гидратации
    // ============================================================

    // Компонент (Component) — интерфейс задачи гидратации
    public interface IHydrationTask
    {
        string Name { get; }
        int TotalMl { get; }
        void Display(int indent);
    }

    // Лист (Leaf) — отдельная задача выпить конкретный напиток
    public class SingleDrinkTask : IHydrationTask
    {
        private readonly IDrink _drink;

        public string Name => _drink.GetDescription();
        public int TotalMl => (int)_drink.VolumeMl;

        public SingleDrinkTask(IDrink drink)
        {
            _drink = drink;
        }

        public void Display(int indent)
        {
            Console.WriteLine($"{new string(' ', indent)}- {Name} | {TotalMl} мл | {_drink.Calories} ккал");
        }
    }

    // Композиция (Composite) — группа задач
    public class TaskGroup : IHydrationTask
    {
        private readonly string _name;
        private readonly List<IHydrationTask> _children = new List<IHydrationTask>();

        public string Name => _name;
        public int TotalMl
        {
            get
            {
                int total = 0;
                foreach (var child in _children)
                    total += child.TotalMl;
                return total;
            }
        }

        public TaskGroup(string name)
        {
            _name = name;
        }

        public void Add(IHydrationTask task) => _children.Add(task);
        public void Remove(IHydrationTask task) => _children.Remove(task);

        public void Display(int indent)
        {
            Console.WriteLine($"{new string(' ', indent)}[Группа] {_name} (ИТОГО: {TotalMl} мл)");
            foreach (var child in _children)
                child.Display(indent + 2);
        }
    }


    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №4: Аквалайзер ===\n");

            // --- Демонстрация паттерна ДЕКОРАТОР ---
            Console.WriteLine("1. Демонстрация паттерна 'Декоратор':");
            Console.WriteLine("   (Динамическое расширение свойств напитков)\n");

            // Базовый напиток
            IDrink water = new BaseDrink("Чистая вода", 500, 0);
            Console.WriteLine($"   Базовый: {water.GetDescription()} | {water.Calories} ккал");

            // Оборачиваем в декораторы — добавляем лимон
            IDrink waterWithLemon = new LemonDecorator(water);
            Console.WriteLine($"   С лимоном: {waterWithLemon.GetDescription()} | {waterWithLemon.Calories} ккал");

            // Оборачиваем в несколько декораторов — лимон + электролиты + охлаждение
            IDrink sportsDrink = new ChilledDecorator(
                                    new ElectrolytesDecorator(
                                        new LemonDecorator(new BaseDrink("Спортивный напиток", 600, 50))));
            Console.WriteLine($"   Спортивный: {sportsDrink.GetDescription()} | {sportsDrink.Calories} ккал\n");


            // --- Демонстрация паттерна КОМПОНОВЩИК ---
            Console.WriteLine("2. Демонстрация паттерна 'Компоновщик':");
            Console.WriteLine("   (Иерархия задач гидратации)\n");

            // Создаём отдельные задачи (листы)
            SingleDrinkTask morningWater = new SingleDrinkTask(new BaseDrink("Стакан воды", 250, 0));
            SingleDrinkTask morningTea = new SingleDrinkTask(
                                            new LemonDecorator(new BaseDrink("Чай", 300, 10)));

            // Создаём группу "Утренняя гидратация"
            TaskGroup morningGroup = new TaskGroup("Утренняя гидратация");
            morningGroup.Add(morningWater);
            morningGroup.Add(morningTea);

            // Создаём отдельные задачи для дня
            SingleDrinkTask afternoonWater = new SingleDrinkTask(new BaseDrink("Вода после обеда", 400, 0));
            SingleDrinkTask workoutDrink = new SingleDrinkTask(sportsDrink);

            // Создаём группу "Дневной план" — композиция групп и отдельных задач
            TaskGroup dailyPlan = new TaskGroup("ДНЕВНОЙ ПЛАН ГИДРАТАЦИИ");
            dailyPlan.Add(morningGroup);
            dailyPlan.Add(afternoonWater);
            dailyPlan.Add(workoutDrink);

            // Отображаем всю иерархию
            dailyPlan.Display(0);

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}