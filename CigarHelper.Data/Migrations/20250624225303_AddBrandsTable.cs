using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Создаем таблицу Brands
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsModerated = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            // 2. Создаем уникальный индекс для названий брендов
            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name",
                unique: true);

            // 3. Добавляем колонку BrandId в CigarBases (временно nullable)
            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "CigarBases",
                type: "integer",
                nullable: true);

            // 4. Создаем бренды из существующих данных
            migrationBuilder.Sql(@"
                INSERT INTO ""Brands"" (""Name"", ""IsModerated"", ""CreatedAt"")
                SELECT DISTINCT ""Brand"", true, NOW()
                FROM ""CigarBases""
                WHERE ""Brand"" IS NOT NULL AND ""Brand"" != ''
                ON CONFLICT (""Name"") DO NOTHING;
            ");

            // 5. Обновляем BrandId в CigarBases
            migrationBuilder.Sql(@"
                UPDATE ""CigarBases""
                SET ""BrandId"" = b.""Id""
                FROM ""Brands"" b
                WHERE ""CigarBases"".""Brand"" = b.""Name"";
            ");

            // 6. Делаем BrandId NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "BrandId",
                table: "CigarBases",
                type: "integer",
                nullable: false);

            // 7. Удаляем старую колонку Brand
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "CigarBases");

            // 8. Создаем новый индекс для Name + BrandId
            migrationBuilder.CreateIndex(
                name: "IX_CigarBases_Name_BrandId",
                table: "CigarBases",
                columns: new[] { "Name", "BrandId" },
                unique: true);

            // 9. Создаем индекс для BrandId
            migrationBuilder.CreateIndex(
                name: "IX_CigarBases_BrandId",
                table: "CigarBases",
                column: "BrandId");

            // 10. Добавляем внешний ключ
            migrationBuilder.AddForeignKey(
                name: "FK_CigarBases_Brands_BrandId",
                table: "CigarBases",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Удаляем внешний ключ
            migrationBuilder.DropForeignKey(
                name: "FK_CigarBases_Brands_BrandId",
                table: "CigarBases");

            // 2. Удаляем индексы
            migrationBuilder.DropIndex(
                name: "IX_CigarBases_BrandId",
                table: "CigarBases");

            migrationBuilder.DropIndex(
                name: "IX_CigarBases_Name_BrandId",
                table: "CigarBases");

            // 3. Добавляем обратно колонку Brand
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "CigarBases",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // 4. Восстанавливаем данные Brand из Brands
            migrationBuilder.Sql(@"
                UPDATE ""CigarBases""
                SET ""Brand"" = b.""Name""
                FROM ""Brands"" b
                WHERE ""CigarBases"".""BrandId"" = b.""Id"";
            ");

            // 5. Удаляем колонку BrandId
            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "CigarBases");

            // 6. Создаем старый индекс
            migrationBuilder.CreateIndex(
                name: "IX_CigarBases_Name_Brand",
                table: "CigarBases",
                columns: new[] { "Name", "Brand" },
                unique: true);

            // 7. Удаляем таблицу Brands
            migrationBuilder.DropTable(
                name: "Brands");
        }
    }
}
