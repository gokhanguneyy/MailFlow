using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EmailCreator.DataAccess.Contexts;

#nullable disable

namespace EmailCreator.DataAccess.Migrations;

[DbContext(typeof(EmailCreatorDbContext))]
[Migration("20260705161000_CreateMailTemplates")]
partial class CreateMailTemplates
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("EmailCreator.Entities.Company", entity =>
        {
            entity.Property<string>("Domain")
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)");

            entity.Property<string>("CompanyEmail")
                .IsRequired()
                .HasMaxLength(320)
                .HasColumnType("nvarchar(320)");

            entity.Property<string>("CompanyName")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset");

            entity.Property<int>("GenderStatus")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasDefaultValue(0);

            entity.Property<string>("LinkedInUrl")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            entity.HasKey("Domain");

            entity.HasIndex("CompanyEmail");

            entity.ToTable("Companies");
        });

        modelBuilder.Entity("EmailCreator.Entities.MailTemplate", entity =>
        {
            entity.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            entity.Property<string>("Body")
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset");

            entity.Property<long>("PdfFileSize")
                .HasColumnType("bigint");

            entity.Property<string>("PdfOriginalFileName")
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnType("nvarchar(260)");

            entity.Property<string>("PdfStoragePath")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            entity.Property<string>("PdfStoredFileName")
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnType("nvarchar(260)");

            entity.Property<string>("Subject")
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");

            entity.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset");

            entity.HasKey("Id");

            entity.HasIndex("CreatedAt");

            entity.HasIndex("Title");

            entity.ToTable("MailTemplates");
        });
#pragma warning restore 612, 618
    }
}
