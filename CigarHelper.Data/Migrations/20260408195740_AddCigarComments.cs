using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CigarHelper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCigarComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CigarComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuthorUserId = table.Column<int>(type: "integer", nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CigarBaseId = table.Column<int>(type: "integer", nullable: true),
                    UserCigarId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CigarComments", x => x.Id);
                    table.CheckConstraint("CK_CigarComments_SingleTarget", "(\"CigarBaseId\" IS NOT NULL AND \"UserCigarId\" IS NULL) OR (\"CigarBaseId\" IS NULL AND \"UserCigarId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_CigarComments_CigarBases_CigarBaseId",
                        column: x => x.CigarBaseId,
                        principalTable: "CigarBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CigarComments_UserCigars_UserCigarId",
                        column: x => x.UserCigarId,
                        principalTable: "UserCigars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CigarComments_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CigarComments_AuthorUserId",
                table: "CigarComments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CigarComments_CigarBaseId",
                table: "CigarComments",
                column: "CigarBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CigarComments_UserCigarId",
                table: "CigarComments",
                column: "UserCigarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CigarComments");
        }
    }
}
