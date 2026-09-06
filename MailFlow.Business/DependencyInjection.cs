using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MailFlow.Business.Abstract;
using MailFlow.Business.Concrete;
using MailFlow.Business.Factories;
using MailFlow.DataAccess.Contexts;
using MailFlow.DataAccess.Repositories;

namespace MailFlow.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddMailFlowBusiness(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<MailFlowDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IGenericRepository<>), typeof(EfCoreGenericRepository<>));
        services.AddScoped<ICompanyFactory, CompanyFactory>();
        services.AddScoped<ICompanyRecordService, EfCoreCompanyRecordService>();
        services.AddScoped<ICompanyDraftService, EfCoreCompanyDraftService>();
        services.AddScoped<IMailTemplateService, EfCoreMailTemplateService>();

        return services;
    }

    public static async Task MigrateMailFlowDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MailFlowDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
