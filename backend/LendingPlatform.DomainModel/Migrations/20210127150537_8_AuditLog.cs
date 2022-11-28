using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _8_AuditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "InterestRate",
                table: "LoanApplication",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TableName = table.Column<string>(nullable: true),
                    Action = table.Column<string>(nullable: true),
                    AuditJson = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: true),
                    CreatedByBankUserId = table.Column<Guid>(nullable: true),
                    TablePk = table.Column<Guid>(nullable: false),
                    IpAddress = table.Column<string>(nullable: true),
                    LogBlockName = table.Column<int>(nullable: false),
                    LogBlockNameId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_BankUser_CreatedByBankUserId",
                        column: x => x.CreatedByBankUserId,
                        principalTable: "BankUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLog_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CreatedByBankUserId",
                table: "AuditLog",
                column: "CreatedByBankUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CreatedByUserId",
                table: "AuditLog",
                column: "CreatedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.AlterColumn<decimal>(
                name: "InterestRate",
                table: "LoanApplication",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);
        }
    }
}
