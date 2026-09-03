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

    public DbSet<CompanyDraft> CompanyDrafts => Set<CompanyDraft>();

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

        modelBuilder.Entity<CompanyDraft>(entity =>
        {
            entity.ToTable("CompanyDrafts");

            entity.HasKey(draft => draft.Id);

            entity.Property(draft => draft.Domain)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(draft => draft.CompanyName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(draft => draft.LinkedInUrl)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(draft => draft.CompanyEmail)
                .HasMaxLength(320)
                .IsRequired();

            entity.Property(draft => draft.CompanyCreatedAt)
                .IsRequired();

            entity.Property(draft => draft.MailTemplateId)
                .IsRequired();

            entity.Property(draft => draft.MailSubject)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(draft => draft.MailBody)
                .IsRequired();

            entity.Property(draft => draft.GmailDraftId)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(draft => draft.GmailMessageId)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(draft => draft.DraftCreatedAt)
                .IsRequired();

            entity.HasIndex(draft => draft.Domain)
                .IsUnique();

            entity.HasIndex(draft => draft.DraftCreatedAt);
            entity.HasIndex(draft => draft.GmailDraftId);
            entity.HasIndex(draft => draft.MailTemplateId);
        });
    }
}
