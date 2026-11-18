using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropertyManagementSystem.Data;

#nullable disable

namespace PropertyManagementSystem.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260816120000_FileExistingStatementsOfAccount")]
    public partial class FileExistingStatementsOfAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE dbo.StatementsOfAccount
                SET Status = N'filed',
                    LastUpdatedDate = SYSUTCDATETIME()
                WHERE Status <> N'filed';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE dbo.StatementsOfAccount
                SET Status = N'draft',
                    LastUpdatedDate = SYSUTCDATETIME()
                WHERE Status = N'filed';");
        }
    }
}
