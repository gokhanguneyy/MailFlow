using System.Text;
using EmailCreator.Business.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;

namespace EmailCreator.Services;

public sealed class GmailDraftService : IGmailDraftService
{
    public async Task<GmailDraftCreationResult> CreateAsync(
        CompanyRecord company,
        MailTemplateRecord mailTemplate,
        GoogleCredential credential,
        string? fromEmail,
        CancellationToken cancellationToken = default)
    {
        var gmailService = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Email Creator"
        });

        var rawMessage = BuildRawMessage(
            company.CompanyEmail,
            fromEmail,
            mailTemplate.Subject,
            mailTemplate.Body);

        var draft = new Draft
        {
            Message = new Message
            {
                Raw = rawMessage
            }
        };

        var createdDraft = await gmailService.Users.Drafts
            .Create(draft, "me")
            .ExecuteAsync(cancellationToken);

        return new GmailDraftCreationResult(
            createdDraft.Id ?? string.Empty,
            createdDraft.Message?.Id ?? string.Empty);
    }

    private static string BuildRawMessage(
        string toEmail,
        string? fromEmail,
        string subject,
        string body)
    {
        var mime = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(fromEmail))
        {
            mime.Append("From: ").AppendLine(fromEmail.Trim());
        }

        mime.Append("To: ").AppendLine(toEmail.Trim());
        mime.Append("Subject: ").AppendLine(EncodeHeader(subject.Trim()));
        mime.AppendLine("MIME-Version: 1.0");
        mime.AppendLine("Content-Type: text/plain; charset=\"UTF-8\"");
        mime.AppendLine("Content-Transfer-Encoding: base64");
        mime.AppendLine();
        mime.AppendLine(WrapBase64(Convert.ToBase64String(Encoding.UTF8.GetBytes(body.Trim()))));

        return ToBase64Url(Encoding.UTF8.GetBytes(mime.ToString()));
    }

    private static string EncodeHeader(string value)
    {
        return value.All(character => character <= 127)
            ? value
            : $"=?utf-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}?=";
    }

    private static string WrapBase64(string value)
    {
        const int lineLength = 76;
        var wrapped = new StringBuilder();

        for (var index = 0; index < value.Length; index += lineLength)
        {
            var length = Math.Min(lineLength, value.Length - index);
            wrapped.AppendLine(value.Substring(index, length));
        }

        return wrapped.ToString().TrimEnd();
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
