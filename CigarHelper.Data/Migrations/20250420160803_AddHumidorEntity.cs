using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHumidorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HumidorId",
                table: "Cigars",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Humidors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    CurrentHumidity = table.Column<int>(type: "integer", nullable: true),
                    CurrentTemperature = table.Column<decimal>(type: "numeric", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Humidors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Humidors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cigars_HumidorId",
                table: "Cigars",
                column: "HumidorId");

            migrationBuilder.CreateIndex(
                name: "IX_Humidors_UserId",
                table: "Humidors",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cigars_Humidors_HumidorId",
                table: "Cigars",
                column: "HumidorId",
                principalTable: "Humidors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cigars_Humidors_HumidorId",
                table: "Cigars");

            migrationBuilder.DropTable(
                name: "Humidors");

            migrationBuilder.DropIndex(
                name: "IX_Cigars_HumidorId",
                table: "Cigars");

            migrationBuilder.DropColumn(
                name: "HumidorId",
                table: "Cigars");
        }
    }
}
