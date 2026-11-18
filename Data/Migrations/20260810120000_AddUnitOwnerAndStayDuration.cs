using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260810120000_AddUnitOwnerAndStayDuration")]
    public partial class AddUnitOwnerAndStayDuration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "OwnerName", table: "CondominiumUnits", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<int>(name: "StayDuration", table: "TenantAssignments", type: "int", nullable: true);
            migrationBuilder.AddColumn<string>(name: "StayDurationUnit", table: "TenantAssignments", type: "nvarchar(10)", maxLength: 10, nullable: true);
            migrationBuilder.Sql(
                "UPDATE [TenantAssignments] SET " +
                "[StayDuration] = TRY_CONVERT(INT, LEFT(LTRIM([MonthsOfStay]), CHARINDEX(' ', LTRIM([MonthsOfStay]) + ' ') - 1)), " +
                "[StayDurationUnit] = CASE WHEN LOWER([MonthsOfStay]) LIKE '%week%' THEN 'Weeks' WHEN LOWER([MonthsOfStay]) LIKE '%day%' THEN 'Days' ELSE 'Months' END " +
                "WHERE [MonthsOfStay] IS NOT NULL");
            migrationBuilder.DropColumn(name: "MonthsOfStay", table: "TenantAssignments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "MonthsOfStay", table: "TenantAssignments", type: "nvarchar(50)", maxLength: 50, nullable: true);
            migrationBuilder.Sql(
                "UPDATE [TenantAssignments] SET [MonthsOfStay] = CONCAT([StayDuration], ' ', [StayDurationUnit]) WHERE [StayDuration] IS NOT NULL");
            migrationBuilder.DropColumn(name: "StayDuration", table: "TenantAssignments");
            migrationBuilder.DropColumn(name: "StayDurationUnit", table: "TenantAssignments");
            migrationBuilder.DropColumn(name: "OwnerName", table: "CondominiumUnits");
        }
    }
}
