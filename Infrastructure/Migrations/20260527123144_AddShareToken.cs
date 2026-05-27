using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Practice.Migrations
{
    /// <inheritdoc />
    public partial class AddShareToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Routes_ShareToken",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "ShareToken",
                table: "Routes");

            migrationBuilder.CreateTable(
                name: "RouteShareLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RouteId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteShareLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteShareLinks_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RouteShareLinks_RouteId",
                table: "RouteShareLinks",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteShareLinks_Token",
                table: "RouteShareLinks",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteShareLinks");

            migrationBuilder.AddColumn<string>(
                name: "ShareToken",
                table: "Routes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ShareToken",
                table: "Routes",
                column: "ShareToken",
                unique: true,
                filter: "\"ShareToken\" IS NOT NULL");
        }
    }
}
