using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationFieldToCigarBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsModerated",
                table: "CigarBases",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsModerated",
                table: "CigarBases");
        }
    }
}
