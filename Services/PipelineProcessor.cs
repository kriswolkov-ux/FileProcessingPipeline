using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class PipelineProcessor
{
    public static async Task<AnalysisReport> ProcessAsync(
        IEnumerable<FileTask> tasks,
        int workerCount)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        if (workerCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(workerCount));
        }

        var taskList = tasks.ToList();

        var results = new ConcurrentBag<FileAnalysisResult>();
        var errors = new ConcurrentBag<FileProcessingError>();

        var channel = Channel.CreateUnbounded<FileTask>();

        var stopwatch = Stopwatch.StartNew();

        Task[] workers = Enumerable
            .Range(1, workerCount)
            .Select(id => WorkerLoopAsync(
                id,
                channel.Reader,
                results,
                errors))
            .ToArray();

        foreach (var task in taskList)
        {
            await channel.Writer.WriteAsync(task);
        }

        channel.Writer.TryComplete();

        await Task.WhenAll(workers);

        stopwatch.Stop();

        return ReportService.Create(
            $"Pipeline ({workerCount} workers)",
            taskList.Count,
            results.ToList(),
            errors.ToList(),
            workerCount,
            stopwatch.ElapsedMilliseconds);
    }

    private static async Task WorkerLoopAsync(
        int workerId,
        ChannelReader<FileTask> reader,
        ConcurrentBag<FileAnalysisResult> results,
        ConcurrentBag<FileProcessingError> errors)
    {
        await foreach (var task in reader.ReadAllAsync())
        {
            await ProcessTaskAsync(
                workerId,
                task,
                results,
                errors);
        }
    }

    private static async Task ProcessTaskAsync(
        int workerId,
        FileTask task,
        ConcurrentBag<FileAnalysisResult> results,
        ConcurrentBag<FileProcessingError> errors)
    {
        try
        {
            Console.WriteLine(
                $"[WORKER {workerId}] Начал: {task.FilePath}");

            var result = await FileAnalyzer.AnalyzeAsync(
                task,
                workerId);

            results.Add(result);

            Console.WriteLine(
                $"[WORKER {workerId}] Завершил: {result.FileName}");
        }
        catch (IOException ex)
        {
            errors.Add(new FileProcessingError(
                task.Id,
                Path.GetFileName(task.FilePath),
                nameof(IOException),
                ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            errors.Add(new FileProcessingError(
                task.Id,
                Path.GetFileName(task.FilePath),
                nameof(UnauthorizedAccessException),
                ex.Message));
        }
    }
}
