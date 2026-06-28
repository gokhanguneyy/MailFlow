using Microsoft.EntityFrameworkCore;
using EmailCreator.Business.Abstract;
using EmailCreator.Business.Exceptions;
using EmailCreator.Business.Models;
using EmailCreator.DataAccess.Contexts;
using EmailCreator.Entities;

namespace EmailCreator.Business.Concrete;

public sealed class EfCoreCompanyRecordService : ICompanyRecordService
{
    private readonly EmailCreatorDbContext _dbContext;

    public EfCoreCompanyRecordService(EmailCreatorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CompanyRecord> AddAsync(
        string companyName,
        string linkedInUrl,
        string companyEmail,
        string domain)
    {
        var normalizedDomain = NormalizeDomain(domain);

        if (await _dbContext.Companies.AnyAsync(company => company.Domain == normalizedDomain))
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

        _dbContext.Companies.Add(company);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new DuplicateCompanyDomainException(normalizedDomain);
        }

        return ToRecord(company);
    }

    public async Task<IReadOnlyList<CompanyRecord>> GetAllAsync(string? domainSearch = null)
    {
        var query = _dbContext.Companies
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(domainSearch))
        {
            var normalizedDomainSearch = NormalizeDomain(domainSearch);

            query = query.Where(company => company.Domain.Contains(normalizedDomainSearch));
        }

        return await query
            .OrderByDescending(company => company.CreatedAt)
            .ThenBy(company => company.Domain)
            .Select(company => ToRecord(company))
            .ToListAsync();
    }

    public async Task<CompanyRecord?> GetLatestAsync()
    {
        var company = await _dbContext.Companies
            .AsNoTracking()
            .OrderByDescending(company => company.CreatedAt)
            .FirstOrDefaultAsync();

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
