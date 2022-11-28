using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _7_AddSourceJsonForStandardAccountsInFinance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceJson",
                table: "EntityFinanceStandardAccount",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceJson",
                table: "EntityFinanceStandardAccount");
        }
    }
}
