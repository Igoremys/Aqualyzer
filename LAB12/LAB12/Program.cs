using System;
using System.Collections.Generic;

namespace AqualyzerLab12
{
    // ============================================================
    // 1. MODEL — данные и бизнес-логика
    // ============================================================

    public class DrinkRecord
    {
        public string DrinkName { get; }
        public int VolumeMl { get; }
        public DateTime Timestamp { get; }

        public DrinkRecord(string drinkName, int volumeMl, DateTime timestamp)
        {
            DrinkName = drinkName;
            VolumeMl = volumeMl;
            Timestamp = timestamp;
        }
    }

    public class HydrationModel
    {
        private int _currentMl;
        private int _dailyGoalMl;
        private readonly List<DrinkRecord> _history = new List<DrinkRecord>();

        public HydrationModel(int dailyGoalMl)
        {
            _dailyGoalMl = dailyGoalMl;
            _currentMl = 0;
        }

        public void AddWater(string drinkName, int volumeMl)
        {
            if (volumeMl <= 0)
                throw new ArgumentException("Объём должен быть больше нуля.");

            _currentMl += volumeMl;
            _history.Add(new DrinkRecord(drinkName, volumeMl, DateTime.Now));
        }

        public void SetGoal(int goalMl)
        {
            if (goalMl <= 0)
                throw new ArgumentException("Цель должна быть больше нуля.");
            _dailyGoalMl = goalMl;
        }

        public int GetCurrentMl() => _currentMl;
        public int GetDailyGoal() => _dailyGoalMl;

        public double GetProgressPercent()
        {
            if (_dailyGoalMl == 0) return 0;
            return (double)_currentMl / _dailyGoalMl * 100;
        }

        public bool IsGoalReached() => _currentMl >= _dailyGoalMl;

        public List<DrinkRecord> GetHistory() => new List<DrinkRecord>(_history);
    }


    // ============================================================
    // 2. VIEW — интерфейс пользователя
    // ============================================================

    public interface IView
    {
        void ShowMenu();
        void ShowProgress(int current, int goal, double percent);
        void ShowMessage(string message);
        void ShowHistory(List<DrinkRecord> history);
        string GetInput();
    }

    public class ConsoleView : IView
    {
        public void ShowMenu()
        {
            Console.WriteLine("\n=== МЕНЮ АКВАЛАЙЗЕРА ===");
            Console.WriteLine("1. Добавить воду");
            Console.WriteLine("2. Показать прогресс");
            Console.WriteLine("3. Показать историю");
            Console.WriteLine("4. Изменить дневную цель");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите действие: ");
        }

        public void ShowProgress(int current, int goal, double percent)
        {
            Console.WriteLine($"\n--- Прогресс гидратации ---");
            Console.WriteLine($"Выпито: {current} мл из {goal} мл ({percent:F1}%)");

            // Рисуем прогресс-бар
            int barLength = 20;
            int filled = (int)(barLength * percent / 100);
            if (filled > barLength) filled = barLength;

            Console.Write("[");
            for (int i = 0; i < barLength; i++)
                Console.Write(i < filled ? "█" : "░");
            Console.WriteLine($"] {percent:F0}%");

            if (percent >= 100)
                Console.WriteLine("🎉 Цель достигнута! Отличная работа!");
        }

        public void ShowMessage(string message)
        {
            Console.WriteLine($"\n  {message}");
        }

        public void ShowHistory(List<DrinkRecord> history)
        {
            Console.WriteLine("\n--- История записей ---");
            if (history.Count == 0)
            {
                Console.WriteLine("  История пуста.");
                return;
            }
            foreach (var record in history)
            {
                Console.WriteLine($"  [{record.Timestamp:HH:mm}] {record.DrinkName} — {record.VolumeMl} мл");
            }
        }

        public string GetInput()
        {
            return Console.ReadLine()?.Trim() ?? "";
        }
    }


    // ============================================================
    // 3. CONTROLLER — логика управления
    // ============================================================

    public class HydrationController
    {
        private readonly HydrationModel _model;
        private readonly IView _view;
        private bool _isRunning = true;

        public HydrationController(HydrationModel model, IView view)
        {
            _model = model;
            _view = view;
        }

        public void Run()
        {
            _view.ShowMessage("Добро пожаловать в Аквалайзер!");
            _view.ShowMessage($"Ваша дневная цель: {_model.GetDailyGoal()} мл.");

            while (_isRunning)
            {
                _view.ShowMenu();
                string input = _view.GetInput();

                switch (input)
                {
                    case "1":
                        HandleAddWater();
                        break;
                    case "2":
                        HandleShowProgress();
                        break;
                    case "3":
                        HandleShowHistory();
                        break;
                    case "4":
                        HandleSetGoal();
                        break;
                    case "0":
                        _isRunning = false;
                        _view.ShowMessage("До свидания! Пейте больше воды 💧");
                        break;
                    default:
                        _view.ShowMessage("Неверный ввод. Попробуйте снова.");
                        break;
                }
            }
        }

        private void HandleAddWater()
        {
            _view.ShowMessage("Введите название напитка (например, Вода, Чай, Кофе):");
            string name = _view.GetInput();
            if (string.IsNullOrEmpty(name)) name = "Вода";

            _view.ShowMessage("Введите объём в мл:");
            string volumeStr = _view.GetInput();
            if (int.TryParse(volumeStr, out int volume) && volume > 0)
            {
                try
                {
                    _model.AddWater(name, volume);
                    _view.ShowMessage($"✅ Добавлено: {name} — {volume} мл.");

                    if (_model.IsGoalReached())
                        _view.ShowMessage("🎉 Поздравляем! Дневная цель достигнута!");
                }
                catch (Exception ex)
                {
                    _view.ShowMessage($"❌ Ошибка: {ex.Message}");
                }
            }
            else
            {
                _view.ShowMessage("❌ Неверный объём.");
            }
        }

        private void HandleShowProgress()
        {
            int current = _model.GetCurrentMl();
            int goal = _model.GetDailyGoal();
            double percent = _model.GetProgressPercent();
            _view.ShowProgress(current, goal, percent);
        }

        private void HandleShowHistory()
        {
            _view.ShowHistory(_model.GetHistory());
        }

        private void HandleSetGoal()
        {
            _view.ShowMessage("Введите новую дневную цель (мл):");
            string goalStr = _view.GetInput();
            if (int.TryParse(goalStr, out int goal) && goal > 0)
            {
                try
                {
                    _model.SetGoal(goal);
                    _view.ShowMessage($"✅ Новая цель установлена: {goal} мл.");
                }
                catch (Exception ex)
                {
                    _view.ShowMessage($"❌ Ошибка: {ex.Message}");
                }
            }
            else
            {
                _view.ShowMessage("❌ Неверное значение цели.");
            }
        }
    }


    // ============================================================
    // 4. ТОЧКА ВХОДА — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №12: Аквалайзер (MVC) ===\n");
            Console.WriteLine("Демонстрация работы паттерна MVC.\n");
            Console.WriteLine("В демо-режиме программа выполнит несколько действий автоматически,\n" +
                              "а затем предложит интерактивное меню.\n");

            // Создаём компоненты MVC
            HydrationModel model = new HydrationModel(2500);
            IView view = new ConsoleView();
            HydrationController controller = new HydrationController(model, view);

            // Автоматическая демонстрация (имитация действий пользователя)
            Console.WriteLine("--- Автоматическая демонстрация ---");

            // Добавляем воду напрямую через модель (для демонстрации)
            model.AddWater("Вода", 500);
            view.ShowMessage("Добавлено 500 мл воды.");

            model.AddWater("Зелёный чай", 300);
            view.ShowMessage("Добавлено 300 мл зелёного чая.");

            // Показываем прогресс
            view.ShowProgress(model.GetCurrentMl(), model.GetDailyGoal(), model.GetProgressPercent());

            // Показываем историю
            view.ShowHistory(model.GetHistory());

            // Меняем цель
            model.SetGoal(3000);
            view.ShowMessage($"Цель изменена на {model.GetDailyGoal()} мл.");
            view.ShowProgress(model.GetCurrentMl(), model.GetDailyGoal(), model.GetProgressPercent());

            Console.WriteLine("\n--- Запуск интерактивного режима ---");
            Console.WriteLine("(Введите '0' для выхода)\n");

            // Запускаем интерактивный контроллер
            controller.Run();
        }
    }
}