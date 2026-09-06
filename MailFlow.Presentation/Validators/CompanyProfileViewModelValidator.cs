using FluentValidation;
using MailFlow.Models;

namespace MailFlow.Validators;

public sealed class CompanyProfileViewModelValidator : AbstractValidator<CompanyProfileViewModel>
{
    public CompanyProfileViewModelValidator()
    {
        // Form validasyonunu view model attribute'larından ayırıyoruz.
        // Böylece doğrulama kuralları tek bir sınıfta okunur, test edilir ve gerektiğinde genişletilir.
        RuleFor(model => model.CompanyName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Firma adı zorunludur.");

        RuleFor(model => model.LinkedInUrl)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("LinkedIn hesabı zorunludur.")
            .Must(BeValidHttpUrl)
            .WithMessage("Geçerli bir LinkedIn adresi girin.");

        RuleFor(model => model.CompanyEmail)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Firma maili zorunludur.")
            .EmailAddress()
            .WithMessage("Geçerli bir mail adresi girin.");
    }

    private static bool BeValidHttpUrl(string? url)
    {
        // Uri.TryCreate exception fırlatmadan URL formatını kontrol eder.
        // Sadece http/https kabul ederek kullanıcıdan gerçek web adresi beklediğimizi netleştiriyoruz.
        return Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl)
            && (parsedUrl.Scheme == Uri.UriSchemeHttp || parsedUrl.Scheme == Uri.UriSchemeHttps);
    }
}
