using Microsoft.AspNetCore.Http;

namespace EmailCreator.Models;

public class MailTemplateViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public IFormFile? PdfFile { get; set; }

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public List<MailTemplateListItemViewModel> SavedTemplates { get; set; } = [];

    public bool HasSavedTemplates => SavedTemplates.Count > 0;
}

public class MailTemplateListItemViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string PdfOriginalFileName { get; set; } = string.Empty;

    public string PdfStoragePath { get; set; } = string.Empty;

    public string PdfFileSize { get; set; } = string.Empty;

    public string CreatedAt { get; set; } = string.Empty;

    public string UpdatedAt { get; set; } = string.Empty;
}

public class MailTemplateUpdateViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public IFormFile? PdfFile { get; set; }
}
