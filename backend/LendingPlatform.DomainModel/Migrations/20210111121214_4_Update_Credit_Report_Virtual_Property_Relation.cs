using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _4_Update_Credit_Report_Virtual_Property_Relation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditReport_LoanApplication_LoanApplicationId",
                table: "CreditReport");

            migrationBuilder.DropIndex(
                name: "IX_CreditReport_EntityId",
                table: "CreditReport");

            migrationBuilder.DropIndex(
                name: "IX_CreditReport_LoanApplicationId",
                table: "CreditReport");

            migrationBuilder.DropColumn(
                name: "LoanApplicationId",
                table: "CreditReport");

            migrationBuilder.CreateIndex(
                name: "IX_CreditReport_EntityId",
                table: "CreditReport",
                column: "EntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditReport_EntityId",
                table: "CreditReport");

            migrationBuilder.AddColumn<Guid>(
                name: "LoanApplicationId",
                table: "CreditReport",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditReport_EntityId",
                table: "CreditReport",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditReport_LoanApplicationId",
                table: "CreditReport",
                column: "LoanApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditReport_LoanApplication_LoanApplicationId",
                table: "CreditReport",
                column: "LoanApplicationId",
                principalTable: "LoanApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
