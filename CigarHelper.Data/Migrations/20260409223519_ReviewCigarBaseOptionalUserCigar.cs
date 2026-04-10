using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReviewCigarBaseOptionalUserCigar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_UserCigars_CigarId",
                table: "Reviews");

            migrationBuilder.AddColumn<int>(
                name: "CigarBaseId",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Reviews" AS r
                SET "CigarBaseId" = uc."CigarBaseId"
                FROM "UserCigars" AS uc
                WHERE r."CigarId" = uc."Id";
                """);

            migrationBuilder.AlterColumn<int>(
                name: "CigarBaseId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CigarId",
                table: "Reviews",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CigarBaseId",
                table: "Reviews",
                column: "CigarBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_CigarBases_CigarBaseId",
                table: "Reviews",
                column: "CigarBaseId",
                principalTable: "CigarBases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_UserCigars_CigarId",
                table: "Reviews",
                column: "CigarId",
                principalTable: "UserCigars",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_CigarBases_CigarBaseId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_UserCigars_CigarId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_CigarBaseId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CigarBaseId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "CigarId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_UserCigars_CigarId",
                table: "Reviews",
                column: "CigarId",
                principalTable: "UserCigars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
