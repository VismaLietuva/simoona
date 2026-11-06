using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddKudosCalculatorSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KudosCalculatorSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VatPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TeamMultiplier = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    CreditPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TechWearMonths = table.Column<int>(type: "int", nullable: false),
                    FurnitureWearMonths = table.Column<int>(type: "int", nullable: false),
                    BuyoutMinimum = table.Column<int>(type: "int", nullable: false),
                    GuideUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WhitelistUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KudosCalculatorSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KudosCalculatorSettings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KudosCalculatorSettings_OrganizationId",
                table: "KudosCalculatorSettings",
                column: "OrganizationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KudosCalculatorSettings");
        }
    }
}
