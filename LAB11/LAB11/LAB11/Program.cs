using System;
using System.Collections.Generic;

namespace AqualyzerLab11
{
    // ============================================================
    // 1. ПАТТЕРН: СТРАТЕГИЯ (Strategy)
    // ============================================================

    // Value Object — входные данные для расчёта
    public class DrinkInput
    {
        public double VolumeMl { get; set; }
        public double HydrationFactor { get; set; }
        public int Steps { get; set; }
        public int TemperatureC { get; set; }

        public DrinkInput(double volumeMl, double hydrationFactor, int steps = 0, int temperatureC = 20)
        {
            VolumeMl = volumeMl;
            HydrationFactor = hydrationFactor;
            Steps = steps;
            TemperatureC = temperatureC;
        }
    }

    // Интерфейс стратегии
    public interface IHydrationCalculationStrategy
    {
        double Calculate(DrinkInput input);
        string GetName();
    }

    // Конкретная стратегия 1: линейная формула
    public class LinearStrategy : IHydrationCalculationStrategy
    {
        public double Calculate(DrinkInput input)
        {
            return input.VolumeMl * input.HydrationFactor;
        }

        public string GetName() => "Линейная (объём × коэффициент)";
    }

    // Конкретная стратегия 2: с учётом физической активности
    public class ActivityBasedStrategy : IHydrationCalculationStrategy
    {
        public double Calculate(DrinkInput input)
        {
            // Базовый расчёт
            double baseValue = input.VolumeMl * input.HydrationFactor;
            // Корректировка: каждые 1000 шагов = +5% полезной гидратации
            double activityBonus = (input.Steps / 1000.0) * 0.05 * baseValue;
            return baseValue + activityBonus;
        }

        public string GetName() => "Активность (учёт шагов)";
    }

    // Конкретная стратегия 3: с учётом климата
    public class ClimateBasedStrategy : IHydrationCalculationStrategy
    {
        public double Calculate(DrinkInput input)
        {
            double baseValue = input.VolumeMl * input.HydrationFactor;
            // При температуре выше 25°C эффективность снижается (часть воды уходит на потоотделение)
            if (input.TemperatureC > 25)
            {
                double penalty = (input.TemperatureC - 25) * 0.02 * baseValue;
                return baseValue - penalty;
            }
            return baseValue;
        }

        public string GetName() => "Климат (учёт температуры)";
    }

    // Контекст (Context) — использует стратегию
    public class HydrationCalculator
    {
        private IHydrationCalculationStrategy _strategy;

        public HydrationCalculator(IHydrationCalculationStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(IHydrationCalculationStrategy strategy)
        {
            _strategy = strategy;
            Console.WriteLine($"  [Контекст] Стратегия изменена на: {_strategy.GetName()}");
        }

        public double CalculateHydration(DrinkInput input)
        {
            return _strategy.Calculate(input);
        }

        public string GetStrategyName() => _strategy.GetName();
    }


    // ============================================================
    // 2. ПАТТЕРН: ШАБЛОННЫЙ МЕТОД (Template Method)
    // ============================================================

    // Абстрактный класс — определяет скелет алгоритма
    public abstract class ReportGenerator
    {
        // Шаблонный метод — фиксированная последовательность шагов
        public void GenerateReport()
        {
            Console.WriteLine($"\n  >>> Формирование отчёта: {GetReportTitle()}");
            Console.WriteLine("  -----------------------------------------");
            CollectData();
            double stat = CalculateStatistics();
            string formatted = ApplyFormat($"Статистика: {stat:F1} мл эффективной гидратации");
            Output(formatted);
        }

        public abstract string GetReportTitle();
        public abstract void CollectData();
        public abstract double CalculateStatistics();
        public abstract string ApplyFormat(string data);

        // Общий шаг для всех подклассов
        public void Output(string result)
        {
            Console.WriteLine($"  {result}");
            Console.WriteLine("  -----------------------------------------");
        }
    }

    // Конкретный класс 1: дневной отчёт
    public class DailyReportGenerator : ReportGenerator
    {
        private readonly HydrationCalculator _calculator;
        private readonly List<DrinkInput> _dailyDrinks = new List<DrinkInput>();

        public DailyReportGenerator(HydrationCalculator calculator)
        {
            _calculator = calculator;
            // Демо-данные за один день
            _dailyDrinks.Add(new DrinkInput(500, 1.0, 4000, 22));
            _dailyDrinks.Add(new DrinkInput(300, 0.9, 4000, 22));
            _dailyDrinks.Add(new DrinkInput(200, 0.7, 4000, 22));
        }

        public override string GetReportTitle() => "ДНЕВНОЙ ОТЧЁТ";

        public override void CollectData()
        {
            Console.WriteLine($"  [Сбор данных] Загружено {_dailyDrinks.Count} записей за сегодня.");
        }

        public override double CalculateStatistics()
        {
            double total = 0;
            foreach (var drink in _dailyDrinks)
            {
                total += _calculator.CalculateHydration(drink);
            }
            return total;
        }

        public override string ApplyFormat(string data)
        {
            return $"[Дневной] {data}";
        }
    }

    // Конкретный класс 2: недельный отчёт
    public class WeeklyReportGenerator : ReportGenerator
    {
        private readonly HydrationCalculator _calculator;
        private readonly List<DrinkInput> _weeklyDrinks = new List<DrinkInput>();

        public WeeklyReportGenerator(HydrationCalculator calculator)
        {
            _calculator = calculator;
            // Демо-данные за неделю (7 дней × 3 напитка)
            for (int day = 0; day < 7; day++)
            {
                _weeklyDrinks.Add(new DrinkInput(500, 1.0, 8000, 24));
                _weeklyDrinks.Add(new DrinkInput(300, 0.9, 8000, 24));
                _weeklyDrinks.Add(new DrinkInput(200, 0.7, 8000, 24));
            }
        }

        public override string GetReportTitle() => "НЕДЕЛЬНЫЙ ОТЧЁТ";

        public override void CollectData()
        {
            Console.WriteLine($"  [Сбор данных] Загружено {_weeklyDrinks.Count} записей за 7 дней.");
        }

        public override double CalculateStatistics()
        {
            double total = 0;
            foreach (var drink in _weeklyDrinks)
            {
                total += _calculator.CalculateHydration(drink);
            }
            return total / 7.0; // Среднее в день
        }

        public override string ApplyFormat(string data)
        {
            return $"[Неделя] Среднее {data}";
        }
    }

    // Конкретный класс 3: месячный отчёт
    public class MonthlyReportGenerator : ReportGenerator
    {
        private readonly HydrationCalculator _calculator;
        private readonly List<DrinkInput> _monthlyDrinks = new List<DrinkInput>();

        public MonthlyReportGenerator(HydrationCalculator calculator)
        {
            _calculator = calculator;
            // Демо-данные за 30 дней
            for (int day = 0; day < 30; day++)
            {
                _monthlyDrinks.Add(new DrinkInput(500, 1.0, 10000, 26));
                _monthlyDrinks.Add(new DrinkInput(300, 0.9, 10000, 26));
                _monthlyDrinks.Add(new DrinkInput(200, 0.7, 10000, 26));
            }
        }

        public override string GetReportTitle() => "МЕСЯЧНЫЙ ОТЧЁТ";

        public override void CollectData()
        {
            Console.WriteLine($"  [Сбор данных] Загружено {_monthlyDrinks.Count} записей за 30 дней.");
        }

        public override double CalculateStatistics()
        {
            double total = 0;
            foreach (var drink in _monthlyDrinks)
            {
                total += _calculator.CalculateHydration(drink);
            }
            return total / 30.0; // Среднее в день
        }

        public override string ApplyFormat(string data)
        {
            return $"[Месяц] Среднее {data}";
        }
    }


    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №11: Аквалайзер ===\n");

            // --- Демонстрация паттерна СТРАТЕГИЯ ---
            Console.WriteLine("1. Демонстрация паттерна 'Стратегия':");
            Console.WriteLine("   (Переключение алгоритмов расчёта гидратации)\n");

            DrinkInput testDrink = new DrinkInput(volumeMl: 500, hydrationFactor: 1.0, steps: 8000, temperatureC: 30);

            // Стратегия 1: линейная
            HydrationCalculator calculator = new HydrationCalculator(new LinearStrategy());
            double result1 = calculator.CalculateHydration(testDrink);
            Console.WriteLine($"  Стратегия: {calculator.GetStrategyName()}");
            Console.WriteLine($"  Результат расчёта для 500 мл (8000 шагов, 30°C): {result1:F1} мл\n");

            // Стратегия 2: активность
            calculator.SetStrategy(new ActivityBasedStrategy());
            double result2 = calculator.CalculateHydration(testDrink);
            Console.WriteLine($"  Стратегия: {calculator.GetStrategyName()}");
            Console.WriteLine($"  Результат расчёта для 500 мл (8000 шагов, 30°C): {result2:F1} мл\n");

            // Стратегия 3: климат
            calculator.SetStrategy(new ClimateBasedStrategy());
            double result3 = calculator.CalculateHydration(testDrink);
            Console.WriteLine($"  Стратегия: {calculator.GetStrategyName()}");
            Console.WriteLine($"  Результат расчёта для 500 мл (8000 шагов, 30°C): {result3:F1} мл");


            // --- Демонстрация паттерна ШАБЛОННЫЙ МЕТОД ---
            Console.WriteLine("\n\n2. Демонстрация паттерна 'Шаблонный метод':");
            Console.WriteLine("   (Формирование отчётов за разные периоды)\n");

            // Создаём контекст с активной стратегией (климат)
            HydrationCalculator activeCalculator = new HydrationCalculator(new ClimateBasedStrategy());

            // Дневной отчёт
            ReportGenerator dailyReport = new DailyReportGenerator(activeCalculator);
            dailyReport.GenerateReport();

            // Недельный отчёт
            ReportGenerator weeklyReport = new WeeklyReportGenerator(activeCalculator);
            weeklyReport.GenerateReport();

            // Месячный отчёт
            ReportGenerator monthlyReport = new MonthlyReportGenerator(activeCalculator);
            monthlyReport.GenerateReport();

            // --- Взаимодействие паттернов: меняем стратегию и пересчитываем отчёт ---
            Console.WriteLine("\n\n3. Взаимодействие паттернов:");
            Console.WriteLine("   (Смена стратегии влияет на все отчёты)\n");

            activeCalculator.SetStrategy(new LinearStrategy());
            Console.WriteLine("  После смены стратегии на 'Линейная':");
            ReportGenerator dailyReport2 = new DailyReportGenerator(activeCalculator);
            dailyReport2.GenerateReport();

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}