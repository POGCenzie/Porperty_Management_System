using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260810130000_NormalizeAssignmentPaymentFields")]
    public partial class NormalizeAssignmentPaymentFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KeyRelease",
                table: "TenantAssignments",
                newName: "KeyReleaseLegacy");
            migrationBuilder.RenameColumn(
                name: "AccessCardRelease",
                table: "TenantAssignments",
                newName: "AccessCardReleaseLegacy");

            migrationBuilder.AddColumn<int>(
                name: "KeyRelease",
                table: "TenantAssignments",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "AccessCardRelease",
                table: "TenantAssignments",
                type: "int",
                nullable: true);
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

            migrationBuilder.Sql(
                "UPDATE [TenantAssignments] SET " +
                "[KeyRelease] = TRY_CONVERT(int, [KeyReleaseLegacy]), " +
                "[AccessCardRelease] = TRY_CONVERT(int, [AccessCardReleaseLegacy]), " +
                "[OwnerAdvanceAmount] = TRY_CONVERT(decimal(18,2), [OwnerAdvanceAndDeposit])");

            migrationBuilder.DropColumn(name: "KeyReleaseLegacy", table: "TenantAssignments");
            migrationBuilder.DropColumn(name: "AccessCardReleaseLegacy", table: "TenantAssignments");
            migrationBuilder.DropColumn(name: "OwnerAdvanceAndDeposit", table: "TenantAssignments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KeyRelease",
                table: "TenantAssignments",
                newName: "KeyReleaseNumber");
            migrationBuilder.RenameColumn(
                name: "AccessCardRelease",
                table: "TenantAssignments",
                newName: "AccessCardReleaseNumber");

            migrationBuilder.AddColumn<string>(
                name: "KeyRelease",
                table: "TenantAssignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "AccessCardRelease",
                table: "TenantAssignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "OwnerAdvanceAndDeposit",
                table: "TenantAssignments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [TenantAssignments] SET " +
                "[KeyRelease] = CONVERT(nvarchar(100), [KeyReleaseNumber]), " +
                "[AccessCardRelease] = CONVERT(nvarchar(100), [AccessCardReleaseNumber]), " +
                "[OwnerAdvanceAndDeposit] = CONVERT(nvarchar(150), COALESCE([OwnerAdvanceAmount], 0) + COALESCE([OwnerDepositAmount], 0))");

            migrationBuilder.DropColumn(name: "KeyReleaseNumber", table: "TenantAssignments");
            migrationBuilder.DropColumn(name: "AccessCardReleaseNumber", table: "TenantAssignments");
            migrationBuilder.DropColumn(name: "OwnerAdvanceAmount", table: "TenantAssignments");
            migrationBuilder.DropColumn(name: "OwnerDepositAmount", table: "TenantAssignments");
        }
    }
}
