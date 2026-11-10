namespace FileProcessingPipeline.Models;

public sealed record FileAnalysisResult(
    int TaskId,
    string FileName,
    int Lines,
    int Words,
    long Characters,
    long ProcessingTimeMs,
    int WorkerId
);
