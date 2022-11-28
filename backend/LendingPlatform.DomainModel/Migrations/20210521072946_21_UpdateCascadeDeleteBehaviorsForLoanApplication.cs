using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _21_UpdateCascadeDeleteBehaviorsForLoanApplication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditReport_LoanApplication_LoanApplicationId",
                table: "CreditReport");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityAdditionalDocument_LoanApplication_LoanApplicationId",
                table: "EntityAdditionalDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityFinance_LoanApplication_LoanApplicationId",
                table: "EntityFinance");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityTaxForm_LoanApplication_LoanApplicationId",
                table: "EntityTaxForm");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditReport_LoanApplication_LoanApplicationId",
                table: "CreditReport",
                column: "LoanApplicationId",
                principalTable: "LoanApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityAdditionalDocument_LoanApplication_LoanApplicationId",
                table: "EntityAdditionalDocument",
                column: "LoanApplicationId",
                principalTable: "LoanApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityFinance_LoanApplication_LoanApplicationId",
                table: "EntityFinance",
                column: "LoanApplicationId",
                principalTable: "LoanApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityTaxForm_LoanApplication_LoanApplicationId",
                table: "EntityTaxForm",
                column: "LoanApplicationId",
                principalTable: "LoanApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditReport_LoanApplication_LoanApplicationId",
                table: "CreditReport");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityAdditionalDocument_LoanApplication_LoanApplicationId",
                table: "EntityAdditionalDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityFinance_LoanApplication_LoanApplicationId",
                table: "EntityFinance");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityTaxForm_LoanApplication_LoanApplicationId",
                table: "EntityTaxForm");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditReport_LoanApplication_LoanApplicationId",
                table: "CreditReport",
                column: "LoanApplicationId",
                principalTable: "LoanApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityAdditionalDocument_LoanApplication_LoanApplicationId",
                table: "EntityAdditionalDocument",
                column: "LoanApplicationId",
                principalTable: "LoanApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityFinance_LoanApplication_LoanApplicationId",
                table: "EntityFinance",
                column: "LoanApplicationId",
                principalTable: "LoanApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityTaxForm_LoanApplication_LoanApplicationId",
                table: "EntityTaxForm",
                column: "LoanApplicationId",
                principalTable: "LoanApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
