using FluentValidation;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using EmailCreator.Business;
using EmailCreator.Models;
using EmailCreator.Options;
using EmailCreator.Services;
using EmailCreator.Validators;

var builder = WebApplication.CreateBuilder(args);

// MVC, non-nullable string alanlari otomatik Required gibi yorumlayabilir.
// Dogrulama kurallarini tek noktada tutmak icin bu davranisi kapatip FluentValidation'i ana kaynak yapiyoruz.
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddScoped<IValidator<CompanyProfileViewModel>, CompanyProfileViewModelValidator>();
builder.Services.AddScoped<IValidator<MailTemplateViewModel>, MailTemplateViewModelValidator>();
builder.Services.AddScoped<IValidator<MailTemplateUpdateViewModel>, MailTemplateUpdateViewModelValidator>();
builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection("Google"));
builder.Services.AddScoped<IGmailDraftService, GmailDraftService>();
builder.Services.AddEmailCreatorBusiness(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection connection string is missing."));

var googleAuthOptions = builder.Configuration
    .GetSection("Google")
    .Get<GoogleAuthOptions>() ?? new GoogleAuthOptions();

if (googleAuthOptions.IsConfigured)
{
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddGoogleOpenIdConnect(options =>
        {
            options.ClientId = googleAuthOptions.ClientId!;
            options.ClientSecret = googleAuthOptions.ClientSecret!;
        });
}
else
{
    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie();
}

builder.Services.AddAuthorization();

var app = builder.Build();

await app.Services.MigrateEmailCreatorDatabaseAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
