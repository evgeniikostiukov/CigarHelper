using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCigarCommentModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModeratedAt",
                table: "CigarComments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModeratedByUserId",
                table: "CigarComments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModerationStatus",
                table: "CigarComments",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_CigarComments_ModeratedByUserId",
                table: "CigarComments",
                column: "ModeratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CigarComments_ModerationStatus",
                table: "CigarComments",
                column: "ModerationStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_CigarComments_Users_ModeratedByUserId",
                table: "CigarComments",
                column: "ModeratedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CigarComments_Users_ModeratedByUserId",
                table: "CigarComments");

            migrationBuilder.DropIndex(
                name: "IX_CigarComments_ModeratedByUserId",
                table: "CigarComments");

            migrationBuilder.DropIndex(
                name: "IX_CigarComments_ModerationStatus",
                table: "CigarComments");

            migrationBuilder.DropColumn(
                name: "ModeratedAt",
                table: "CigarComments");

            migrationBuilder.DropColumn(
                name: "ModeratedByUserId",
                table: "CigarComments");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "CigarComments");
        }
    }
}
