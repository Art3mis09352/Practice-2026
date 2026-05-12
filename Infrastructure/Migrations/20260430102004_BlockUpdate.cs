using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Practice.Migrations
{
    /// <inheritdoc />
    public partial class BlockUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_AspNetUsers_OwnerId1",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_AspNetUsers_UserId1",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_UserId1",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Blocks_OwnerId1",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "ShareLink",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "TotalBudget",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "OwnerId1",
                table: "Blocks");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Routes",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Routes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Routes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShareToken",
                table: "Routes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Blocks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Blocks",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Blocks",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Blocks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvgPrice",
                table: "Blocks",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Blocks",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Blocks",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Blocks",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Blocks",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "RouteDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RouteId = table.Column<int>(type: "integer", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteDays_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteDayBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RouteDayId = table.Column<int>(type: "integer", nullable: false),
                    BlockId = table.Column<int>(type: "integer", nullable: false),
                    OrderInDay = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteDayBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteDayBlocks_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteDayBlocks_RouteDays_RouteDayId",
                        column: x => x.RouteDayId,
                        principalTable: "RouteDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ShareToken",
                table: "Routes",
                column: "ShareToken",
                unique: true,
                filter: "\"ShareToken\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_UserId",
                table: "Routes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_Category",
                table: "Blocks",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_City",
                table: "Blocks",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_IsApproved",
                table: "Blocks",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_OwnerId",
                table: "Blocks",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteDayBlocks_BlockId",
                table: "RouteDayBlocks",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteDayBlocks_RouteDayId",
                table: "RouteDayBlocks",
                column: "RouteDayId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteDays_RouteId_DayNumber",
                table: "RouteDays",
                columns: new[] { "RouteId", "DayNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_AspNetUsers_OwnerId",
                table: "Blocks",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_AspNetUsers_UserId",
                table: "Routes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_AspNetUsers_OwnerId",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_AspNetUsers_UserId",
                table: "Routes");

            migrationBuilder.DropTable(
                name: "RouteDayBlocks");

            migrationBuilder.DropTable(
                name: "RouteDays");

            migrationBuilder.DropIndex(
                name: "IX_Routes_ShareToken",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_UserId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Blocks_Category",
                table: "Blocks");

            migrationBuilder.DropIndex(
                name: "IX_Blocks_City",
                table: "Blocks");

            migrationBuilder.DropIndex(
                name: "IX_Blocks_IsApproved",
                table: "Blocks");

            migrationBuilder.DropIndex(
                name: "IX_Blocks_OwnerId",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "ShareToken",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Blocks");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Routes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Routes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Routes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShareLink",
                table: "Routes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBudget",
                table: "Routes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Routes",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Blocks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Blocks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Blocks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Blocks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AvgPrice",
                table: "Blocks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Blocks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId1",
                table: "Blocks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_UserId1",
                table: "Routes",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_OwnerId1",
                table: "Blocks",
                column: "OwnerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_AspNetUsers_OwnerId1",
                table: "Blocks",
                column: "OwnerId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_AspNetUsers_UserId1",
                table: "Routes",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
