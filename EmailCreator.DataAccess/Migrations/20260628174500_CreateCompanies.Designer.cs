using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EmailCreator.DataAccess.Contexts;

#nullable disable

namespace EmailCreator.DataAccess.Migrations;

[DbContext(typeof(EmailCreatorDbContext))]
[Migration("20260628174500_CreateCompanies")]
partial class CreateCompanies
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

            entity.Property<string>("LinkedInUrl")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            entity.HasKey("Domain");

            entity.HasIndex("CompanyEmail");

            entity.ToTable("Companies");
        });
#pragma warning restore 612, 618
    }
}
