using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _5_UpdateColumnNameInLoanApplication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplication_BankUser_StatusUpdatedByBankUserId",
                table: "LoanApplication");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplication_StatusUpdatedByBankUserId",
                table: "LoanApplication");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedByBankUserId",
                table: "LoanApplication");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByBankUserId",
                table: "LoanApplication",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_UpdatedByBankUserId",
                table: "LoanApplication",
                column: "UpdatedByBankUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplication_BankUser_UpdatedByBankUserId",
                table: "LoanApplication",
                column: "UpdatedByBankUserId",
                principalTable: "BankUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplication_BankUser_UpdatedByBankUserId",
                table: "LoanApplication");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplication_UpdatedByBankUserId",
                table: "LoanApplication");

            migrationBuilder.DropColumn(
                name: "UpdatedByBankUserId",
                table: "LoanApplication");

            migrationBuilder.AddColumn<Guid>(
                name: "StatusUpdatedByBankUserId",
                table: "LoanApplication",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_StatusUpdatedByBankUserId",
                table: "LoanApplication",
                column: "StatusUpdatedByBankUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplication_BankUser_StatusUpdatedByBankUserId",
                table: "LoanApplication",
                column: "StatusUpdatedByBankUserId",
                principalTable: "BankUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
