using EmailCreator.Business.Models;

namespace EmailCreator.Business.Abstract;

public interface IMailTemplateService
{
    Task<MailTemplateRecord> AddAsync(
        string title,
        string subject,
        string body,
        string pdfOriginalFileName,
        string pdfStoredFileName,
        string pdfStoragePath,
        long pdfFileSize);

    Task<IReadOnlyList<MailTemplateRecord>> GetAllAsync();

    Task<MailTemplateRecord?> GetByIdAsync(int id);

    Task<MailTemplateRecord?> UpdateAsync(
        int id,
        string title,
        string subject,
        string body,
        string? pdfOriginalFileName = null,
        string? pdfStoredFileName = null,
        string? pdfStoragePath = null,
        long? pdfFileSize = null);

    Task<MailTemplateRecord?> DeleteAsync(int id);
}
