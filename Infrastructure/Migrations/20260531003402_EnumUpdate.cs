using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Migrations
{
    /// <inheritdoc />
    public partial class EnumUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Blocks_IsApproved",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Blocks");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Blocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_Status",
                table: "Blocks",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Blocks_Status",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Blocks");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Blocks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_IsApproved",
                table: "Blocks",
                column: "IsApproved");
        }
    }
}
