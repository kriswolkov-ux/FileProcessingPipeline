namespace FileProcessingPipeline.Models;

public sealed record WorkerStat(
    int WorkerId,
    int TaskCount
);
