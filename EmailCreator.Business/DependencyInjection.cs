using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EmailCreator.Business.Abstract;
using EmailCreator.Business.Concrete;
using EmailCreator.DataAccess.Contexts;
using EmailCreator.DataAccess.Repositories;

namespace EmailCreator.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddEmailCreatorBusiness(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<EmailCreatorDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IGenericRepository<>), typeof(EfCoreGenericRepository<>));
        services.AddScoped<ICompanyRecordService, EfCoreCompanyRecordService>();

        return services;
    }

    public static async Task MigrateEmailCreatorDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailCreatorDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
