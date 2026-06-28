using EmailCreator.Business.Models;

namespace EmailCreator.Business.Abstract;

public interface ICompanyRecordService
{
    Task<CompanyRecord> AddAsync(
        string companyName,
        string linkedInUrl,
        string companyEmail,
        string domain);

    // IReadOnlyList olarak tanýmlayarak þunu diyoruz, ben sadece bu listi okuyabilirim,
    // deðiþtiremem. Normal List'te deðeri manipüle edebiliyoruz.
    Task<IReadOnlyList<CompanyRecord>> GetAllAsync(string? domainSearch = null);

    Task<CompanyRecord?> GetLatestAsync();
}
