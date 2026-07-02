namespace EmailCreator.Models;

public class CompanyProfileViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public string LinkedInUrl { get; set; } = string.Empty;

    public string CompanyEmail { get; set; } = string.Empty;

    public string? SavedCompanyName { get; set; }

    public string? SavedDomain { get; set; }

    public string? SearchEmail { get; set; }

    public List<CompanyListItemViewModel> SavedCompanies { get; set; } = [];

    public bool HasSavedCompany => !string.IsNullOrWhiteSpace(SavedCompanyName)
        && !string.IsNullOrWhiteSpace(SavedDomain);

    public bool HasSavedCompanies => SavedCompanies.Count > 0;

    public bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchEmail);
}

public class CompanyListItemViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public string LinkedInUrl { get; set; } = string.Empty;

    public string CompanyEmail { get; set; } = string.Empty;

    public string Domain { get; set; } = string.Empty;

    public string CreatedAt { get; set; } = string.Empty;
}
