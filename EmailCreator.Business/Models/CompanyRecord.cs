using EmailCreator.Entities;

namespace EmailCreator.Business.Models;

public sealed record CompanyRecord(
    string CompanyName,
    string LinkedInUrl,
    string CompanyEmail,
    string Domain,
    GenderStatus GenderStatus,
    DateTimeOffset CreatedAt);
