using MailFlow.Business.Models;
using Google.Apis.Auth.OAuth2;

namespace MailFlow.Services;

public interface IGmailDraftService
{
    Task<GmailDraftCreationResult> CreateAsync(
        CompanyRecord company,
        MailTemplateRecord mailTemplate,
        GoogleCredential credential,
        string? fromEmail,
        CancellationToken cancellationToken = default);
}
