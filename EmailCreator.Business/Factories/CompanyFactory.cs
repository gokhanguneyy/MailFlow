using EmailCreator.Business.Abstract;
using EmailCreator.Entities;

namespace EmailCreator.Business.Factories;

public sealed class CompanyFactory : ICompanyFactory
{
    public Company CreateCompany(
        string companyName,
        string linkedInUrl,
        string companyEmail,
        string domain)
    {
        // Company entity'si olusurken ortak veri temizleme ve varsayilan degerler burada verilir.
        // Boylece yeni bir Company uretme kurali degistiginde servisleri tek tek gezmek zorunda kalmayiz.
        return new Company
        {
            Domain = NormalizeDomain(domain),
            CompanyName = companyName.Trim(),
            LinkedInUrl = linkedInUrl.Trim(),
            CompanyEmail = companyEmail.Trim(),
            GenderStatus = GenderStatus.Company,
            CreatedAt = DateTimeOffset.Now
        };
    }

    private static string NormalizeDomain(string domain)
    {
        // Domain benzersiz kimlik olarak kullanildigi icin bosluk ve buyuk/kucuk harf farkini kaldiriyoruz.
        // Ornegin FAIR.COM ile fair.com ayni kayit olarak degerlendirilir.
        return domain.Trim().ToLowerInvariant();
    }
}
