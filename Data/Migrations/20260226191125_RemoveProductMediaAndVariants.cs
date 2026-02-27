using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eShopServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductMediaAndVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_MediaAssets_MediaAssetId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_MediaAssetId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_VariantGroupId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VariantGroupId",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MediaAssetId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantGroupId",
                table: "Products",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Products_MediaAssetId",
                table: "Products",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_VariantGroupId",
                table: "Products",
                column: "VariantGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_MediaAssets_MediaAssetId",
                table: "Products",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
