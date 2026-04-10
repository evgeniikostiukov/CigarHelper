using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCigarImageBinaryData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CigarImages_CigarBases_CigarBaseId",
                table: "CigarImages");

            migrationBuilder.DropForeignKey(
                name: "FK_CigarImages_UserCigars_UserCigarId",
                table: "CigarImages");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "CigarImages",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CigarImages_CigarBases_CigarBaseId",
                table: "CigarImages",
                column: "CigarBaseId",
                principalTable: "CigarBases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CigarImages_UserCigars_UserCigarId",
                table: "CigarImages",
                column: "UserCigarId",
                principalTable: "UserCigars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CigarImages_CigarBases_CigarBaseId",
                table: "CigarImages");

            migrationBuilder.DropForeignKey(
                name: "FK_CigarImages_UserCigars_UserCigarId",
                table: "CigarImages");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "CigarImages");

            migrationBuilder.AddForeignKey(
                name: "FK_CigarImages_CigarBases_CigarBaseId",
                table: "CigarImages",
                column: "CigarBaseId",
                principalTable: "CigarBases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CigarImages_UserCigars_UserCigarId",
                table: "CigarImages",
                column: "UserCigarId",
                principalTable: "UserCigars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
