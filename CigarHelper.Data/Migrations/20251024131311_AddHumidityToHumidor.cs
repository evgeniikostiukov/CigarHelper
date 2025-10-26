using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHumidityToHumidor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Humidity",
                table: "Humidors",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Humidity",
                table: "Humidors");
        }
    }
}
