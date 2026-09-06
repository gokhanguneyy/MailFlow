using MailFlow.Entities;

namespace MailFlow.Business.Models;

public sealed record CompanyRecord(
    string CompanyName,
    string LinkedInUrl,
    string CompanyEmail,
    string Domain,
    GenderStatus GenderStatus,
    DateTimeOffset CreatedAt);
