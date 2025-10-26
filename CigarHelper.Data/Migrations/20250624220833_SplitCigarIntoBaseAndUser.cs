using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitCigarIntoBaseAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Cigars_CigarId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "Cigars");

            migrationBuilder.CreateTable(
                name: "CigarBases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Strength = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CigarBases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCigars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CigarBaseId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    HumidorId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCigars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCigars_CigarBases_CigarBaseId",
                        column: x => x.CigarBaseId,
                        principalTable: "CigarBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCigars_Humidors_HumidorId",
                        column: x => x.HumidorId,
                        principalTable: "Humidors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserCigars_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CigarBases_Name_Brand",
                table: "CigarBases",
                columns: new[] { "Name", "Brand" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCigars_CigarBaseId",
                table: "UserCigars",
                column: "CigarBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCigars_HumidorId",
                table: "UserCigars",
                column: "HumidorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCigars_UserId",
                table: "UserCigars",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_UserCigars_CigarId",
                table: "Reviews",
                column: "CigarId",
                principalTable: "UserCigars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_UserCigars_CigarId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "UserCigars");

            migrationBuilder.DropTable(
                name: "CigarBases");

            migrationBuilder.CreateTable(
                name: "Cigars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HumidorId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    Size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Strength = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cigars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cigars_Humidors_HumidorId",
                        column: x => x.HumidorId,
                        principalTable: "Humidors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cigars_Users_UserId",
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
                name: "IX_Cigars_UserId",
                table: "Cigars",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Cigars_CigarId",
                table: "Reviews",
                column: "CigarId",
                principalTable: "Cigars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
