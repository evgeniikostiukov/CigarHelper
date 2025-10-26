using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCurrentHumidityFromHumidor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentHumidity",
                table: "Humidors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentHumidity",
                table: "Humidors",
                type: "integer",
                nullable: true);
        }
    }
}
