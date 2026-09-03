namespace EmailCreator.Entities;

public class CompanyDraft
{
    public int Id { get; set; }

    public string Domain { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string LinkedInUrl { get; set; } = string.Empty;

    public string CompanyEmail { get; set; } = string.Empty;

    public DateTimeOffset CompanyCreatedAt { get; set; }

    public DateTimeOffset DraftCreatedAt { get; set; }
}
