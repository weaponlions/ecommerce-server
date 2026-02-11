using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eShopServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MediaAssetId",
                table: "SocialIcons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MediaAssetId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MediaAssetId",
                table: "Collections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MediaAssetId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MediaAssetId",
                table: "CarouselSlides",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialIcons_MediaAssetId",
                table: "SocialIcons",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_MediaAssetId",
                table: "Products",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_MediaAssetId",
                table: "Collections",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_MediaAssetId",
                table: "Categories",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_CarouselSlides_MediaAssetId",
                table: "CarouselSlides",
                column: "MediaAssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarouselSlides_MediaAssets_MediaAssetId",
                table: "CarouselSlides",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_MediaAssets_MediaAssetId",
                table: "Categories",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_MediaAssets_MediaAssetId",
                table: "Collections",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_MediaAssets_MediaAssetId",
                table: "Products",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SocialIcons_MediaAssets_MediaAssetId",
                table: "SocialIcons",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarouselSlides_MediaAssets_MediaAssetId",
                table: "CarouselSlides");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_MediaAssets_MediaAssetId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Collections_MediaAssets_MediaAssetId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_MediaAssets_MediaAssetId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_SocialIcons_MediaAssets_MediaAssetId",
                table: "SocialIcons");

            migrationBuilder.DropIndex(
                name: "IX_SocialIcons_MediaAssetId",
                table: "SocialIcons");

            migrationBuilder.DropIndex(
                name: "IX_Products_MediaAssetId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Collections_MediaAssetId",
                table: "Collections");

            migrationBuilder.DropIndex(
                name: "IX_Categories_MediaAssetId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_CarouselSlides_MediaAssetId",
                table: "CarouselSlides");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "SocialIcons");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "CarouselSlides");
        }
    }
}
