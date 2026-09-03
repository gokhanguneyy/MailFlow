using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using EmailCreator.DataAccess.Contexts;

#nullable disable

namespace EmailCreator.DataAccess.Migrations;

[DbContext(typeof(EmailCreatorDbContext))]
[Migration("20260904120000_CreateCompanyDrafts")]
public partial class CreateCompanyDrafts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CompanyDrafts",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Domain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                LinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CompanyEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                CompanyCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                DraftCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompanyDrafts", draft => draft.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CompanyDrafts_Domain",
            table: "CompanyDrafts",
            column: "Domain",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CompanyDrafts_DraftCreatedAt",
            table: "CompanyDrafts",
            column: "DraftCreatedAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CompanyDrafts");
    }
}
