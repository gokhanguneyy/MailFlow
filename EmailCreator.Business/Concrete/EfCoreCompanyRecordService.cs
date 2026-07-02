using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EmailCreator.Business.Abstract;
using EmailCreator.Business.Exceptions;
using EmailCreator.Business.Models;
using EmailCreator.DataAccess.Repositories;
using EmailCreator.Entities;

namespace EmailCreator.Business.Concrete;

public sealed class EfCoreCompanyRecordService : ICompanyRecordService
{
    private readonly IGenericRepository<Company> _companyRepository;

    public EfCoreCompanyRecordService(IGenericRepository<Company> companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<CompanyRecord> AddAsync(
        string companyName,
        string linkedInUrl,
        string companyEmail,
        string domain)
    {
        var normalizedDomain = NormalizeDomain(domain);

        // Domain primary key olduğu için aynı firmayı ikinci kez eklememeliyiz.
        // Ön kontrol kullanıcıya SQL hatası yerine anlaşılır iş kuralı mesajı döndürmemizi sağlar.
        if (await _companyRepository.AnyAsync(company => company.Domain == normalizedDomain))
        {
            throw new DuplicateCompanyDomainException(normalizedDomain);
        }

        var company = new Company
        {
            Domain = normalizedDomain,
            CompanyName = companyName.Trim(),
            LinkedInUrl = linkedInUrl.Trim(),
            CompanyEmail = companyEmail.Trim(),
            GenderStatus = GenderStatus.Company,
            CreatedAt = DateTimeOffset.Now
        };

        await _companyRepository.AddAsync(company);

        try
        {
            await _companyRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Aynı domain eş zamanlı iki istekle gelirse ön kontrol yetmeyebilir.
            // Database primary key hatasını da iş kuralı exception'ına çevirerek UI mesajını koruruz.
            throw new DuplicateCompanyDomainException(normalizedDomain);
        }

        return ToRecord(company);
    }

    public async Task<IReadOnlyList<CompanyRecord>> GetAllAsync(string? domainSearch = null)
    {
        Expression<Func<Company, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(domainSearch))
        {
            var normalizedDomainSearch = NormalizeDomain(domainSearch);

            // Search mailin tamamından değil domain parçasından çalışır.
            // Böylece gokhan@fair.com araması info@fair.com kaydını da bulur.
            filter = company => company.Domain.Contains(normalizedDomainSearch);
        }

        var companies = await _companyRepository.ListAsync(
            filter,
            query => query
                .OrderByDescending(company => company.CreatedAt)
                .ThenBy(company => company.Domain));

        return companies
            .Select(company => ToRecord(company))
            .ToList();
    }

    public async Task<CompanyRecord?> GetLatestAsync()
    {
        var company = await _companyRepository.FirstOrDefaultAsync(
            orderBy: query => query.OrderByDescending(company => company.CreatedAt));

        return company is null ? null : ToRecord(company);
    }

    private static CompanyRecord ToRecord(Company company)
    {
        return new CompanyRecord(
            company.CompanyName,
            company.LinkedInUrl,
            company.CompanyEmail,
            company.Domain,
            company.GenderStatus,
            company.CreatedAt);
    }

    private static string NormalizeDomain(string domain)
    {
        return domain.Trim().ToLowerInvariant();
    }
}
