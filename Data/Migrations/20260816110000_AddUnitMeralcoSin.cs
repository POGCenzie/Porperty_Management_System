using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260816110000_AddUnitMeralcoSin")]
    public partial class AddUnitMeralcoSin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeralcoSin",
                table: "CondominiumUnits",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE unit_info
                SET unit_info.MeralcoSin = assignment_info.MeralcoSin
                FROM dbo.CondominiumUnits AS unit_info
                OUTER APPLY
                (
                    SELECT TOP (1) assignment.MeralcoSin
                    FROM dbo.TenantAssignments AS assignment
                    WHERE assignment.CondominiumId = unit_info.CondominiumId
                      AND assignment.UnitNumber = unit_info.UnitNumber
                      AND NULLIF(LTRIM(RTRIM(assignment.MeralcoSin)), N'') IS NOT NULL
                    ORDER BY assignment.LastUpdatedDate DESC, assignment.TenantAssignmentId DESC
                ) AS assignment_info
                WHERE unit_info.MeralcoSin IS NULL
                  AND assignment_info.MeralcoSin IS NOT NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeralcoSin",
                table: "CondominiumUnits");
        }
    }
}
