using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260810140000_CreateOwnersAndLinkUnits")]
    public partial class CreateOwnersAndLinkUnits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    OwnerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table => table.PrimaryKey("PK_Owners", item => item.OwnerId));

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "CondominiumUnits",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                INSERT INTO [Owners] ([FullName], [EmailAddress], [ContactNumber], [IsActive], [LastUpdatedDate])
                SELECT owner_names.[FullName], assignment_info.[OwnerEmailAddress], assignment_info.[OwnerContactNumber], 1, SYSUTCDATETIME()
                FROM (
                    SELECT DISTINCT LTRIM(RTRIM([OwnerName])) AS [FullName]
                    FROM [CondominiumUnits]
                    WHERE NULLIF(LTRIM(RTRIM([OwnerName])), '') IS NOT NULL
                ) AS owner_names
                OUTER APPLY (
                    SELECT TOP (1) assignment.[OwnerEmailAddress], assignment.[OwnerContactNumber]
                    FROM [TenantAssignments] AS assignment
                    WHERE LTRIM(RTRIM(assignment.[OwnerName])) = owner_names.[FullName]
                    ORDER BY assignment.[LastUpdatedDate] DESC
                ) AS assignment_info;

                UPDATE unit_info
                SET [OwnerId] = owner_info.[OwnerId]
                FROM [CondominiumUnits] AS unit_info
                INNER JOIN [Owners] AS owner_info
                    ON owner_info.[FullName] = LTRIM(RTRIM(unit_info.[OwnerName]));");

            migrationBuilder.CreateIndex(name: "IX_Owners_FullName", table: "Owners", column: "FullName");
            migrationBuilder.CreateIndex(name: "IX_CondominiumUnits_OwnerId", table: "CondominiumUnits", column: "OwnerId");
            migrationBuilder.AddForeignKey(
                name: "FK_CondominiumUnits_Owners_OwnerId",
                table: "CondominiumUnits",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.DropColumn(name: "OwnerName", table: "CondominiumUnits");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "CondominiumUnits",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
            migrationBuilder.Sql(@"
                UPDATE unit_info
                SET [OwnerName] = owner_info.[FullName]
                FROM [CondominiumUnits] AS unit_info
                INNER JOIN [Owners] AS owner_info ON owner_info.[OwnerId] = unit_info.[OwnerId];");
            migrationBuilder.DropForeignKey(name: "FK_CondominiumUnits_Owners_OwnerId", table: "CondominiumUnits");
            migrationBuilder.DropIndex(name: "IX_CondominiumUnits_OwnerId", table: "CondominiumUnits");
            migrationBuilder.DropColumn(name: "OwnerId", table: "CondominiumUnits");
            migrationBuilder.DropTable(name: "Owners");
        }
    }
}
