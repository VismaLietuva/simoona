using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddGlobalSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PictureId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LinkId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AccessKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OwnerUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SearchText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false, collation: "Latin1_General_100_BIN2"),
                    IndexedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchDocuments", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_OrganizationId_Kind_EntityId",
                table: "SearchDocuments",
                columns: new[] { "OrganizationId", "Kind", "EntityId" },
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            // Azure SQL has full-text search; a SQL Server without it (the local Docker
            // image) keeps the table only, and search falls back to LIKE on SearchText.
            // Full-text DDL cannot run inside the migration's transaction.
            migrationBuilder.Sql(
                @"IF FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'SearchCatalog')
        EXEC('CREATE FULLTEXT CATALOG SearchCatalog WITH ACCENT_SENSITIVITY = OFF');
    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.SearchDocuments'))
        EXEC('CREATE FULLTEXT INDEX ON dbo.SearchDocuments (SearchText LANGUAGE 0) KEY INDEX PK_SearchDocuments ON SearchCatalog WITH CHANGE_TRACKING AUTO, STOPLIST = OFF');
END",
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.SearchDocuments'))
    DROP FULLTEXT INDEX ON dbo.SearchDocuments;
IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'SearchCatalog')
    DROP FULLTEXT CATALOG SearchCatalog;",
                suppressTransaction: true);

            migrationBuilder.DropTable(
                name: "SearchDocuments");
        }
    }
}
