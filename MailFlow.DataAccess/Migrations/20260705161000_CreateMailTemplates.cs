using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailFlow.DataAccess.Migrations;

public partial class CreateMailTemplates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[dbo].[MerhabalarDocuments]', N'U') IS NOT NULL
                AND OBJECT_ID(N'[dbo].[MailTemplates]', N'U') IS NULL
            BEGIN
                EXEC sp_rename N'[dbo].[MerhabalarDocuments]', N'MailTemplates';
            END

            IF OBJECT_ID(N'[dbo].[MailTemplates]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[MailTemplates] (
                    [Id] int NOT NULL IDENTITY,
                    [Title] nvarchar(200) NOT NULL,
                    [Subject] nvarchar(300) NOT NULL,
                    [Body] nvarchar(max) NOT NULL,
                    [PdfOriginalFileName] nvarchar(260) NOT NULL,
                    [PdfStoredFileName] nvarchar(260) NOT NULL,
                    [PdfStoragePath] nvarchar(500) NOT NULL,
                    [PdfFileSize] bigint NOT NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [UpdatedAt] datetimeoffset NOT NULL,
                    CONSTRAINT [PK_MailTemplates] PRIMARY KEY ([Id])
                );
            END

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE object_id = OBJECT_ID(N'[dbo].[MailTemplates]')
                    AND name IN (N'IX_MailTemplates_CreatedAt', N'IX_MerhabalarDocuments_CreatedAt')
            )
            BEGIN
                CREATE INDEX [IX_MailTemplates_CreatedAt] ON [dbo].[MailTemplates] ([CreatedAt]);
            END

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE object_id = OBJECT_ID(N'[dbo].[MailTemplates]')
                    AND name IN (N'IX_MailTemplates_Title', N'IX_MerhabalarDocuments_Title')
            )
            BEGIN
                CREATE INDEX [IX_MailTemplates_Title] ON [dbo].[MailTemplates] ([Title]);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[dbo].[MailTemplates]', N'U') IS NOT NULL
            BEGIN
                DROP TABLE [dbo].[MailTemplates];
            END
            """);
    }
}
