using EmailCreator.Entities;

namespace EmailCreator.Business.Abstract;

public interface ICompanyFactory
{
    Company CreateCompany(
        string companyName,
        string linkedInUrl,
        string companyEmail,
        string domain);
}
