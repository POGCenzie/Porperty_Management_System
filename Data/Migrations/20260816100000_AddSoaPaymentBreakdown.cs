using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260816100000_AddSoaPaymentBreakdown")]
    public partial class AddSoaPaymentBreakdown : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RentalAmount",
                table: "StatementsOfAccount",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceAmount",
                table: "StatementsOfAccount",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "StatementsOfAccount",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "RentalAmount", table: "StatementsOfAccount");
            migrationBuilder.DropColumn(name: "AdvanceAmount", table: "StatementsOfAccount");
            migrationBuilder.DropColumn(name: "DepositAmount", table: "StatementsOfAccount");
        }
    }
}
