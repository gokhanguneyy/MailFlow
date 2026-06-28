using EmailCreator.Business.Models;

namespace EmailCreator.Business.Abstract;

public interface ICompanyRecordService
{
    Task<CompanyRecord> AddAsync(
        string companyName,
        string linkedInUrl,
        string companyEmail,
        string domain);

    Task<IReadOnlyList<CompanyRecord>> GetAllAsync(string? domainSearch = null);

    Task<CompanyRecord?> GetLatestAsync();
}
