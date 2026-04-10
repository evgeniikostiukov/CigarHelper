using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserRoleToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Users"" ALTER COLUMN ""Role"" DROP DEFAULT;");
            migrationBuilder.Sql(@"ALTER TABLE ""Users"" ALTER COLUMN ""Role"" TYPE int USING (
                CASE ""Role""
                    WHEN 'User' THEN 1
                    WHEN 'Moderator' THEN 2
                    WHEN 'Admin' THEN 3
                    ELSE 1
                END
            );");
            migrationBuilder.Sql(@"ALTER TABLE ""Users"" ALTER COLUMN ""Role"" SET DEFAULT 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
