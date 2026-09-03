namespace EmailCreator.Business.Models;

public sealed record CompanyDraftRecord(
    int Id,
    string CompanyName,
    string LinkedInUrl,
    string CompanyEmail,
    string Domain,
    DateTimeOffset CompanyCreatedAt,
    DateTimeOffset DraftCreatedAt);
