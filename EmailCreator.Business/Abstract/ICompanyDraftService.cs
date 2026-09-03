using EmailCreator.Business.Models;

namespace EmailCreator.Business.Abstract;

public interface ICompanyDraftService
{
    Task<CompanyDraftRecord?> CreateAsync(
        string domain,
        int mailTemplateId,
        string mailSubject,
        string mailBody,
        string gmailDraftId,
        string gmailMessageId);

    Task<CompanyRecord?> GetAvailableCompanyAsync(string domain);

    Task<IReadOnlyList<CompanyRecord>> GetAvailableCompaniesAsync();

    Task<IReadOnlyList<CompanyDraftRecord>> GetAllAsync();
}
