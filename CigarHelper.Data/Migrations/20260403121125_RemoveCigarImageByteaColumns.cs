using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <summary>
    /// Удаляет bytea у CigarImages. Данные в колонках теряются: перед прод-применением вынести бинарники в MinIO
    /// (скрипт/одноразовый job), если они ещё только в БД.
    /// </summary>
    public partial class RemoveCigarImageByteaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "CigarImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailData",
                table: "CigarImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "CigarImages",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ThumbnailData",
                table: "CigarImages",
                type: "bytea",
                nullable: true);
        }
    }
}
