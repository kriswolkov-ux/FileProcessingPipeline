using System.Diagnostics;
using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class SequentialProcessor
{
    public static async Task<AnalysisReport> ProcessAsync(
        IEnumerable<FileTask> tasks)
    {
        var stopwatch = Stopwatch.StartNew();

        var results = new List<FileAnalysisResult>();
        var errors = new List<FileProcessingError>();

        var taskList = tasks.ToList();

        foreach (var task in taskList)
        {
            try
            {
                var result = await FileAnalyzer.AnalyzeAsync(
                    task,
                    1);

                results.Add(result);

                Console.WriteLine(
                    $"[OK] {result.FileName}");
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

        stopwatch.Stop();

        return ReportService.Create(
            "Sequential",
            taskList.Count,
            results,
            errors,
            1,
            stopwatch.ElapsedMilliseconds);
    }
}
