using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260809160000_CreatePropertyManagementTables")]
    public partial class CreatePropertyManagementTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Condominiums",
                columns: table => new
                {
                    CondominiumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table => table.PrimaryKey("PK_Condominiums", x => x.CondominiumId));

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table => table.PrimaryKey("PK_Tenants", x => x.TenantId));

            migrationBuilder.CreateTable(
                name: "TenantAssignments",
                columns: table => new
                {
                    TenantAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CondominiumId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OccupancyStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MeralcoSin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KeyRelease = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccessCardRelease = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RentalAmountToOwner = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    WithDues = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MonthsOfStay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartOfContract = table.Column<DateTime>(type: "date", nullable: true),
                    EndOfContract = table.Column<DateTime>(type: "date", nullable: true),
                    AirconMaintenanceDate = table.Column<DateTime>(type: "date", nullable: true),
                    MaintenanceStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OwnerAdvanceAndDeposit = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    OwnerEmailAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    OwnerContactNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MainTenantContactNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MainTenantEmailAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantAssignments", x => x.TenantAssignmentId);
                    table.ForeignKey("FK_TenantAssignments_Condominiums_CondominiumId", x => x.CondominiumId, "Condominiums", "CondominiumId", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_TenantAssignments_Tenants_TenantId", x => x.TenantId, "Tenants", "TenantId", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_Condominiums_Name", "Condominiums", "Name", unique: true);
            migrationBuilder.CreateIndex("IX_TenantAssignments_CondominiumId_UnitNumber", "TenantAssignments", new[] { "CondominiumId", "UnitNumber" });
            migrationBuilder.CreateIndex("IX_TenantAssignments_TenantId", "TenantAssignments", "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("TenantAssignments");
            migrationBuilder.DropTable("Condominiums");
            migrationBuilder.DropTable("Tenants");
        }
    }
}
