using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailFlow.DataAccess.Migrations;

public partial class CreateCompanies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Companies",
            columns: table => new
            {
                Domain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                LinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CompanyEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Companies", company => company.Domain);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Companies_CompanyEmail",
            table: "Companies",
            column: "CompanyEmail");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Companies");
    }
}
