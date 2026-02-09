using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Second.Persistence.Data.Migrations
{
    public partial class RemoveSellerProfilesResiduals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL
                  BEGIN
                      IF EXISTS (
                          SELECT 1
                          FROM sys.foreign_keys
                          WHERE name = N'FK_Products_SellerProfiles_SellerProfileId')
                      BEGIN
                          ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_SellerProfiles_SellerProfileId];
                      END;

                      IF COL_LENGTH('Products', 'SellerProfileId') IS NOT NULL
                      BEGIN
                          IF EXISTS (
                              SELECT 1
                              FROM sys.indexes
                              WHERE name = N'IX_Products_SellerProfileId'
                                AND object_id = OBJECT_ID(N'[Products]'))
                          BEGIN
                              DROP INDEX [IX_Products_SellerProfileId] ON [Products];
                          END;

                          ALTER TABLE [Products] DROP COLUMN [SellerProfileId];
                      END;
                  END;");

            migrationBuilder.Sql(
                @"IF OBJECT_ID(N'[SellerProfiles]', N'U') IS NOT NULL
                  BEGIN
                      DROP TABLE [SellerProfiles];
                  END;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            // SellerProfiles is deprecated and should not be reintroduced.
        }
    }
}
