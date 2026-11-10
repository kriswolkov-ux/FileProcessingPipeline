namespace FileProcessingPipeline.Models;

public sealed record FileProcessingError(
    int TaskId,
    string FileName,
    string ErrorType,
    string Message
);
