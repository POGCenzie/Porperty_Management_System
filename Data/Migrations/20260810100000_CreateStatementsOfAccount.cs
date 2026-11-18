using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260810100000_CreateStatementsOfAccount")]
    public partial class CreateStatementsOfAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatementsOfAccount",
                columns: table => new
                {
                    StatementOfAccountId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    SoaNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoaType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenantAssignmentId = table.Column<int>(type: "int", nullable: false),
                    StatementDate = table.Column<DateTime>(type: "date", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementsOfAccount", x => x.StatementOfAccountId);
                    table.CheckConstraint("CK_StatementsOfAccount_SoaType", "[SoaType] IN ('owner', 'tenant')");
                    table.ForeignKey("FK_StatementsOfAccount_TenantAssignments_TenantAssignmentId", x => x.TenantAssignmentId, "TenantAssignments", "TenantAssignmentId", onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "SoaLineItems",
                columns: table => new
                {
                    SoaLineItemId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    StatementOfAccountId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoaLineItems", x => x.SoaLineItemId);
                    table.ForeignKey("FK_SoaLineItems_StatementsOfAccount_StatementOfAccountId", x => x.StatementOfAccountId, "StatementsOfAccount", "StatementOfAccountId", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex("IX_StatementsOfAccount_SoaNumber", "StatementsOfAccount", "SoaNumber", unique: true);
            migrationBuilder.CreateIndex("IX_StatementsOfAccount_TenantAssignmentId", "StatementsOfAccount", "TenantAssignmentId");
            migrationBuilder.CreateIndex("IX_SoaLineItems_StatementOfAccountId", "SoaLineItems", "StatementOfAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("SoaLineItems");
            migrationBuilder.DropTable("StatementsOfAccount");
        }
    }
}
