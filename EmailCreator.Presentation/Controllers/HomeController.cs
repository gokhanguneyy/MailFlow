using System.Diagnostics;
using System.Net.Mail;
using System.Security.Claims;
using FluentValidation;
using Google;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EmailCreator.Business.Abstract;
using EmailCreator.Business.Exceptions;
using EmailCreator.Business.Models;
using EmailCreator.Models;
using EmailCreator.Options;
using EmailCreator.Services;

namespace EmailCreator.Controllers;

public class HomeController : Controller
{
    private readonly ICompanyRecordService _companyRecordService;
    private readonly ICompanyDraftService _companyDraftService;
    private readonly IGmailDraftService _gmailDraftService;
    private readonly GoogleAuthOptions _googleAuthOptions;
    private readonly IValidator<CompanyProfileViewModel> _companyProfileValidator;
    private readonly IMailTemplateService _mailTemplateService;
    private readonly IValidator<MailTemplateViewModel> _mailTemplateValidator;
    private readonly IValidator<MailTemplateUpdateViewModel> _mailTemplateUpdateValidator;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HomeController(
        ICompanyRecordService companyRecordService,
        ICompanyDraftService companyDraftService,
        IGmailDraftService gmailDraftService,
        IOptions<GoogleAuthOptions> googleAuthOptions,
        IValidator<CompanyProfileViewModel> companyProfileValidator,
        IMailTemplateService mailTemplateService,
        IValidator<MailTemplateViewModel> mailTemplateValidator,
        IValidator<MailTemplateUpdateViewModel> mailTemplateUpdateValidator,
        IWebHostEnvironment webHostEnvironment)
    {
        _companyRecordService = companyRecordService;
        _companyDraftService = companyDraftService;
        _gmailDraftService = gmailDraftService;
        _googleAuthOptions = googleAuthOptions.Value;
        _companyProfileValidator = companyProfileValidator;
        _mailTemplateService = mailTemplateService;
        _mailTemplateValidator = mailTemplateValidator;
        _mailTemplateUpdateValidator = mailTemplateUpdateValidator;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index(string? searchEmail)
    {
        return View(await BuildViewModelAsync(new CompanyProfileViewModel
        {
            SearchEmail = searchEmail
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CompanyProfileViewModel model)
    {
        var validationResult = await _companyProfileValidator.ValidateAsync(model);

        if (!validationResult.IsValid)
        {
            ModelState.Clear();

            // FluentValidation sonuclarini ModelState'e tasiyoruz.
            // Razor'daki asp-validation-for alanlari ModelState'i okudugu icin mevcut hata gosterim yapisi korunur.
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(await BuildViewModelAsync(model));
        }

        var domain = ExtractDomain(model.CompanyEmail);

        try
        {
            await _companyRecordService.AddAsync(
                model.CompanyName,
                model.LinkedInUrl,
                model.CompanyEmail,
                domain);

            return View(await BuildViewModelAsync(model));
        }
        catch (DuplicateCompanyDomainException exception)
        {
            ModelState.AddModelError(
                nameof(model.CompanyEmail),
                $"{exception.Domain} domaini zaten kayitli. Ayni domainle ikinci firma eklenemez.");

            return View(await BuildViewModelAsync(model));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string domain, string? searchEmail)
    {
        // Silme istegi liste satirindan gelir ve domain benzersiz anahtar oldugu icin tek kaydi hedefler.
        // Islem bittikten sonra kullaniciyi ayni arama filtresiyle listeye geri donduruyoruz.
        await _companyRecordService.DeleteAsync(domain);

        return RedirectToAction(nameof(Index), new { searchEmail });
    }

    public async Task<IActionResult> TaslakOlustur(int? mailTemplateId)
    {
        return View(await BuildCompanyDraftViewModelAsync(new CompanyDraftWorkspaceViewModel
        {
            SelectedMailTemplateId = mailTemplateId,
            SuccessMessage = TempData["CompanyDraftSuccessMessage"] as string,
            ErrorMessage = TempData["CompanyDraftErrorMessage"] as string
        }));
    }

    [GoogleScopedAuthorize(GmailService.ScopeConstants.GmailCompose)]
    public IActionResult GmailBaglan(int? mailTemplateId)
    {
        TempData["CompanyDraftSuccessMessage"] = "Gmail bağlantısı hazır.";

        return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TaslakOlustur(string domain, int? mailTemplateId)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            TempData["CompanyDraftErrorMessage"] = "Taslak oluşturulacak firma bulunamadı.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        if (!mailTemplateId.HasValue)
        {
            TempData["CompanyDraftErrorMessage"] = "Gmail taslağı için önce bir mail şablonu seçin.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        if (!_googleAuthOptions.IsConfigured)
        {
            TempData["CompanyDraftErrorMessage"] = "Gmail taslağı oluşturmak için Google OAuth bilgileri ayarlanmalı.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            TempData["CompanyDraftErrorMessage"] = "Gmail taslağı oluşturmak için önce Gmail hesabına bağlanın.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        var company = await _companyDraftService.GetAvailableCompanyAsync(domain);
        if (company is null)
        {
            TempData["CompanyDraftErrorMessage"] = "Taslak oluşturulacak firma bulunamadı.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        var mailTemplate = await _mailTemplateService.GetByIdAsync(mailTemplateId.Value);
        if (mailTemplate is null)
        {
            TempData["CompanyDraftErrorMessage"] = "Gmail taslağı için seçilen mail şablonu bulunamadı.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        var authProvider = HttpContext.RequestServices.GetService(typeof(IGoogleAuthProvider)) as IGoogleAuthProvider;
        if (authProvider is null)
        {
            TempData["CompanyDraftErrorMessage"] = "Gmail bağlantısı hazır değil. Google OAuth ayarlarını kontrol edin.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        GoogleCredential credential;
        try
        {
            credential = await authProvider.GetCredentialAsync();
        }
        catch
        {
            TempData["CompanyDraftErrorMessage"] = "Gmail yetkisi alınamadı. Gmail hesabına tekrar bağlanın.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        GmailDraftCreationResult gmailDraft;
        try
        {
            gmailDraft = await _gmailDraftService.CreateAsync(
                company,
                mailTemplate,
                credential,
                User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"),
                HttpContext.RequestAborted);
        }
        catch (GoogleApiException)
        {
            TempData["CompanyDraftErrorMessage"] = "Gmail taslağı oluşturulamadı. Gmail API iznini ve hesabı kontrol edin.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }
        catch (IOException)
        {
            TempData["CompanyDraftErrorMessage"] = "Mail şablonuna bağlı PDF okunamadı. PDF dosyasını kontrol edin.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }
        catch (InvalidOperationException)
        {
            TempData["CompanyDraftErrorMessage"] = "Mail şablonuna bağlı PDF hazırlanamadı. Şablon PDF kaydını kontrol edin.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        if (string.IsNullOrWhiteSpace(gmailDraft.DraftId))
        {
            TempData["CompanyDraftErrorMessage"] = "Gmail taslağı oluşturuldu ama Gmail taslak kimliği alınamadı.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        var draft = await _companyDraftService.CreateAsync(
            domain,
            mailTemplate.Id,
            mailTemplate.Subject,
            mailTemplate.Body,
            gmailDraft.DraftId,
            gmailDraft.MessageId);

        if (draft is null)
        {
            TempData["CompanyDraftErrorMessage"] = "Taslak oluşturulacak firma bulunamadı.";

            return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
        }

        TempData["CompanyDraftSuccessMessage"] = $"{draft.CompanyName} için Gmail taslağı oluşturuldu.";

        return RedirectToAction(nameof(TaslakOlustur), new { mailTemplateId });
    }

    public async Task<IActionResult> MailSablonu()
    {
        return View(await BuildMailTemplateViewModelAsync(new MailTemplateViewModel
        {
            SuccessMessage = TempData["MailTemplateSuccessMessage"] as string,
            ErrorMessage = TempData["MailTemplateErrorMessage"] as string
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MailSablonu(MailTemplateViewModel model)
    {
        var validationResult = await _mailTemplateValidator.ValidateAsync(model);

        if (!validationResult.IsValid)
        {
            ModelState.Clear();

            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(await BuildMailTemplateViewModelAsync(model));
        }

        SavedPdfFile? savedPdfFile = null;

        try
        {
            savedPdfFile = await SavePdfFileAsync(model.PdfFile!);

            await _mailTemplateService.AddAsync(
                model.Title,
                model.Subject,
                model.Body,
                savedPdfFile.OriginalFileName,
                savedPdfFile.StoredFileName,
                savedPdfFile.RelativePath,
                savedPdfFile.FileSize);

            TempData["MailTemplateSuccessMessage"] = "Kayit basariyla olusturuldu.";

            return RedirectToAction(nameof(MailSablonu));
        }
        catch
        {
            if (savedPdfFile is not null && System.IO.File.Exists(savedPdfFile.FullPath))
            {
                System.IO.File.Delete(savedPdfFile.FullPath);
            }

            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuncelleMailTemplate(MailTemplateUpdateViewModel model)
    {
        var validationResult = await _mailTemplateUpdateValidator.ValidateAsync(model);

        if (!validationResult.IsValid)
        {
            TempData["MailTemplateErrorMessage"] = string.Join(" ", validationResult.Errors.Select(error => error.ErrorMessage));

            return RedirectToAction(nameof(MailSablonu));
        }

        var existingTemplate = await _mailTemplateService.GetByIdAsync(model.Id);
        if (existingTemplate is null)
        {
            TempData["MailTemplateErrorMessage"] = "Guncellenecek mail sablonu bulunamadi.";

            return RedirectToAction(nameof(MailSablonu));
        }

        SavedPdfFile? savedPdfFile = null;

        try
        {
            if (model.PdfFile is not null)
            {
                savedPdfFile = await SavePdfFileAsync(model.PdfFile);
            }

            var updatedTemplate = await _mailTemplateService.UpdateAsync(
                model.Id,
                model.Title,
                model.Subject,
                model.Body,
                savedPdfFile?.OriginalFileName,
                savedPdfFile?.StoredFileName,
                savedPdfFile?.RelativePath,
                savedPdfFile?.FileSize);

            if (updatedTemplate is null)
            {
                if (savedPdfFile is not null && System.IO.File.Exists(savedPdfFile.FullPath))
                {
                    System.IO.File.Delete(savedPdfFile.FullPath);
                }

                TempData["MailTemplateErrorMessage"] = "Guncellenecek mail sablonu bulunamadi.";

                return RedirectToAction(nameof(MailSablonu));
            }

            if (savedPdfFile is not null)
            {
                DeleteStoredFileIfExists(existingTemplate.PdfStoragePath);
            }

            TempData["MailTemplateSuccessMessage"] = "Mail sablonu guncellendi.";

            return RedirectToAction(nameof(MailSablonu));
        }
        catch
        {
            if (savedPdfFile is not null && System.IO.File.Exists(savedPdfFile.FullPath))
            {
                System.IO.File.Delete(savedPdfFile.FullPath);
            }

            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMailTemplate(int id)
    {
        var deletedTemplate = await _mailTemplateService.DeleteAsync(id);

        if (deletedTemplate is null)
        {
            TempData["MailTemplateErrorMessage"] = "Silinecek mail sablonu bulunamadi.";

            return RedirectToAction(nameof(MailSablonu));
        }

        DeleteStoredFileIfExists(deletedTemplate.PdfStoragePath);

        TempData["MailTemplateSuccessMessage"] = "Mail sablonu silindi.";

        return RedirectToAction(nameof(MailSablonu));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static string ExtractDomain(string email)
    {
        var address = new MailAddress(email);

        return address.Host.ToLowerInvariant();
    }

    private async Task<CompanyProfileViewModel> BuildViewModelAsync(CompanyProfileViewModel model)
    {
        string? domainSearch = null;
        if (!string.IsNullOrWhiteSpace(model.SearchEmail))
        {
            domainSearch = ExtractSearchDomain(model.SearchEmail);
        }

        var records = await _companyRecordService.GetAllAsync(domainSearch);

        model.SavedCompanies = records
            .Select(ToListItem)
            .ToList();

        return model;
    }

    private async Task<CompanyDraftWorkspaceViewModel> BuildCompanyDraftViewModelAsync(CompanyDraftWorkspaceViewModel model)
    {
        var availableCompanies = await _companyDraftService.GetAvailableCompaniesAsync();
        var createdDrafts = await _companyDraftService.GetAllAsync();
        var mailTemplates = await _mailTemplateService.GetAllAsync();

        model.AvailableCompanies = availableCompanies
            .Select(ToListItem)
            .ToList();

        model.MailTemplates = mailTemplates
            .Select(template => new CompanyDraftMailTemplateOptionViewModel
            {
                Id = template.Id,
                Title = template.Title
            })
            .ToList();

        if (model.HasMailTemplates
            && (!model.SelectedMailTemplateId.HasValue
                || model.MailTemplates.All(template => template.Id != model.SelectedMailTemplateId.Value)))
        {
            model.SelectedMailTemplateId = model.MailTemplates[0].Id;
        }

        model.CreatedDrafts = createdDrafts
            .Select(ToCompanyDraftListItem)
            .ToList();

        model.IsGmailConfigured = _googleAuthOptions.IsConfigured;
        model.IsGmailConnected = User.Identity?.IsAuthenticated == true;

        return model;
    }

    private async Task<MailTemplateViewModel> BuildMailTemplateViewModelAsync(MailTemplateViewModel model)
    {
        var mailTemplates = await _mailTemplateService.GetAllAsync();

        model.SavedTemplates = mailTemplates
            .Select(ToMailTemplateListItem)
            .ToList();

        return model;
    }

    private static CompanyListItemViewModel ToListItem(CompanyRecord record)
    {
        return new CompanyListItemViewModel
        {
            CompanyName = record.CompanyName,
            LinkedInUrl = record.LinkedInUrl,
            CompanyEmail = record.CompanyEmail,
            Domain = record.Domain,
            CreatedAt = record.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
        };
    }

    private static CompanyDraftListItemViewModel ToCompanyDraftListItem(CompanyDraftRecord record)
    {
        return new CompanyDraftListItemViewModel
        {
            Id = record.Id,
            CompanyName = record.CompanyName,
            LinkedInUrl = record.LinkedInUrl,
            CompanyEmail = record.CompanyEmail,
            Domain = record.Domain,
            CompanyCreatedAt = record.CompanyCreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
            MailSubject = record.MailSubject,
            GmailDraftId = record.GmailDraftId,
            DraftCreatedAt = record.DraftCreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
        };
    }

    private static MailTemplateListItemViewModel ToMailTemplateListItem(MailTemplateRecord record)
    {
        return new MailTemplateListItemViewModel
        {
            Id = record.Id,
            Title = record.Title,
            Subject = record.Subject,
            Body = record.Body,
            PdfOriginalFileName = record.PdfOriginalFileName,
            PdfStoragePath = record.PdfStoragePath,
            PdfFileSize = FormatFileSize(record.PdfFileSize),
            CreatedAt = record.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
            UpdatedAt = record.UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
        };
    }

    private async Task<SavedPdfFile> SavePdfFileAsync(IFormFile pdfFile)
    {
        var uploadsDirectory = Path.Combine(GetWebRootPath(), "uploads", "mail-sablonu");
        Directory.CreateDirectory(uploadsDirectory);

        var originalFileName = Path.GetFileName(pdfFile.FileName);
        var storedFileName = $"{Guid.NewGuid():N}.pdf";
        var fullPath = Path.Combine(uploadsDirectory, storedFileName);

        await using var fileStream = System.IO.File.Create(fullPath);
        await pdfFile.CopyToAsync(fileStream);

        return new SavedPdfFile(
            originalFileName,
            storedFileName,
            $"/uploads/mail-sablonu/{storedFileName}",
            fullPath,
            pdfFile.Length);
    }

    private string GetWebRootPath()
    {
        return _webHostEnvironment.WebRootPath
            ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
    }

    private void DeleteStoredFileIfExists(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var webRootPath = Path.GetFullPath(GetWebRootPath());
        var normalizedRelativePath = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, normalizedRelativePath));

        if (!fullPath.StartsWith(webRootPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }

    private static string FormatFileSize(long fileSize)
    {
        if (fileSize >= 1024 * 1024)
        {
            return $"{fileSize / 1024d / 1024d:0.##} MB";
        }

        return $"{fileSize / 1024d:0.##} KB";
    }

    private static string ExtractSearchDomain(string searchText)
    {
        var normalizedSearchText = searchText.Trim();
        var atSignIndex = normalizedSearchText.LastIndexOf('@');

        if (atSignIndex >= 0 && atSignIndex < normalizedSearchText.Length - 1)
        {
            return normalizedSearchText[(atSignIndex + 1)..].Trim();
        }

        return normalizedSearchText;
    }

    private sealed record SavedPdfFile(
        string OriginalFileName,
        string StoredFileName,
        string RelativePath,
        string FullPath,
        long FileSize);
}
