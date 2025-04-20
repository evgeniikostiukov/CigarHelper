using Microsoft.EntityFrameworkCore.Migrations;

namespace CigarHelper.Data.Migrations;

public partial class UpdateReviewRatingsToInt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Добавим временные колонки числового типа
        migrationBuilder.AddColumn<int>(
            name: "ConstructionInt",
            table: "Reviews",
            type: "integer",
            nullable: true);
            
        migrationBuilder.AddColumn<int>(
            name: "BurnQualityInt",
            table: "Reviews",
            type: "integer",
            nullable: true);
            
        migrationBuilder.AddColumn<int>(
            name: "DrawInt",
            table: "Reviews",
            type: "integer",
            nullable: true);
            
        // Заполним временные колонки значениями по умолчанию (3)
        // или попытаемся конвертировать строковые значения в числа
        migrationBuilder.Sql(@"
            UPDATE ""Reviews"" 
            SET 
                ""ConstructionInt"" = CASE 
                    WHEN ""Construction"" ~ E'^\\d+$' THEN LEAST(5, GREATEST(1, CAST(""Construction"" AS INTEGER)))
                    ELSE 3 
                END,
                ""BurnQualityInt"" = CASE 
                    WHEN ""BurnQuality"" ~ E'^\\d+$' THEN LEAST(5, GREATEST(1, CAST(""BurnQuality"" AS INTEGER)))
                    ELSE 3 
                END,
                ""DrawInt"" = CASE 
                    WHEN ""Draw"" ~ E'^\\d+$' THEN LEAST(5, GREATEST(1, CAST(""Draw"" AS INTEGER)))
                    ELSE 3 
                END
            WHERE ""Construction"" IS NOT NULL OR ""BurnQuality"" IS NOT NULL OR ""Draw"" IS NOT NULL
        ");
        
        // Удалим старые строковые колонки
        migrationBuilder.DropColumn(
            name: "Construction",
            table: "Reviews");
            
        migrationBuilder.DropColumn(
            name: "BurnQuality",
            table: "Reviews");
            
        migrationBuilder.DropColumn(
            name: "Draw",
            table: "Reviews");
            
        // Переименуем временные колонки в целевые имена
        migrationBuilder.RenameColumn(
            name: "ConstructionInt",
            table: "Reviews",
            newName: "Construction");
            
        migrationBuilder.RenameColumn(
            name: "BurnQualityInt",
            table: "Reviews",
            newName: "BurnQuality");
            
        migrationBuilder.RenameColumn(
            name: "DrawInt",
            table: "Reviews",
            newName: "Draw");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Добавим временные строковые колонки
        migrationBuilder.AddColumn<string>(
            name: "ConstructionStr",
            table: "Reviews",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);
            
        migrationBuilder.AddColumn<string>(
            name: "BurnQualityStr",
            table: "Reviews",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);
            
        migrationBuilder.AddColumn<string>(
            name: "DrawStr",
            table: "Reviews",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);
            
        // Заполним строковые колонки на основе числовых
        migrationBuilder.Sql(@"
            UPDATE ""Reviews"" 
            SET 
                ""ConstructionStr"" = CAST(""Construction"" AS TEXT),
                ""BurnQualityStr"" = CAST(""BurnQuality"" AS TEXT),
                ""DrawStr"" = CAST(""Draw"" AS TEXT)
            WHERE ""Construction"" IS NOT NULL OR ""BurnQuality"" IS NOT NULL OR ""Draw"" IS NOT NULL
        ");
        
        // Удалим числовые колонки
        migrationBuilder.DropColumn(
            name: "Construction",
            table: "Reviews");
            
        migrationBuilder.DropColumn(
            name: "BurnQuality",
            table: "Reviews");
            
        migrationBuilder.DropColumn(
            name: "Draw",
            table: "Reviews");
            
        // Переименуем временные строковые колонки в целевые имена
        migrationBuilder.RenameColumn(
            name: "ConstructionStr",
            table: "Reviews",
            newName: "Construction");
            
        migrationBuilder.RenameColumn(
            name: "BurnQualityStr",
            table: "Reviews",
            newName: "BurnQuality");
            
        migrationBuilder.RenameColumn(
            name: "DrawStr",
            table: "Reviews",
            newName: "Draw");
    }
} 