using Microsoft.EntityFrameworkCore;
using EmailCreator.Entities;

namespace EmailCreator.DataAccess.Contexts;

public class EmailCreatorDbContext : DbContext
{
    public EmailCreatorDbContext(DbContextOptions<EmailCreatorDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");

            entity.HasKey(company => company.Domain);

            entity.Property(company => company.Domain)
                .HasMaxLength(255)
                .ValueGeneratedNever();

            entity.Property(company => company.CompanyName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(company => company.LinkedInUrl)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(company => company.CompanyEmail)
                .HasMaxLength(320)
                .IsRequired();

            entity.Property(company => company.GenderStatus)
                .HasConversion<int>()
                .HasDefaultValue(GenderStatus.Company)
                .IsRequired();

            entity.Property(company => company.CreatedAt)
                .IsRequired();

            entity.HasIndex(company => company.CompanyEmail);
        });
    }
}
