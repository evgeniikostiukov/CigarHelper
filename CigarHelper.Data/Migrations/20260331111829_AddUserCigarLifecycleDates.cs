using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCigarLifecycleDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastTouchedAt",
                table: "UserCigars",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchasedAt",
                table: "UserCigars",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SmokedAt",
                table: "UserCigars",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "UserCigars"
                SET
                    "PurchasedAt" = COALESCE("CreatedAt", NOW()),
                    "LastTouchedAt" = COALESCE("UpdatedAt", "CreatedAt", NOW());
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PurchasedAt",
                table: "UserCigars",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastTouchedAt",
                table: "UserCigars",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastTouchedAt",
                table: "UserCigars");

            migrationBuilder.DropColumn(
                name: "PurchasedAt",
                table: "UserCigars");

            migrationBuilder.DropColumn(
                name: "SmokedAt",
                table: "UserCigars");
        }
    }
}
