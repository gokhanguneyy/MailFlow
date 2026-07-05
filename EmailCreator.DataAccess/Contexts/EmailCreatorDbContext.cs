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

    public DbSet<MailTemplate> MailTemplates => Set<MailTemplate>();

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

        modelBuilder.Entity<MailTemplate>(entity =>
        {
            entity.ToTable("MailTemplates");

            entity.HasKey(document => document.Id);

            entity.Property(document => document.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(document => document.Subject)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(document => document.Body)
                .IsRequired();

            entity.Property(document => document.PdfOriginalFileName)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(document => document.PdfStoredFileName)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(document => document.PdfStoragePath)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(document => document.PdfFileSize)
                .IsRequired();

            entity.Property(document => document.CreatedAt)
                .IsRequired();

            entity.Property(document => document.UpdatedAt)
                .IsRequired();

            entity.HasIndex(document => document.Title);
            entity.HasIndex(document => document.CreatedAt);
        });
    }
}
