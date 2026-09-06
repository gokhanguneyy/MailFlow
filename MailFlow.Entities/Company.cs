namespace MailFlow.Entities;

public class Company
{
    public string Domain { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string LinkedInUrl { get; set; } = string.Empty;

    public string CompanyEmail { get; set; } = string.Empty;

    public GenderStatus GenderStatus { get; set; } = GenderStatus.Company;

    public DateTimeOffset CreatedAt { get; set; }
}
