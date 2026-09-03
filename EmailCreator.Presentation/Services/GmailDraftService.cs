using System.Text;
using EmailCreator.Business.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Hosting;

namespace EmailCreator.Services;

public sealed class GmailDraftService : IGmailDraftService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public GmailDraftService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

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

        var pdfAttachment = await ReadPdfAttachmentAsync(mailTemplate, cancellationToken);
        var rawMessage = BuildRawMessage(
            company.CompanyEmail,
            fromEmail,
            mailTemplate.Subject,
            mailTemplate.Body,
            pdfAttachment);

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
        string body,
        PdfAttachment pdfAttachment)
    {
        var boundary = $"email-creator-{Guid.NewGuid():N}";
        var mime = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(fromEmail))
        {
            mime.Append("From: ").AppendLine(fromEmail.Trim());
        }

        mime.Append("To: ").AppendLine(toEmail.Trim());
        mime.Append("Subject: ").AppendLine(EncodeHeader(subject.Trim()));
        mime.AppendLine("MIME-Version: 1.0");
        mime.Append("Content-Type: multipart/mixed; boundary=\"").Append(boundary).AppendLine("\"");
        mime.AppendLine();
        mime.Append("--").AppendLine(boundary);
        mime.AppendLine("Content-Type: text/plain; charset=\"UTF-8\"");
        mime.AppendLine("Content-Transfer-Encoding: base64");
        mime.AppendLine();
        mime.AppendLine(WrapBase64(Convert.ToBase64String(Encoding.UTF8.GetBytes(body.Trim()))));
        mime.AppendLine();
        mime.Append("--").AppendLine(boundary);
        mime.Append("Content-Type: application/pdf; name=\"").Append(EscapeHeaderValue(pdfAttachment.FileName)).AppendLine("\"");
        mime.Append("Content-Disposition: attachment; filename=\"").Append(EscapeHeaderValue(pdfAttachment.FileName)).AppendLine("\"");
        mime.AppendLine("Content-Transfer-Encoding: base64");
        mime.AppendLine();
        mime.AppendLine(WrapBase64(Convert.ToBase64String(pdfAttachment.Content)));
        mime.Append("--").Append(boundary).AppendLine("--");

        return ToBase64Url(Encoding.UTF8.GetBytes(mime.ToString()));
    }

    private async Task<PdfAttachment> ReadPdfAttachmentAsync(
        MailTemplateRecord mailTemplate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(mailTemplate.PdfStoragePath))
        {
            throw new InvalidOperationException("Mail şablonuna bağlı PDF bulunamadı.");
        }

        var webRootPath = Path.GetFullPath(GetWebRootPath());
        var normalizedRelativePath = mailTemplate.PdfStoragePath
            .TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, normalizedRelativePath));

        if (!fullPath.StartsWith(webRootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Mail şablonuna bağlı PDF yolu geçersiz.");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Mail şablonuna bağlı PDF dosyası bulunamadı.", fullPath);
        }

        return new PdfAttachment(
            string.IsNullOrWhiteSpace(mailTemplate.PdfOriginalFileName)
                ? "ek.pdf"
                : mailTemplate.PdfOriginalFileName,
            await File.ReadAllBytesAsync(fullPath, cancellationToken));
    }

    private string GetWebRootPath()
    {
        return _webHostEnvironment.WebRootPath
            ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
    }

    private static string EncodeHeader(string value)
    {
        return value.All(character => character <= 127)
            ? value
            : $"=?utf-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}?=";
    }

    private static string EscapeHeaderValue(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
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

    private sealed record PdfAttachment(
        string FileName,
        byte[] Content);
}
