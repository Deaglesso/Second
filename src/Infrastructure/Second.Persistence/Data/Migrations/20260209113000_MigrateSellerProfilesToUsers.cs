using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Second.Persistence.Data.Migrations
{
    public partial class MigrateSellerProfilesToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_SellerProfiles_SellerProfileId",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "SellerUserId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE p
                  SET p.SellerUserId = sp.UserId
                  FROM Products p
                  INNER JOIN SellerProfiles sp ON sp.Id = p.SellerProfileId;");

            migrationBuilder.Sql(
                @"INSERT INTO Users (
                        Id,
                        Email,
                        PasswordHash,
                        Role,
                        EmailVerified,
                        CreatedAt,
                        UpdatedAt,
                        IsDeleted,
                        DeletedAt,
                        EmailVerificationTokenHash,
                        EmailVerificationTokenExpiresAtUtc,
                        PasswordResetTokenHash,
                        PasswordResetTokenExpiresAtUtc)
                  SELECT
                        sp.UserId,
                        CONCAT('legacy-seller-', CONVERT(varchar(36), sp.UserId), '@local.invalid'),
                        '',
                        1,
                        1,
                        SYSUTCDATETIME(),
                        NULL,
                        0,
                        NULL,
                        NULL,
                        NULL,
                        NULL,
                        NULL
                  FROM SellerProfiles sp
                  LEFT JOIN Users u ON u.Id = sp.UserId
                  WHERE u.Id IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "SellerUserId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerProfileId",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerUserId",
                table: "Products",
                column: "SellerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_SellerUserId",
                table: "Products",
                column: "SellerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "SellerProfileId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "SellerProfiles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_SellerUserId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerUserId",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "SellerProfileId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SellerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerProfiles", x => x.Id);
                });

            migrationBuilder.Sql(
                @"INSERT INTO SellerProfiles (Id, UserId, DisplayName, Bio, Status, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
                  SELECT
                    p.SellerUserId,
                    p.SellerUserId,
                    CONCAT('Seller ', CONVERT(varchar(8), p.SellerUserId)),
                    NULL,
                    1,
                    SYSUTCDATETIME(),
                    NULL,
                    0,
                    NULL
                  FROM Products p
                  GROUP BY p.SellerUserId;");

            migrationBuilder.Sql(
                @"UPDATE p
                  SET p.SellerProfileId = p.SellerUserId
                  FROM Products p;");

            migrationBuilder.AlterColumn<Guid>(
                name: "SellerProfileId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerProfileId",
                table: "Products",
                column: "SellerProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SellerProfiles_SellerProfileId",
                table: "Products",
                column: "SellerProfileId",
                principalTable: "SellerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropColumn(
                name: "SellerUserId",
                table: "Products");
        }
    }
}
