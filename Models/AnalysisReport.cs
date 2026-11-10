namespace FileProcessingPipeline.Models;

public sealed class AnalysisReport
{
    public string Mode { get; init; } = "";
    public int TotalFiles { get; init; }
    public int SuccessfulFiles { get; init; }
    public int FailedFiles { get; init; }
    public int WorkerCount { get; init; }
    public long TotalLines { get; init; }
    public long TotalWords { get; init; }
    public long TotalCharacters { get; init; }
    public long ProcessingTimeMs { get; init; }
    public List<WorkerStat> WorkerStatistics { get; init; } = [];
    public List<FileAnalysisResult> Results { get; init; } = [];
    public List<FileProcessingError> Errors { get; init; } = [];
}
