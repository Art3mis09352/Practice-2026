using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviewPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreviewPhotoId",
                table: "Blocks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_PreviewPhotoId",
                table: "Blocks",
                column: "PreviewPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_BlockPhotos_PreviewPhotoId",
                table: "Blocks",
                column: "PreviewPhotoId",
                principalTable: "BlockPhotos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_BlockPhotos_PreviewPhotoId",
                table: "Blocks");

            migrationBuilder.DropIndex(
                name: "IX_Blocks_PreviewPhotoId",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "PreviewPhotoId",
                table: "Blocks");
        }
    }
}
