using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260810110000_CreateCondominiumUnits")]
    public partial class CreateCondominiumUnits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CondominiumUnits",
                columns: table => new
                {
                    CondominiumUnitId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    CondominiumId = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CondominiumUnits", x => x.CondominiumUnitId);
                    table.ForeignKey("FK_CondominiumUnits_Condominiums_CondominiumId", x => x.CondominiumId, "Condominiums", "CondominiumId", onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex(
                name: "IX_CondominiumUnits_CondominiumId_UnitNumber",
                table: "CondominiumUnits",
                columns: new[] { "CondominiumId", "UnitNumber" },
                unique: true);
            migrationBuilder.Sql(
                "INSERT INTO [CondominiumUnits] ([CondominiumId], [UnitNumber], [IsActive], [LastUpdatedDate]) " +
                "SELECT DISTINCT [CondominiumId], [UnitNumber], 1, SYSUTCDATETIME() FROM [TenantAssignments]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CondominiumUnits");
        }
    }
}
