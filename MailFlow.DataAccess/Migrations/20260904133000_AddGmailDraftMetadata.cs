using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MailFlow.DataAccess.Contexts;

#nullable disable

namespace MailFlow.DataAccess.Migrations;

[DbContext(typeof(MailFlowDbContext))]
[Migration("20260904133000_AddGmailDraftMetadata")]
public partial class AddGmailDraftMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "MailTemplateId",
            table: "CompanyDrafts",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "MailSubject",
            table: "CompanyDrafts",
            type: "nvarchar(300)",
            maxLength: 300,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "MailBody",
            table: "CompanyDrafts",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "GmailDraftId",
            table: "CompanyDrafts",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "GmailMessageId",
            table: "CompanyDrafts",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_CompanyDrafts_GmailDraftId",
            table: "CompanyDrafts",
            column: "GmailDraftId");

        migrationBuilder.CreateIndex(
            name: "IX_CompanyDrafts_MailTemplateId",
            table: "CompanyDrafts",
            column: "MailTemplateId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CompanyDrafts_GmailDraftId",
            table: "CompanyDrafts");

        migrationBuilder.DropIndex(
            name: "IX_CompanyDrafts_MailTemplateId",
            table: "CompanyDrafts");

        migrationBuilder.DropColumn(
            name: "MailTemplateId",
            table: "CompanyDrafts");

        migrationBuilder.DropColumn(
            name: "MailSubject",
            table: "CompanyDrafts");

        migrationBuilder.DropColumn(
            name: "MailBody",
            table: "CompanyDrafts");

        migrationBuilder.DropColumn(
            name: "GmailDraftId",
            table: "CompanyDrafts");

        migrationBuilder.DropColumn(
            name: "GmailMessageId",
            table: "CompanyDrafts");
    }
}
