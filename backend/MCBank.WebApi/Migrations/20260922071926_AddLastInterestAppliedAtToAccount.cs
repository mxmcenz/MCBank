using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCBank.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLastInterestAppliedAtToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastInterestAppliedAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastInterestAppliedAt",
                table: "Accounts");
        }
    }
}
