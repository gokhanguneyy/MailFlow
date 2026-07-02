using FluentValidation;
using EmailCreator.Business;
using EmailCreator.Models;
using EmailCreator.Validators;

var builder = WebApplication.CreateBuilder(args);

// MVC, non-nullable string alanlari otomatik Required gibi yorumlayabilir.
// Dogrulama kurallarini tek noktada tutmak icin bu davranisi kapatip FluentValidation'i ana kaynak yapiyoruz.
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddScoped<IValidator<CompanyProfileViewModel>, CompanyProfileViewModelValidator>();
builder.Services.AddEmailCreatorBusiness(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection connection string is missing."));

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
