using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddTimeOvertimeRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeOvertimeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DateFrom = table.Column<DateTime>(type: "date", nullable: false),
                    DateTo = table.Column<DateTime>(type: "date", nullable: false),
                    MinutesPerDay = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OrderedByManager = table.Column<bool>(type: "bit", nullable: false),
                    RequestedById = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DecidedById = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeOvertimeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeOvertimeRequests_AspNetUsers_DecidedById",
                        column: x => x.DecidedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeOvertimeRequests_AspNetUsers_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeOvertimeRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeOvertimeRequests_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeOvertimeRequests_DecidedById",
                table: "TimeOvertimeRequests",
                column: "DecidedById");

            migrationBuilder.CreateIndex(
                name: "IX_TimeOvertimeRequests_OrganizationId_Status",
                table: "TimeOvertimeRequests",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeOvertimeRequests_OrganizationId_UserId_DateFrom",
                table: "TimeOvertimeRequests",
                columns: new[] { "OrganizationId", "UserId", "DateFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeOvertimeRequests_RequestedById",
                table: "TimeOvertimeRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_TimeOvertimeRequests_UserId",
                table: "TimeOvertimeRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeOvertimeRequests");
        }
    }
}
