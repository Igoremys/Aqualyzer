using System;
using System.Threading;

namespace AqualyzerLab6
{
    // ============================================================
    // 1. ПАТТЕРН: ЗАМЕСТИТЕЛЬ (Proxy)
    // Контроль доступа к сервису пользовательских данных
    // ============================================================

    // Общий интерфейс (Subject)
    public interface IUserDataService
    {
        string GetUserInfo(string userId);
        void UpdateDailyGoal(string userId, int goalMl);
    }

    // Реальный сервис (RealSubject) — имитирует медленное облачное хранилище
    public class CloudUserDataService : IUserDataService
    {
        public string GetUserInfo(string userId)
        {
            // Имитация задержки при обращении к облачной БД
            Thread.Sleep(1000);
            return $"[CloudDB] Данные пользователя {userId}: цель 2500 мл, статус активен.";
        }

        public void UpdateDailyGoal(string userId, int goalMl)
        {
            Thread.Sleep(1500); // Имитация долгой транзакции
            Console.WriteLine($"[CloudDB] Цель пользователя {userId} успешно обновлена на {goalMl} мл.");
        }
    }

    // Заместитель (Proxy) — контролирует доступ и логирует операции
    public class UserDataServiceProxy : IUserDataService
    {
        private CloudUserDataService _realService;
        private readonly string _currentUserRole;

        public UserDataServiceProxy(string currentUserRole)
        {
            _currentUserRole = currentUserRole;
        }

        // Ленивая инициализация реального сервиса (элемент Virtual Proxy)
        private CloudUserDataService GetRealService()
        {
            if (_realService == null)
            {
                Console.WriteLine("[Proxy] Инициализация тяжелого облачного сервиса...");
                _realService = new CloudUserDataService();
            }
            return _realService;
        }

        public string GetUserInfo(string userId)
        {
            Console.WriteLine($"[Proxy] Запрос данных для пользователя {userId}. Роли не требуются.");
            return GetRealService().GetUserInfo(userId);
        }

        public void UpdateDailyGoal(string userId, int goalMl)
        {
            Console.WriteLine($"\n[Proxy] Попытка изменить цель для {userId} на {goalMl} мл.");
            Console.WriteLine($"[Proxy] Проверка прав доступа (текущая роль: '{_currentUserRole}')...");

            // Защита (Protection Proxy): только Admin может менять цели
            if (_currentUserRole != "Admin")
            {
                Console.WriteLine($"[Proxy] ОШИБКА ДОСТУПА: Роль '{_currentUserRole}' не имеет прав на изменение глобальных настроек.");
                return;
            }

            Console.WriteLine("[Proxy] Права подтверждены. Делегирование реальному сервису...");
            GetRealService().UpdateDailyGoal(userId, goalMl);
        }
    }


    // ============================================================
    // 2. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №6: Аквалайзер ===\n");

            // Сценарий 1: Обычный пользователь (роль "User")
            Console.WriteLine("--- Сценарий 1: Пользователь с ролью 'User' ---");
            IUserDataService userProxy = new UserDataServiceProxy("User");

            // Чтение данных разрешено всем
            string info = userProxy.GetUserInfo("user_123");
            Console.WriteLine($"Результат: {info}\n");

            // Изменение цели запрещено
            userProxy.UpdateDailyGoal("user_123", 3000);


            // Сценарий 2: Администратор (роль "Admin")
            Console.WriteLine("\n--- Сценарий 2: Пользователь с ролью 'Admin' ---");
            IUserDataService adminProxy = new UserDataServiceProxy("Admin");

            // Изменение цели разрешено
            adminProxy.UpdateDailyGoal("user_123", 3000);

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}