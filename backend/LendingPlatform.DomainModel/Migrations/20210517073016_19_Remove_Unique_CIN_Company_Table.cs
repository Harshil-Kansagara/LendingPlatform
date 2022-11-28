using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _19_Remove_Unique_CIN_Company_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Company_CIN",
                table: "Company");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Company_CIN",
                table: "Company",
                column: "CIN",
                unique: true);
        }
    }
}
