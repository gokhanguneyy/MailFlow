using System.Diagnostics;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using EmailCreator.Business.Abstract;
using EmailCreator.Business.Exceptions;
using EmailCreator.Business.Models;
using EmailCreator.Models;

namespace EmailCreator.Controllers;

public class HomeController : Controller
{
    private readonly ICompanyRecordService _companyRecordService;

    public HomeController(ICompanyRecordService companyRecordService)
    {
        _companyRecordService = companyRecordService;
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
        if (!ModelState.IsValid)
        {
            return View(await BuildViewModelAsync(model));
        }

        var domain = ExtractDomain(model.CompanyEmail);

        try
        {
            var savedRecord = await _companyRecordService.AddAsync(
                model.CompanyName,
                model.LinkedInUrl,
                model.CompanyEmail,
                domain);

            return View(await BuildViewModelAsync(model, savedRecord));
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

    private async Task<CompanyProfileViewModel> BuildViewModelAsync(
        CompanyProfileViewModel model,
        CompanyRecord? savedRecord = null)
    {
        var latestRecord = savedRecord ?? await _companyRecordService.GetLatestAsync();

        if (latestRecord is not null)
        {
            model.SavedCompanyName = latestRecord.CompanyName;
            model.SavedDomain = latestRecord.Domain;
        }

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
