using FluentValidation;
using Microsoft.AspNetCore.Http;
using MailFlow.Models;

namespace MailFlow.Validators;

public sealed class MailTemplateViewModelValidator : AbstractValidator<MailTemplateViewModel>
{
    private const long MaxPdfFileSizeBytes = 10 * 1024 * 1024;

    public MailTemplateViewModelValidator()
    {
        RuleFor(model => model.Title)
            .NotEmpty().WithMessage("Baslik alani zorunludur.")
            .MaximumLength(200).WithMessage("Baslik en fazla 200 karakter olabilir.");

        RuleFor(model => model.Subject)
            .NotEmpty().WithMessage("Konu alani zorunludur.")
            .MaximumLength(300).WithMessage("Konu en fazla 300 karakter olabilir.");

        RuleFor(model => model.Body)
            .NotEmpty().WithMessage("Metin alani zorunludur.");

        RuleFor(model => model.PdfFile)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("PDF dosyasi secmelisiniz.")
            .Must(file => file is not null && file.Length > 0).WithMessage("PDF dosyasi bos olamaz.")
            .Must(file => file is not null && file.Length <= MaxPdfFileSizeBytes).WithMessage("PDF dosyasi en fazla 10 MB olabilir.")
            .Must(HasPdfExtension).WithMessage("Sadece PDF dosyasi yukleyebilirsiniz.");
    }

    internal static bool HasPdfExtension(IFormFile? file)
    {
        return file is not null
            && Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsValidPdfFileSize(IFormFile? file)
    {
        return file is not null
            && file.Length > 0
            && file.Length <= MaxPdfFileSizeBytes;
    }
}

public sealed class MailTemplateUpdateViewModelValidator : AbstractValidator<MailTemplateUpdateViewModel>
{
    public MailTemplateUpdateViewModelValidator()
    {
        RuleFor(model => model.Id)
            .GreaterThan(0).WithMessage("Guncellenecek kayit bulunamadi.");

        RuleFor(model => model.Title)
            .NotEmpty().WithMessage("Baslik alani zorunludur.")
            .MaximumLength(200).WithMessage("Baslik en fazla 200 karakter olabilir.");

        RuleFor(model => model.Subject)
            .NotEmpty().WithMessage("Konu alani zorunludur.")
            .MaximumLength(300).WithMessage("Konu en fazla 300 karakter olabilir.");

        RuleFor(model => model.Body)
            .NotEmpty().WithMessage("Metin alani zorunludur.");

        When(model => model.PdfFile is not null, () =>
        {
            RuleFor(model => model.PdfFile)
                .Cascade(CascadeMode.Stop)
                .Must(MailTemplateViewModelValidator.IsValidPdfFileSize).WithMessage("PDF/CV dosyasi bos olamaz ve en fazla 10 MB olabilir.")
                .Must(MailTemplateViewModelValidator.HasPdfExtension).WithMessage("Sadece PDF/CV dosyasi yukleyebilirsiniz.");
        });
    }
}
