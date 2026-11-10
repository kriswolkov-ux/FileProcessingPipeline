using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class ReportService
{
    public static AnalysisReport Create(
        string mode,
        int totalFiles,
        List<FileAnalysisResult> results,
        List<FileProcessingError> errors,
        int workerCount,
        long processingTimeMs)
    {
        return new AnalysisReport
        {
            Mode = mode,
            TotalFiles = totalFiles,
            SuccessfulFiles = results.Count,
            FailedFiles = errors.Count,
            WorkerCount = workerCount,
            TotalLines = results.Sum(x => (long)x.Lines),
            TotalWords = results.Sum(x => (long)x.Words),
            TotalCharacters = results.Sum(x => x.Characters),
            ProcessingTimeMs = processingTimeMs,
            WorkerStatistics = Enumerable
                .Range(1, workerCount)
                .Select(id => new WorkerStat(
                    id,
                    results.Count(r => r.WorkerId == id)))
                .ToList(),
            Results = results
                .OrderBy(x => x.FileName)
                .ToList(),

            Errors = errors
                .OrderBy(x => x.FileName)
                .ToList()
        };
    }
}
