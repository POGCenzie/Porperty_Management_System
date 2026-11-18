using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260816130000_UseMonthsForAdvanceAndDeposit")]
    public partial class UseMonthsForAdvanceAndDeposit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerAdvanceMonths",
                table: "TenantAssignments",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "OwnerDepositMonths",
                table: "TenantAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE dbo.TenantAssignments
                SET OwnerAdvanceMonths = CASE
                        WHEN OwnerAdvanceAmount IS NULL OR OwnerAdvanceAmount <= 0 THEN NULL
                        WHEN OwnerAdvanceAmount <= 120 THEN CONVERT(int, ROUND(OwnerAdvanceAmount, 0))
                        WHEN RentalAmountToOwner > 0 THEN CONVERT(int, ROUND(OwnerAdvanceAmount / RentalAmountToOwner, 0))
                        ELSE NULL
                    END,
                    OwnerDepositMonths = CASE
                        WHEN OwnerDepositAmount IS NULL OR OwnerDepositAmount <= 0 THEN NULL
                        WHEN OwnerDepositAmount <= 120 THEN CONVERT(int, ROUND(OwnerDepositAmount, 0))
                        WHEN RentalAmountToOwner > 0 THEN CONVERT(int, ROUND(OwnerDepositAmount / RentalAmountToOwner, 0))
                        ELSE NULL
                    END;

                UPDATE statement_info
                SET RentalAmount = NULLIF(amount_info.Rent, 0),
                    AdvanceAmount = amount_info.Rent * COALESCE(assignment.OwnerAdvanceMonths, 0),
                    DepositAmount = amount_info.Rent * COALESCE(assignment.OwnerDepositMonths, 0),
                    BaseAmount = CASE
                        WHEN statement_info.SoaType = N'owner'
                            THEN amount_info.Rent * (COALESCE(assignment.OwnerAdvanceMonths, 0) + COALESCE(assignment.OwnerDepositMonths, 0))
                        ELSE amount_info.Rent * COALESCE(assignment.OwnerDepositMonths, 0)
                    END,
                    NetAmount = CASE
                        WHEN statement_info.SoaType = N'owner'
                            THEN amount_info.Rent * (COALESCE(assignment.OwnerAdvanceMonths, 0) + COALESCE(assignment.OwnerDepositMonths, 0))
                        ELSE amount_info.Rent * COALESCE(assignment.OwnerDepositMonths, 0)
                    END - statement_info.TotalDeductions,
                    LastUpdatedDate = SYSUTCDATETIME()
                FROM dbo.StatementsOfAccount AS statement_info
                INNER JOIN dbo.TenantAssignments AS assignment
                    ON assignment.TenantAssignmentId = statement_info.TenantAssignmentId
                CROSS APPLY
                (
                    SELECT COALESCE(statement_info.RentalAmount, assignment.RentalAmountToOwner, 0) AS Rent
                ) AS amount_info;");

            migrationBuilder.DropColumn(name: "OwnerAdvanceAmount", table: "TenantAssignments");
            migrationBuilder.DropColumn(name: "OwnerDepositAmount", table: "TenantAssignments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OwnerAdvanceAmount",
                table: "TenantAssignments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "OwnerDepositAmount",
                table: "TenantAssignments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE dbo.TenantAssignments
                SET OwnerAdvanceAmount = RentalAmountToOwner * OwnerAdvanceMonths,
                    OwnerDepositAmount = RentalAmountToOwner * OwnerDepositMonths;");

            migrationBuilder.DropColumn(name: "OwnerAdvanceMonths", table: "TenantAssignments");
            migrationBuilder.DropColumn(name: "OwnerDepositMonths", table: "TenantAssignments");
        }
    }
}
