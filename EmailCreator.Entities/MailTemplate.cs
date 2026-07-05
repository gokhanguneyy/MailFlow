namespace EmailCreator.Entities;

public class MailTemplate
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string PdfOriginalFileName { get; set; } = string.Empty;

    public string PdfStoredFileName { get; set; } = string.Empty;

    public string PdfStoragePath { get; set; } = string.Empty;

    public long PdfFileSize { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
