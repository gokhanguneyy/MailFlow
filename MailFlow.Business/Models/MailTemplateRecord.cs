namespace MailFlow.Business.Models;

public sealed record MailTemplateRecord(
    int Id,
    string Title,
    string Subject,
    string Body,
    string PdfOriginalFileName,
    string PdfStoredFileName,
    string PdfStoragePath,
    long PdfFileSize,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
