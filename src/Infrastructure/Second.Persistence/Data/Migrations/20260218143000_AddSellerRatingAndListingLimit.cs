using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Second.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerRatingAndListingLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ListingLimit",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<decimal>(
                name: "SellerRating",
                table: "Users",
                type: "decimal(2,1)",
                precision: 2,
                scale: 1,
                nullable: false,
                defaultValue: 0.0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_ListingLimit",
                table: "Users",
                sql: "[ListingLimit] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_SellerRating",
                table: "Users",
                sql: "[SellerRating] >= 0 AND [SellerRating] <= 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_ListingLimit",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_SellerRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ListingLimit",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerRating",
                table: "Users");
        }
    }
}
