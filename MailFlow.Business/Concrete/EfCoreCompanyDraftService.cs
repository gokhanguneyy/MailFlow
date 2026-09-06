using Microsoft.EntityFrameworkCore;
using MailFlow.Business.Abstract;
using MailFlow.Business.Models;
using MailFlow.DataAccess.Repositories;
using MailFlow.Entities;

namespace MailFlow.Business.Concrete;

public sealed class EfCoreCompanyDraftService : ICompanyDraftService
{
    private readonly IGenericRepository<Company> _companyRepository;
    private readonly IGenericRepository<CompanyDraft> _companyDraftRepository;

    public EfCoreCompanyDraftService(
        IGenericRepository<Company> companyRepository,
        IGenericRepository<CompanyDraft> companyDraftRepository)
    {
        _companyRepository = companyRepository;
        _companyDraftRepository = companyDraftRepository;
    }

    public async Task<CompanyDraftRecord?> CreateAsync(
        string domain,
        int mailTemplateId,
        string mailSubject,
        string mailBody,
        string gmailDraftId,
        string gmailMessageId)
    {
        var normalizedDomain = NormalizeDomain(domain);

        var existingDraft = await _companyDraftRepository.FirstOrDefaultAsync(
            draft => draft.Domain == normalizedDomain);

        if (existingDraft is not null)
        {
            return ToDraftRecord(existingDraft);
        }

        var company = await _companyRepository.FirstOrDefaultAsync(
            company => company.Domain == normalizedDomain);

        if (company is null)
        {
            return null;
        }

        var draft = new CompanyDraft
        {
            Domain = company.Domain,
            CompanyName = company.CompanyName,
            LinkedInUrl = company.LinkedInUrl,
            CompanyEmail = company.CompanyEmail,
            CompanyCreatedAt = company.CreatedAt,
            MailTemplateId = mailTemplateId,
            MailSubject = mailSubject.Trim(),
            MailBody = mailBody.Trim(),
            GmailDraftId = gmailDraftId,
            GmailMessageId = gmailMessageId,
            DraftCreatedAt = DateTimeOffset.UtcNow
        };

        await _companyDraftRepository.AddAsync(draft);

        try
        {
            await _companyDraftRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            var concurrentDraft = await _companyDraftRepository.FirstOrDefaultAsync(
                draft => draft.Domain == normalizedDomain);

            if (concurrentDraft is null)
            {
                throw;
            }

            return ToDraftRecord(concurrentDraft);
        }

        return ToDraftRecord(draft);
    }

    public async Task<CompanyRecord?> GetAvailableCompanyAsync(string domain)
    {
        var normalizedDomain = NormalizeDomain(domain);

        var existingDraft = await _companyDraftRepository.FirstOrDefaultAsync(
            draft => draft.Domain == normalizedDomain);

        if (existingDraft is not null)
        {
            return null;
        }

        var company = await _companyRepository.FirstOrDefaultAsync(
            company => company.Domain == normalizedDomain);

        return company is null ? null : ToCompanyRecord(company);
    }

    public async Task<IReadOnlyList<CompanyRecord>> GetAvailableCompaniesAsync()
    {
        var drafts = await _companyDraftRepository.ListAsync();
        var draftedDomains = drafts
            .Select(draft => draft.Domain)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var companies = await _companyRepository.ListAsync(
            orderBy: query => query
                .OrderByDescending(company => company.CreatedAt)
                .ThenBy(company => company.Domain));

        return companies
            .Where(company => !draftedDomains.Contains(company.Domain))
            .Select(ToCompanyRecord)
            .ToList();
    }

    public async Task<IReadOnlyList<CompanyDraftRecord>> GetAllAsync()
    {
        var drafts = await _companyDraftRepository.ListAsync(
            orderBy: query => query
                .OrderByDescending(draft => draft.DraftCreatedAt)
                .ThenBy(draft => draft.Domain));

        return drafts
            .Select(ToDraftRecord)
            .ToList();
    }

    private static CompanyRecord ToCompanyRecord(Company company)
    {
        return new CompanyRecord(
            company.CompanyName,
            company.LinkedInUrl,
            company.CompanyEmail,
            company.Domain,
            company.GenderStatus,
            company.CreatedAt);
    }

    private static CompanyDraftRecord ToDraftRecord(CompanyDraft draft)
    {
        return new CompanyDraftRecord(
            draft.Id,
            draft.CompanyName,
            draft.LinkedInUrl,
            draft.CompanyEmail,
            draft.Domain,
            draft.CompanyCreatedAt,
            draft.MailTemplateId,
            draft.MailSubject,
            draft.MailBody,
            draft.GmailDraftId,
            draft.GmailMessageId,
            draft.DraftCreatedAt);
    }

    private static string NormalizeDomain(string domain)
    {
        return domain.Trim().ToLowerInvariant();
    }
}
