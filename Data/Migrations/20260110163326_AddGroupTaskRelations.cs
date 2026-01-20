using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SosyalAjandam.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupTaskRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                table: "TodoItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LifeItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LifeItems_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_AssignedToUserId",
                table: "TodoItems",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LifeItems_OwnerId",
                table: "LifeItems",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_AspNetUsers_AssignedToUserId",
                table: "TodoItems",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_AspNetUsers_AssignedToUserId",
                table: "TodoItems");

            migrationBuilder.DropTable(
                name: "LifeItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_AssignedToUserId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "TodoItems");
        }
    }
}
