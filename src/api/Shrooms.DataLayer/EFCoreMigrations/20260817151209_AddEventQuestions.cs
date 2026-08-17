using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddEventQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "EventOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuestionId",
                table: "EventOptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    SelectType = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ShowIfOptionId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventQuestions_EventOptions_ShowIfOptionId",
                        column: x => x.ShowIfOptionId,
                        principalTable: "EventOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventQuestions_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventOptions_QuestionId",
                table: "EventOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_EventQuestions_EventId",
                table: "EventQuestions",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventQuestions_ShowIfOptionId",
                table: "EventQuestions",
                column: "ShowIfOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventOptions_EventQuestions_QuestionId",
                table: "EventOptions",
                column: "QuestionId",
                principalTable: "EventQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventOptions_EventQuestions_QuestionId",
                table: "EventOptions");

            migrationBuilder.DropTable(
                name: "EventQuestions");

            migrationBuilder.DropIndex(
                name: "IX_EventOptions_QuestionId",
                table: "EventOptions");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "EventOptions");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "EventOptions");
        }
    }
}
