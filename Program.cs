using System.Text.Json;
using FileProcessingPipeline.Models;
using FileProcessingPipeline.Services;

namespace FileProcessingPipeline;

internal class Program
{
    private static async Task Main()
    {
        Console.WriteLine("================================");
        Console.WriteLine("     FILE PROCESSING PIPELINE");
        Console.WriteLine("================================");

        string dataDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data");

        Directory.CreateDirectory(dataDirectory);

        string[] paths = Directory.GetFiles(
                dataDirectory,
                "*.txt")
            .OrderBy(path => path)
            .ToArray();

        if (paths.Length == 0)
        {
            Console.WriteLine("Нет файлов для обработки.");
            return;
        }

        var tasks = paths
            .Select((path, index) =>
                new FileTask(index + 1, path))
            .ToList();

        Console.WriteLine(
            $"Найдено файлов: {tasks.Count}");

        Console.WriteLine();
        Console.WriteLine("1. Последовательная обработка");
        Console.WriteLine("2. Параллельная обработка");
        Console.WriteLine("3. Сравнение конфигураций");
        Console.WriteLine("4. Обработка с ошибкой");

        Console.Write("Выберите режим: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
            {
                AnalysisReport report =
                    await SequentialProcessor.ProcessAsync(tasks);

                PrintReport(report);
                await SaveReportAsync(report);

                break;
            }

            case "2":
            {
                Console.Write("Количество обработчиков: ");

                if (!int.TryParse(
                        Console.ReadLine(),
                        out int workerCount) ||
                    workerCount < 1)
                {
                    Console.WriteLine("Некорректное значение.");
                    return;
                }

                AnalysisReport report =
                    await PipelineProcessor.ProcessAsync(
                        tasks,
                        workerCount);

                PrintReport(report);
                await SaveReportAsync(report);

                break;
            }

            case "3":
            {
                await CompareConfigurationsAsync(tasks);
                break;
            }

            case "4":
            {
                await RunErrorScenarioAsync(dataDirectory);
                break;
            }

            default:
            {
                Console.WriteLine("Неизвестный режим.");
                break;
            }
        }
    }

    private static async Task CompareConfigurationsAsync(
        List<FileTask> tasks)
    {
        Console.WriteLine();
        Console.WriteLine("Прогрев перед замерами...");

        await SequentialProcessor.ProcessAsync(tasks);
        await PipelineProcessor.ProcessAsync(tasks, 1);

        Console.WriteLine();
        Console.WriteLine(
            "Режим      | Воркеры | Время, мс | Успешно | Ошибок");
        Console.WriteLine(
            "---------------------------------------------------");

        AnalysisReport sequential =
            await SequentialProcessor.ProcessAsync(tasks);

        var reports = new List<AnalysisReport> { sequential };

        PrintComparisonRow("Sequential", 1, sequential);

        foreach (int workerCount in new[] { 1, 2, 4, 8 })
        {
            AnalysisReport report =
                await PipelineProcessor.ProcessAsync(
                    tasks,
                    workerCount);

            reports.Add(report);

            PrintComparisonRow("Parallel", workerCount, report);
        }

        bool totalsMatch = reports.All(report =>
            report.TotalLines == sequential.TotalLines &&
            report.TotalWords == sequential.TotalWords &&
            report.TotalCharacters == sequential.TotalCharacters);

        Console.WriteLine();
        Console.WriteLine($"Итоги совпадают во всех режимах: {totalsMatch}");
        Console.WriteLine(
            "Успешно и ошибок должно быть одинаково во всех режимах.");
    }

    private static void PrintComparisonRow(
        string mode,
        int workers,
        AnalysisReport report)
    {
        Console.WriteLine(
            $"{mode,-10} | {workers,7} | " +
            $"{report.ProcessingTimeMs,9} | " +
            $"{report.SuccessfulFiles,7} | " +
            $"{report.FailedFiles,6}");
    }

    private static async Task RunErrorScenarioAsync(
        string dataDirectory)
    {
        var tasks = new List<FileTask>
        {
            new(1, Path.Combine(dataDirectory, "file1.txt")),
            new(2, Path.Combine(dataDirectory, "file2.txt")),
            new(3, Path.Combine(dataDirectory, "missing.txt")),
            new(4, Path.Combine(dataDirectory, "file3.txt")),
            new(5, Path.Combine(dataDirectory, "file4.txt"))
        };

        AnalysisReport report =
            await PipelineProcessor.ProcessAsync(tasks, 2);

        PrintReport(report);
        await SaveReportAsync(report);
    }

    private static void PrintReport(AnalysisReport report)
    {
        Console.WriteLine();
        Console.WriteLine("========== ОТЧЁТ ==========");
        Console.WriteLine($"Режим: {report.Mode}");
        Console.WriteLine($"Всего файлов: {report.TotalFiles}");
        Console.WriteLine($"Успешно: {report.SuccessfulFiles}");
        Console.WriteLine($"Ошибок: {report.FailedFiles}");
        Console.WriteLine($"Обработчиков: {report.WorkerCount}");
        Console.WriteLine($"Строк: {report.TotalLines}");
        Console.WriteLine($"Слов: {report.TotalWords}");
        Console.WriteLine($"Символов: {report.TotalCharacters}");
        Console.WriteLine($"Время: {report.ProcessingTimeMs} мс");

        Console.WriteLine();
        Console.WriteLine("РЕЗУЛЬТАТЫ:");

        foreach (var result in report.Results)
        {
            Console.WriteLine(
                $"{result.FileName} | " +
                $"Worker: {result.WorkerId} | " +
                $"Строк: {result.Lines} | " +
                $"Слов: {result.Words}");
        }

        if (report.Errors.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("ОШИБКИ:");

            foreach (var error in report.Errors)
            {
                Console.WriteLine(
                    $"{error.FileName}: " +
                    $"{error.ErrorType} - {error.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("========== СТАТИСТИКА WORKERS ==========");

        foreach (var stat in report.WorkerStatistics)
        {
            Console.WriteLine(
                $"Worker {stat.WorkerId}: {stat.TaskCount} заданий");
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Всего обработано: {report.SuccessfulFiles}");
        Console.WriteLine(
            "=======================================");
    }

    private static async Task SaveReportAsync(
        AnalysisReport report)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(
            report,
            options);

        string path = Path.Combine(
            AppContext.BaseDirectory,
            "report.json");

        await File.WriteAllTextAsync(path, json);

        Console.WriteLine();
        Console.WriteLine($"Отчёт сохранён: {path}");
    }
}
