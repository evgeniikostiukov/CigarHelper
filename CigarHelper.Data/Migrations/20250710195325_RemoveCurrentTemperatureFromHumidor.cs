using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCurrentTemperatureFromHumidor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentTemperature",
                table: "Humidors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentTemperature",
                table: "Humidors",
                type: "numeric",
                nullable: true);
        }
    }
}
