using System;
using System.Collections.Generic;
using System.Linq;

namespace AqualyzerLab14
{
    // ============================================================
    // 1. СУЩНОСТЬ (Entity)
    // ============================================================

    public class DrinkRecord
    {
        public int Id { get; set; }
        public string DrinkName { get; set; }
        public int VolumeMl { get; set; }
        public DateTime Timestamp { get; set; }

        public DrinkRecord(string drinkName, int volumeMl)
        {
            DrinkName = drinkName;
            VolumeMl = volumeMl;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Id}] {DrinkName} — {VolumeMl} мл ({Timestamp:HH:mm:ss})";
        }
    }


    // ============================================================
    // 2. ПАТТЕРН: REPOSITORY (Репозиторий)
    // ============================================================

    // Общий интерфейс репозитория
    public interface IRepository<T> where T : class
    {
        List<T> GetAll();
        T GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        int Count();
    }

    // Конкретный репозиторий для записей о напитках
    public class DrinkRecordRepository : IRepository<DrinkRecord>
    {
        private readonly List<DrinkRecord> _storage = new List<DrinkRecord>();
        private int _nextId = 1;

        public List<DrinkRecord> GetAll()
        {
            Console.WriteLine("  [Repository] Запрос всех записей.");
            return new List<DrinkRecord>(_storage);
        }

        public DrinkRecord GetById(int id)
        {
            Console.WriteLine($"  [Repository] Поиск записи по Id={id}.");
            return _storage.FirstOrDefault(r => r.Id == id);
        }

        public void Add(DrinkRecord entity)
        {
            entity.Id = _nextId++;
            _storage.Add(entity);
            Console.WriteLine($"  [Repository] Добавлена запись: {entity}");
        }

        public void Update(DrinkRecord entity)
        {
            int index = _storage.FindIndex(r => r.Id == entity.Id);
            if (index >= 0)
            {
                _storage[index] = entity;
                Console.WriteLine($"  [Repository] Обновлена запись: {entity}");
            }
            else
            {
                Console.WriteLine($"  [Repository] ОШИБКА: запись с Id={entity.Id} не найдена.");
            }
        }

        public void Delete(int id)
        {
            DrinkRecord record = _storage.FirstOrDefault(r => r.Id == id);
            if (record != null)
            {
                _storage.Remove(record);
                Console.WriteLine($"  [Repository] Удалена запись Id={id} ({record.DrinkName}).");
            }
            else
            {
                Console.WriteLine($"  [Repository] ОШИБКА: запись с Id={id} не найдена.");
            }
        }

        public int Count() => _storage.Count;
    }


    // ============================================================
    // 3. ПАТТЕРН: UNIT OF WORK (Единица работы)
    // ============================================================

    // 🔥 ИСПРАВЛЕНИЕ: добавлены методы MarkAsChanged и MarkAsDeleted
    public interface IUnitOfWork : IDisposable
    {
        IRepository<DrinkRecord> GetDrinkRepository();
        void Commit();
        void Rollback();
        void MarkAsChanged(DrinkRecord record);
        void MarkAsDeleted(int id);
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly DrinkRecordRepository _drinkRepository;
        private readonly List<DrinkRecord> _pendingChanges = new List<DrinkRecord>();
        private readonly List<int> _pendingDeletes = new List<int>();
        private bool _isCommitted = false;
        private bool _disposed = false;

        public UnitOfWork()
        {
            _drinkRepository = new DrinkRecordRepository();
            Console.WriteLine("  [UnitOfWork] Создана новая единица работы.");
        }

        public IRepository<DrinkRecord> GetDrinkRepository()
        {
            return _drinkRepository;
        }

        public void MarkAsChanged(DrinkRecord record)
        {
            _pendingChanges.Add(record);
        }

        public void MarkAsDeleted(int id)
        {
            _pendingDeletes.Add(id);
        }

        public void Commit()
        {
            Console.WriteLine("\n  [UnitOfWork] === Начало транзакции (Commit) ===");
            try
            {
                foreach (var record in _pendingChanges)
                {
                    _drinkRepository.Update(record);
                }
                foreach (int id in _pendingDeletes)
                {
                    _drinkRepository.Delete(id);
                }

                _isCommitted = true;
                _pendingChanges.Clear();
                _pendingDeletes.Clear();
                Console.WriteLine("  [UnitOfWork] Транзакция успешно завершена.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [UnitOfWork] ОШИБКА транзакции: {ex.Message}");
                Rollback();
            }
        }

        public void Rollback()
        {
            Console.WriteLine("  [UnitOfWork] Откат транзакции. Изменения отменены.");
            _pendingChanges.Clear();
            _pendingDeletes.Clear();
        }

        public void Dispose()
        {
            if (!_disposed && !_isCommitted)
            {
                Console.WriteLine("  [UnitOfWork] Предупреждение: UnitOfWork завершён без Commit().");
            }
            _disposed = true;
        }
    }


    // ============================================================
    // 4. СЕРВИС БИЗНЕС-ЛОГИКИ (клиент паттернов)
    // ============================================================

    public class HydrationService
    {
        private readonly IUnitOfWork _uow;

        public HydrationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void LogDrinksBatch(List<DrinkRecord> drinks)
        {
            Console.WriteLine($"\n--- Пакетная запись: {drinks.Count} напитков ---");
            var repo = _uow.GetDrinkRepository();

            foreach (var drink in drinks)
            {
                repo.Add(drink);
            }

            _uow.Commit();
        }

        public string GetDailyReport()
        {
            var repo = _uow.GetDrinkRepository();
            var records = repo.GetAll();
            int totalMl = records.Sum(r => r.VolumeMl);

            return $"Всего записей: {repo.Count()}, общий объём: {totalMl} мл.";
        }

        public void ReplaceDrink(int oldId, DrinkRecord newDrink)
        {
            Console.WriteLine("\n--- Операция замены записи ---");
            var repo = _uow.GetDrinkRepository();

            repo.Add(newDrink);
            _uow.MarkAsDeleted(oldId);

            _uow.Commit();
        }
    }


    // ============================================================
    // 5. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №14: Аквалайзер ===\n");
            Console.WriteLine("Паттерны: Repository + Unit of Work\n");

            // --- Демонстрация паттерна REPOSITORY ---
            Console.WriteLine("1. Демонстрация паттерна 'Repository':");
            Console.WriteLine("   (Инкапсуляция доступа к данным)\n");

            using (IUnitOfWork uow = new UnitOfWork())
            {
                IRepository<DrinkRecord> repo = uow.GetDrinkRepository();

                DrinkRecord water = new DrinkRecord("Вода", 500);
                repo.Add(water);

                DrinkRecord tea = new DrinkRecord("Зелёный чай", 300);
                repo.Add(tea);

                DrinkRecord coffee = new DrinkRecord("Кофе", 200);
                repo.Add(coffee);

                Console.WriteLine($"\n  Всего записей в хранилище: {repo.Count()}");

                DrinkRecord found = repo.GetById(2);
                if (found != null)
                {
                    Console.WriteLine($"  Найденная запись: {found}");
                }

                found.VolumeMl = 350;
                repo.Update(found);

                repo.Delete(1);

                Console.WriteLine($"\n  После операций — всего записей: {repo.Count()}");
                Console.WriteLine("  Все оставшиеся записи:");
                foreach (var r in repo.GetAll())
                {
                    Console.WriteLine($"    - {r}");
                }
            }


            // --- Демонстрация паттерна UNIT OF WORK ---
            Console.WriteLine("\n\n2. Демонстрация паттерна 'Unit of Work':");
            Console.WriteLine("   (Атомарные транзакции и пакетные операции)\n");

            using (IUnitOfWork uow2 = new UnitOfWork())
            {
                HydrationService service = new HydrationService(uow2);

                List<DrinkRecord> morningDrinks = new List<DrinkRecord>
                {
                    new DrinkRecord("Вода", 400),
                    new DrinkRecord("Сок", 250),
                    new DrinkRecord("Чай", 300)
                };
                service.LogDrinksBatch(morningDrinks);

                Console.WriteLine($"  Отчёт после утреннего пакета: {service.GetDailyReport()}");

                service.ReplaceDrink(1, new DrinkRecord("Минеральная вода", 500));

                Console.WriteLine($"  Отчёт после замены: {service.GetDailyReport()}");
            }


            // --- Демонстрация отката транзакции ---
            Console.WriteLine("\n\n3. Демонстрация отката транзакции (Rollback):\n");

            using (IUnitOfWork uow3 = new UnitOfWork())
            {
                var repo = uow3.GetDrinkRepository();
                repo.Add(new DrinkRecord("Тестовая запись", 100));

                Console.WriteLine("  Имитация ошибки во время транзакции...");
                uow3.Rollback();

                Console.WriteLine($"  После отката записей в хранилище: {repo.Count()}");
            }

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}