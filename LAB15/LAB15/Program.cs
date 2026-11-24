using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AqualyzerLab15
{
    // ============================================================
    // 1. МОДЕЛЬ ДАННЫХ (Событие гидратации)
    // ============================================================

    public class HydrationEvent
    {
        private static int _globalId = 0;

        public int EventId { get; }
        public string Source { get; }
        public int VolumeMl { get; }
        public DateTime Timestamp { get; }

        public HydrationEvent(string source, int volumeMl)
        {
            EventId = Interlocked.Increment(ref _globalId);
            Source = source;
            VolumeMl = volumeMl;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[Event #{EventId}] {Source}: {VolumeMl} мл ({Timestamp:HH:mm:ss.fff})";
        }
    }


    // ============================================================
    // 2. ОБЩИЙ БУФЕР (Ручная реализация потокобезопасной очереди)
    // 🔥 ИСПРАВЛЕНИЕ: Используем List<T> вместо Queue<T> для обхода 
    // ошибки среды компиляции. Логика очереди сохраняется.
    // ============================================================
    public class EventBuffer
    {
        private readonly List<HydrationEvent> _list = new List<HydrationEvent>();
        private readonly int _capacity;
        private bool _isCompleted = false;

        public EventBuffer(int capacity = 10)
        {
            _capacity = capacity;
        }

        public void AddEvent(HydrationEvent evt)
        {
            lock (_list)
            {
                // Если очередь полна, ждем, пока потребитель заберет элемент
                while (_list.Count >= _capacity)
                {
                    Monitor.Wait(_list);
                }
                _list.Add(evt); // Аналог Enqueue
                // Оповещаем потребителей, что появился новый элемент
                Monitor.PulseAll(_list);
            }
        }

        public bool TryGetEvent(out HydrationEvent evt, CancellationToken token)
        {
            lock (_list)
            {
                // Если очередь пуста и не завершена, ждем
                while (_list.Count == 0 && !_isCompleted)
                {
                    // Ждем с таймаутом, чтобы периодически проверять CancellationToken
                    if (!Monitor.Wait(_list, 100))
                    {
                        if (token.IsCancellationRequested)
                        {
                            evt = null;
                            return false;
                        }
                    }
                }

                if (_list.Count > 0)
                {
                    evt = _list[0];         // Берем первый элемент
                    _list.RemoveAt(0);      // Аналог Dequeue
                    // Оповещаем производителей, что появилось свободное место
                    Monitor.PulseAll(_list);
                    return true;
                }

                evt = null;
                return false;
            }
        }

        public void CompleteAdding()
        {
            lock (_list)
            {
                _isCompleted = true;
                Monitor.PulseAll(_list); // Разблокируем всех ждущих потребителей
            }
        }
    }


    // ============================================================
    // 3. ПРОИЗВОДИТЕЛИ (Producers)
    // ============================================================

    public interface IProducer
    {
        Task ProduceAsync(EventBuffer buffer, CancellationToken token);
    }

    public class SmartBottleProducer : IProducer
    {
        public async Task ProduceAsync(EventBuffer buffer, CancellationToken token)
        {
            Console.WriteLine("  [SmartBottle Producer] Запущен.");
            int[] volumes = { 150, 200, 250, 100 };
            int index = 0;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int volume = volumes[index % volumes.Length];
                    var evt = new HydrationEvent("SmartBottle", volume);

                    buffer.AddEvent(evt);
                    Console.WriteLine($"  [Producer] SmartBottle добавила событие: {volume} мл");

                    index++;
                    await Task.Delay(800, token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("  [SmartBottle Producer] Остановлен.");
            }
        }
    }

    public class FitnessTrackerProducer : IProducer
    {
        public async Task ProduceAsync(EventBuffer buffer, CancellationToken token)
        {
            Console.WriteLine("  [FitnessTracker Producer] Запущен.");

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var evt = new HydrationEvent("FitnessTracker", 300);
                    buffer.AddEvent(evt);
                    Console.WriteLine($"  [Producer] FitnessTracker добавил событие: 300 мл");

                    await Task.Delay(1500, token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("  [FitnessTracker Producer] Остановлен.");
            }
        }
    }


    // ============================================================
    // 4. ПОТРЕБИТЕЛИ (Consumers)
    // ============================================================

    public interface IConsumer
    {
        Task ConsumeAsync(EventBuffer buffer, CancellationToken token);
    }

    public class DatabaseLoggerConsumer : IConsumer
    {
        public async Task ConsumeAsync(EventBuffer buffer, CancellationToken token)
        {
            Console.WriteLine("  [DB Logger Consumer] Запущен.");

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (buffer.TryGetEvent(out var evt, token))
                    {
                        await Task.Delay(500, token); // Имитация записи в БД
                        Console.WriteLine($"  [Consumer DB] Сохранено в БД: {evt}");
                    }
                    else
                    {
                        break; // Очередь пуста и завершена
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("  [DB Logger Consumer] Остановлен.");
            }
        }
    }

    public class AnalyticsConsumer : IConsumer
    {
        public async Task ConsumeAsync(EventBuffer buffer, CancellationToken token)
        {
            Console.WriteLine("  [Analytics Consumer] Запущен.");

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (buffer.TryGetEvent(out var evt, token))
                    {
                        await Task.Delay(700, token); // Имитация анализа
                        Console.WriteLine($"  [Consumer Analytics] Проанализировано: {evt}");
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("  [Analytics Consumer] Остановлен.");
            }
        }
    }


    // ============================================================
    // 5. ОРКЕСТРАТОР СИСТЕМЫ
    // ============================================================

    public class AqualyzerSystem
    {
        private readonly EventBuffer _buffer;
        private readonly List<IProducer> _producers;
        private readonly List<IConsumer> _consumers;
        private CancellationTokenSource _cts;

        public AqualyzerSystem()
        {
            _buffer = new EventBuffer(capacity: 5);
            _producers = new List<IProducer> { new SmartBottleProducer(), new FitnessTrackerProducer() };
            _consumers = new List<IConsumer> { new DatabaseLoggerConsumer(), new AnalyticsConsumer() };
            _cts = new CancellationTokenSource();
        }

        public async Task RunAsync(int durationSeconds)
        {
            Console.WriteLine("=== Запуск системы Аквалайзер (Producer-Consumer) ===\n");

            var consumerTasks = new List<Task>();
            foreach (var consumer in _consumers)
            {
                consumerTasks.Add(Task.Run(() => consumer.ConsumeAsync(_buffer, _cts.Token)));
            }

            var producerTasks = new List<Task>();
            foreach (var producer in _producers)
            {
                producerTasks.Add(Task.Run(() => producer.ProduceAsync(_buffer, _cts.Token)));
            }

            Console.WriteLine($"Система работает {durationSeconds} секунд...\n");
            await Task.Delay(TimeSpan.FromSeconds(durationSeconds));

            Console.WriteLine("\n=== Остановка системы ===");
            _cts.Cancel();
            _buffer.CompleteAdding();

            await Task.WhenAll(producerTasks);
            await Task.WhenAll(consumerTasks);

            Console.WriteLine("\nВсе потоки завершили работу.");
        }
    }


    // ============================================================
    // 6. ТОЧКА ВХОДА
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Лабораторная работа №15: Аквалайзер (Producer-Consumer)\n");

            var system = new AqualyzerSystem();
            system.RunAsync(durationSeconds: 5).Wait();

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}