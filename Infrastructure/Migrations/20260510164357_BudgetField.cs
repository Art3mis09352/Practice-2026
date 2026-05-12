using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Migrations
{
    /// <inheritdoc />
    public partial class BudgetField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RouteDayBlocks_RouteDayId",
                table: "RouteDayBlocks");

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Routes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "RouteDayBlocks",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteDayBlocks_RouteDayId_OrderInDay",
                table: "RouteDayBlocks",
                columns: new[] { "RouteDayId", "OrderInDay" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RouteDayBlocks_RouteDayId_OrderInDay",
                table: "RouteDayBlocks");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Routes");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "RouteDayBlocks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteDayBlocks_RouteDayId",
                table: "RouteDayBlocks",
                column: "RouteDayId");
        }
    }
}
