using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewAxisScoresAndCigarBaseReviewStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AromaScore",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BodyStrengthScore",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PairingsScore",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReviewAvgAromaScore",
                table: "CigarBases",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReviewAvgBodyStrength",
                table: "CigarBases",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReviewAvgPairingsScore",
                table: "CigarBases",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewScoredReviewCount",
                table: "CigarBases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Пересчёт агрегатов по существующим неудалённым отзывам (оси optional — среднее только по заполненным).
            migrationBuilder.Sql(
                """
                UPDATE "CigarBases" AS cb
                SET
                  "ReviewAvgBodyStrength" = s.avg_body,
                  "ReviewAvgAromaScore" = s.avg_aroma,
                  "ReviewAvgPairingsScore" = s.avg_pairings,
                  "ReviewScoredReviewCount" = s.scored_cnt
                FROM (
                  SELECT
                    r."CigarBaseId",
                    AVG(r."BodyStrengthScore"::decimal) FILTER (WHERE r."BodyStrengthScore" IS NOT NULL) AS avg_body,
                    AVG(r."AromaScore"::decimal) FILTER (WHERE r."AromaScore" IS NOT NULL) AS avg_aroma,
                    AVG(r."PairingsScore"::decimal) FILTER (WHERE r."PairingsScore" IS NOT NULL) AS avg_pairings,
                    COUNT(*) FILTER (
                      WHERE r."BodyStrengthScore" IS NOT NULL
                         OR r."AromaScore" IS NOT NULL
                         OR r."PairingsScore" IS NOT NULL) AS scored_cnt
                  FROM "Reviews" AS r
                  WHERE r."DeletedAt" IS NULL
                  GROUP BY r."CigarBaseId"
                ) AS s
                WHERE cb."Id" = s."CigarBaseId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AromaScore",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "BodyStrengthScore",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "PairingsScore",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ReviewAvgAromaScore",
                table: "CigarBases");

            migrationBuilder.DropColumn(
                name: "ReviewAvgBodyStrength",
                table: "CigarBases");

            migrationBuilder.DropColumn(
                name: "ReviewAvgPairingsScore",
                table: "CigarBases");

            migrationBuilder.DropColumn(
                name: "ReviewScoredReviewCount",
                table: "CigarBases");
        }
    }
}
