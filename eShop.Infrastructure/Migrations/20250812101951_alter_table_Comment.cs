using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class alter_table_Comment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Orders_OrderId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Comments",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_OrderId",
                table: "Comments",
                newName: "IX_Comments_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Products_ProductId",
                table: "Comments",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Products_ProductId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Comments",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ProductId",
                table: "Comments",
                newName: "IX_Comments_OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Orders_OrderId",
                table: "Comments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
