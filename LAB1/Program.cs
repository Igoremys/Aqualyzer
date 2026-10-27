using System;
using System.Collections.Generic;

// ============================================================
// 1. ПАТТЕРН ОДИНОЧКА (Singleton)
// ============================================================
public sealed class SettingsManager
{
    private static readonly Lazy<SettingsManager> _lazy =
        new Lazy<SettingsManager>(() => new SettingsManager());

    public static SettingsManager Instance => _lazy.Value;

    private readonly Dictionary<string, object> _settings;

    private SettingsManager()
    {
        _settings = new Dictionary<string, object>
        {
            { "DefaultGoalMl", 2000 },
            { "ReminderIntervalMin", 60 },
            { "Unit", "мл" }
        };
    }

    public object GetSetting(string key) =>
        _settings.TryGetValue(key, out var value) ? value : null;

    public void SetSetting(string key, object value) =>
        _settings[key] = value;
}

// ============================================================
// 2. ПРОДУКТ — класс отчёта
// ============================================================
public class HydrationReport
{
    public string UserName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int TargetMl { get; set; }
    public int ConsumedMl { get; set; }
    public bool RemindersEnabled { get; set; }
    public string Summary { get; set; } = string.Empty;

    private string GetUnit() =>
        SettingsManager.Instance.GetSetting("Unit")?.ToString() ?? "мл";

    public override string ToString()
    {
        string status = RemindersEnabled ? "Включены" : "Выключены";
        string unit = GetUnit();
        return
$@"=== Отчёт приложения 'Аквалайзер' ===
Пользователь : {UserName}
Дата         : {Date}
Цель         : {TargetMl} {unit}
Выпито       : {ConsumedMl} {unit}
Напоминания  : {status}
Итог         : {Summary}
======================================";
    }
}

// ============================================================
// 3. ПАТТЕРН СТРОИТЕЛЬ (Builder)
// ============================================================
public class HydrationReportBuilder
{
    private readonly HydrationReport _report = new HydrationReport();

    public HydrationReportBuilder SetUserName(string name)
    {
        _report.UserName = name;
        return this;
    }

    public HydrationReportBuilder SetDate(string date)
    {
        _report.Date = date;
        return this;
    }

    public HydrationReportBuilder SetTarget(int targetMl)
    {
        _report.TargetMl = targetMl;
        return this;
    }

    public HydrationReportBuilder SetConsumed(int consumedMl)
    {
        _report.ConsumedMl = consumedMl;
        return this;
    }

    public HydrationReportBuilder EnableReminders()
    {
        _report.RemindersEnabled = true;
        return this;
    }

    public HydrationReportBuilder GenerateSummary()
    {
        int remaining = _report.TargetMl - _report.ConsumedMl;
        _report.Summary = remaining > 0
            ? $"Осталось выпить: {remaining} мл. Продолжайте в том же духе!"
            : "Дневная цель достигнута! Отличная работа!";
        return this;
    }

    public HydrationReport Build() => _report;
}

// ============================================================
// 4. ТОЧКА ВХОДА (top-level statements)
// ============================================================
Console.WriteLine("--- Инициализация приложения 'Аквалайзер' ---\n");

// Демонстрация Singleton
var settings1 = SettingsManager.Instance;
var settings2 = SettingsManager.Instance;

Console.WriteLine($"1. Проверка Одиночки: ReferenceEquals -> {ReferenceEquals(settings1, settings2)}");
Console.WriteLine($"   Цель по умолчанию: {settings1.GetSetting("DefaultGoalMl")} мл");

settings1.SetSetting("DefaultGoalMl", 2500);
Console.WriteLine($"   Цель после изменения: {settings2.GetSetting("DefaultGoalMl")} мл (изменилась глобально)\n");

// Демонстрация Builder
var report = new HydrationReportBuilder()
    .SetUserName("Мусхажиев Игорь")
    .SetDate("27.10.2026")
    .SetTarget((int)settings1.GetSetting("DefaultGoalMl"))
    .SetConsumed(1800)
    .EnableReminders()
    .GenerateSummary()
    .Build();

Console.WriteLine("2. Сгенерированный отчёт:\n");
Console.WriteLine(report);