using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCBank.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountInterestIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Type_LastInterestAppliedAt",
                table: "Accounts",
                columns: new[] { "Type", "LastInterestAppliedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Type_LastInterestAppliedAt",
                table: "Accounts");
        }
    }
}
