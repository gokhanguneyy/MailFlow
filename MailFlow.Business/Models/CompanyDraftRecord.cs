namespace MailFlow.Business.Models;

public sealed record CompanyDraftRecord(
    int Id,
    string CompanyName,
    string LinkedInUrl,
    string CompanyEmail,
    string Domain,
    DateTimeOffset CompanyCreatedAt,
    int MailTemplateId,
    string MailSubject,
    string MailBody,
    string GmailDraftId,
    string GmailMessageId,
    DateTimeOffset DraftCreatedAt);
