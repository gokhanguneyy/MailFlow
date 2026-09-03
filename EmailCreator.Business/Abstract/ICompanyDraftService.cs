using EmailCreator.Business.Models;

namespace EmailCreator.Business.Abstract;

public interface ICompanyDraftService
{
    Task<CompanyDraftRecord?> CreateAsync(string domain);

    Task<IReadOnlyList<CompanyRecord>> GetAvailableCompaniesAsync();

    Task<IReadOnlyList<CompanyDraftRecord>> GetAllAsync();
}
