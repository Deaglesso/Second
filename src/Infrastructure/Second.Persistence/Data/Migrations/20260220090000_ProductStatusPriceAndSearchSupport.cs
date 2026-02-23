using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Second.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductStatusPriceAndSearchSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.Sql("UPDATE Products SET Status = CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Archived' END");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Price",
                table: "Products",
                column: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Price",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Products SET IsActive = CASE WHEN Status = 'Active' THEN 1 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Products");
        }
    }
}
