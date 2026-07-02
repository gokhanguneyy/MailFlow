using EmailCreator.Business.Models;

namespace EmailCreator.Business.Abstract;

public interface ICompanyRecordService
{
    Task<CompanyRecord> AddAsync(
        string companyName,
        string linkedInUrl,
        string companyEmail,
        string domain);

    // IReadOnlyList kullanarak bu listenin sadece okunmasini sagliyoruz.
    // Boylece presentation katmani gelen koleksiyonu yanlislikla degistiremez.
    Task<IReadOnlyList<CompanyRecord>> GetAllAsync(string? domainSearch = null);

    Task DeleteAsync(string domain);
}
