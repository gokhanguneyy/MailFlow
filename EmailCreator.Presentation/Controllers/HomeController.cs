using System.Diagnostics;
using System.Net.Mail;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using EmailCreator.Business.Abstract;
using EmailCreator.Business.Exceptions;
using EmailCreator.Business.Models;
using EmailCreator.Models;

namespace EmailCreator.Controllers;

public class HomeController : Controller
{
    private readonly ICompanyRecordService _companyRecordService;
    private readonly IValidator<CompanyProfileViewModel> _companyProfileValidator;

    public HomeController(
        ICompanyRecordService companyRecordService,
        IValidator<CompanyProfileViewModel> companyProfileValidator)
    {
        _companyRecordService = companyRecordService;
        _companyProfileValidator = companyProfileValidator;
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

            // FluentValidation sonuçlarını ModelState'e taşıyoruz.
            // Razor'daki asp-validation-for alanları ModelState'i okuduğu için mevcut hata gösterim yapısı korunur.
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
                $"{exception.Domain} domaini zaten kayıtlı. Aynı domainle ikinci firma eklenemez.");

            return View(await BuildViewModelAsync(model));
        }
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
}
