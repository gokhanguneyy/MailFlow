namespace MailFlow.Entities;

public class CompanyDraft
{
    public int Id { get; set; }

    public string Domain { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string LinkedInUrl { get; set; } = string.Empty;

    public string CompanyEmail { get; set; } = string.Empty;

    public DateTimeOffset CompanyCreatedAt { get; set; }

    public int MailTemplateId { get; set; }

    public string MailSubject { get; set; } = string.Empty;

    public string MailBody { get; set; } = string.Empty;

    public string GmailDraftId { get; set; } = string.Empty;

    public string GmailMessageId { get; set; } = string.Empty;

    public DateTimeOffset DraftCreatedAt { get; set; }
}
