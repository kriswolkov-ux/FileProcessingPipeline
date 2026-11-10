using System.Diagnostics;
using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class FileAnalyzer
{
    public static async Task<FileAnalysisResult> AnalyzeAsync(
        FileTask task,
        int workerId)
    {
        var stopwatch = Stopwatch.StartNew();

        string content = await File.ReadAllTextAsync(
            task.FilePath);

        var statistics = TextCounter.Count(content);

        stopwatch.Stop();

        return new FileAnalysisResult(
            TaskId: task.Id,
            FileName: Path.GetFileName(task.FilePath),
            Lines: statistics.Lines,
            Words: statistics.Words,
            Characters: statistics.Characters,
            ProcessingTimeMs: stopwatch.ElapsedMilliseconds,
            WorkerId: workerId
        );
    }
}
