using System.Diagnostics;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using EmailCreator.Models;

namespace EmailCreator.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(new CompanyProfileViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(CompanyProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.SavedCompanyName = model.CompanyName.Trim();
        model.SavedDomain = ExtractDomain(model.CompanyEmail);

        return View(model);
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
}
