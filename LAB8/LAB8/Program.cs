using System;

namespace AqualyzerLab8
{
    // ============================================================
    // 1. ПАТТЕРН: ЦЕПОЧКА ОБЯЗАННОСТЕЙ (Chain of Responsibility)
    // ============================================================

    // Объект-запрос, передаваемый по цепочке
    public class DrinkLogRequest
    {
        public string DrinkType { get; set; }
        public double VolumeMl { get; set; }
        public double CurrentTotalMl { get; set; }
        public double DailyGoalMl { get; set; }

        public DrinkLogRequest(string drinkType, double volumeMl, double currentTotalMl, double dailyGoalMl)
        {
            DrinkType = drinkType;
            VolumeMl = volumeMl;
            CurrentTotalMl = currentTotalMl;
            DailyGoalMl = dailyGoalMl;
        }
    }

    // Базовый класс обработчика
    public abstract class Handler
    {
        protected Handler _nextHandler;

        public Handler SetNext(Handler handler)
        {
            _nextHandler = handler;
            return handler; // Возвращаем handler для удобного построения цепочки (fluent interface)
        }

        public abstract void Handle(DrinkLogRequest request);
    }

    // 1. Обработчик валидации
    public class ValidationHandler : Handler
    {
        public override void Handle(DrinkLogRequest request)
        {
            if (request.VolumeMl <= 0)
            {
                Console.WriteLine("  [ОШИБКА] Объем жидкости должен быть больше нуля.");
                return; // Прерываем цепочку
            }
            if (request.VolumeMl > 2000)
            {
                Console.WriteLine("  [ОШИБКА] Невозможно выпить более 2000 мл за один раз.");
                return; // Прерываем цепочку
            }

            Console.WriteLine("  [Валидация] Данные корректны.");
            _nextHandler?.Handle(request);
        }
    }

    // 2. Обработчик предупреждений о здоровье
    public class HealthWarningHandler : Handler
    {
        public override void Handle(DrinkLogRequest request)
        {
            if (request.VolumeMl > 1000)
            {
                Console.WriteLine("  [ПРЕДУПРЕЖДЕНИЕ] Вы пытаетесь выпить более 1 литра за раз. Это может быть вредно!");
            }
            // Не прерываем цепочку, просто предупреждаем и передаем дальше
            _nextHandler?.Handle(request);
        }
    }

    // 3. Обработчик проверки достижения цели
    public class GoalCheckHandler : Handler
    {
        public override void Handle(DrinkLogRequest request)
        {
            double newTotal = request.CurrentTotalMl + request.VolumeMl;
            if (newTotal >= request.DailyGoalMl && request.CurrentTotalMl < request.DailyGoalMl)
            {
                Console.WriteLine("  [ПОЗДРАВЛЕНИЕ] Поздравляем! Вы достигли дневной нормы гидратации!");
            }
            _nextHandler?.Handle(request);
        }
    }

    // 4. Финальный обработчик логирования
    public class LoggingHandler : Handler
    {
        public override void Handle(DrinkLogRequest request)
        {
            double newTotal = request.CurrentTotalMl + request.VolumeMl;
            Console.WriteLine($"  [УСПЕХ] Записано: {request.DrinkType}, {request.VolumeMl} мл. " +
                              $"Общий объем за день: {newTotal} мл из {request.DailyGoalMl} мл.");
            // Это конец цепочки, _nextHandler здесь обычно null
        }
    }

    // ============================================================
    // 2. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №8: Аквалайзер ===\n");

            // 1. Формируем цепочку обработчиков
            Handler validation = new ValidationHandler();
            Handler warning = new HealthWarningHandler();
            Handler goalCheck = new GoalCheckHandler();
            Handler logging = new LoggingHandler();

            // Собираем цепочку: Валидация -> Предупреждение -> Проверка цели -> Логирование
            validation.SetNext(warning).SetNext(goalCheck).SetNext(logging);

            // 2. Тестовые сценарии
            double currentTotal = 1800;
            double dailyGoal = 2500;

            Console.WriteLine("--- Сценарий 1: Обычное добавление воды ---");
            DrinkLogRequest req1 = new DrinkLogRequest("Чистая вода", 300, currentTotal, dailyGoal);
            validation.Handle(req1);

            Console.WriteLine("\n--- Сценарий 2: Достижение дневной цели ---");
            DrinkLogRequest req2 = new DrinkLogRequest("Зеленый чай", 400, currentTotal, dailyGoal);
            validation.Handle(req2);

            Console.WriteLine("\n--- Сценарий 3: Чрезмерное потребление (с предупреждением) ---");
            DrinkLogRequest req3 = new DrinkLogRequest("Вода", 1200, 500, dailyGoal);
            validation.Handle(req3);

            Console.WriteLine("\n--- Сценарий 4: Ошибка валидации (отрицательный объем) ---");
            DrinkLogRequest req4 = new DrinkLogRequest("Вода", -200, currentTotal, dailyGoal);
            validation.Handle(req4);

            Console.WriteLine("\n--- Сценарий 5: Ошибка валидации (нереалистичный объем) ---");
            DrinkLogRequest req5 = new DrinkLogRequest("Вода", 2500, currentTotal, dailyGoal);
            validation.Handle(req5);

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}