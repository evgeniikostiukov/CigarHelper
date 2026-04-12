using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitCigarBaseSizeIntoLengthAndDiameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LengthMm",
                table: "CigarBases",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Diameter",
                table: "CigarBases",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "CigarBases"
                SET
                    "LengthMm" = (regexp_match(btrim("Size"), '^(\d+)\s*[xX×]\s*(\d+)$'))[1]::integer,
                    "Diameter" = (regexp_match(btrim("Size"), '^(\d+)\s*[xX×]\s*(\d+)$'))[2]::integer
                WHERE "Size" IS NOT NULL
                  AND regexp_match(btrim("Size"), '^(\d+)\s*[xX×]\s*(\d+)$') IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "Size",
                table: "CigarBases");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "CigarBases",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "CigarBases"
                SET "Size" =
                    CASE
                        WHEN "LengthMm" IS NOT NULL AND "Diameter" IS NOT NULL
                            THEN "LengthMm"::text || ' x ' || "Diameter"::text
                        ELSE NULL
                    END;
                """);

            migrationBuilder.DropColumn(
                name: "Diameter",
                table: "CigarBases");

            migrationBuilder.DropColumn(
                name: "LengthMm",
                table: "CigarBases");
        }
    }
}
