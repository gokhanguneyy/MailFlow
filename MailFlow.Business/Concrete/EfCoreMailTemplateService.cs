using MailFlow.Business.Abstract;
using MailFlow.Business.Models;
using MailFlow.DataAccess.Repositories;
using MailFlow.Entities;

namespace MailFlow.Business.Concrete;

public sealed class EfCoreMailTemplateService : IMailTemplateService
{
    private readonly IGenericRepository<MailTemplate> _mailTemplateRepository;

    public EfCoreMailTemplateService(IGenericRepository<MailTemplate> mailTemplateRepository)
    {
        _mailTemplateRepository = mailTemplateRepository;
    }

    public async Task<MailTemplateRecord> AddAsync(
        string title,
        string subject,
        string body,
        string pdfOriginalFileName,
        string pdfStoredFileName,
        string pdfStoragePath,
        long pdfFileSize)
    {
        var now = DateTimeOffset.UtcNow;
        var mailTemplate = new MailTemplate
        {
            Title = title.Trim(),
            Subject = subject.Trim(),
            Body = body.Trim(),
            PdfOriginalFileName = pdfOriginalFileName,
            PdfStoredFileName = pdfStoredFileName,
            PdfStoragePath = pdfStoragePath,
            PdfFileSize = pdfFileSize,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _mailTemplateRepository.AddAsync(mailTemplate);
        await _mailTemplateRepository.SaveChangesAsync();

        return ToRecord(mailTemplate);
    }

    public async Task<IReadOnlyList<MailTemplateRecord>> GetAllAsync()
    {
        var mailTemplates = await _mailTemplateRepository.ListAsync(
            orderBy: query => query
                .OrderByDescending(mailTemplate => mailTemplate.CreatedAt)
                .ThenBy(mailTemplate => mailTemplate.Title));

        return mailTemplates
            .Select(ToRecord)
            .ToList();
    }

    public async Task<MailTemplateRecord?> GetByIdAsync(int id)
    {
        var mailTemplate = await _mailTemplateRepository.FirstOrDefaultAsync(
            mailTemplate => mailTemplate.Id == id);

        return mailTemplate is null ? null : ToRecord(mailTemplate);
    }

    public async Task<MailTemplateRecord?> UpdateAsync(
        int id,
        string title,
        string subject,
        string body,
        string? pdfOriginalFileName = null,
        string? pdfStoredFileName = null,
        string? pdfStoragePath = null,
        long? pdfFileSize = null)
    {
        var mailTemplate = await _mailTemplateRepository.FirstOrDefaultAsync(
            mailTemplate => mailTemplate.Id == id,
            asNoTracking: false);

        if (mailTemplate is null)
        {
            return null;
        }

        mailTemplate.Title = title.Trim();
        mailTemplate.Subject = subject.Trim();
        mailTemplate.Body = body.Trim();
        mailTemplate.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(pdfOriginalFileName)
            && !string.IsNullOrWhiteSpace(pdfStoredFileName)
            && !string.IsNullOrWhiteSpace(pdfStoragePath)
            && pdfFileSize.HasValue)
        {
            mailTemplate.PdfOriginalFileName = pdfOriginalFileName;
            mailTemplate.PdfStoredFileName = pdfStoredFileName;
            mailTemplate.PdfStoragePath = pdfStoragePath;
            mailTemplate.PdfFileSize = pdfFileSize.Value;
        }

        await _mailTemplateRepository.SaveChangesAsync();

        return ToRecord(mailTemplate);
    }

    public async Task<MailTemplateRecord?> DeleteAsync(int id)
    {
        var mailTemplate = await _mailTemplateRepository.FirstOrDefaultAsync(
            mailTemplate => mailTemplate.Id == id,
            asNoTracking: false);

        if (mailTemplate is null)
        {
            return null;
        }

        var record = ToRecord(mailTemplate);

        _mailTemplateRepository.Delete(mailTemplate);
        await _mailTemplateRepository.SaveChangesAsync();

        return record;
    }

    private static MailTemplateRecord ToRecord(MailTemplate mailTemplate)
    {
        return new MailTemplateRecord(
            mailTemplate.Id,
            mailTemplate.Title,
            mailTemplate.Subject,
            mailTemplate.Body,
            mailTemplate.PdfOriginalFileName,
            mailTemplate.PdfStoredFileName,
            mailTemplate.PdfStoragePath,
            mailTemplate.PdfFileSize,
            mailTemplate.CreatedAt,
            mailTemplate.UpdatedAt);
    }
}
