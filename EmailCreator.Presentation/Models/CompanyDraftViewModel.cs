namespace EmailCreator.Models;

public class CompanyDraftWorkspaceViewModel
{
    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public List<CompanyListItemViewModel> AvailableCompanies { get; set; } = [];

    public List<CompanyDraftListItemViewModel> CreatedDrafts { get; set; } = [];

    public bool HasAvailableCompanies => AvailableCompanies.Count > 0;

    public bool HasCreatedDrafts => CreatedDrafts.Count > 0;
}

public class CompanyDraftListItemViewModel
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string LinkedInUrl { get; set; } = string.Empty;

    public string CompanyEmail { get; set; } = string.Empty;

    public string Domain { get; set; } = string.Empty;

    public string CompanyCreatedAt { get; set; } = string.Empty;

    public string DraftCreatedAt { get; set; } = string.Empty;
}
