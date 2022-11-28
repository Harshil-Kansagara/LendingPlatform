using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _12_AddVersionRelatedColumnsInTaxFinanceAndCreditReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LoanApplicationId",
                table: "EntityTaxForm",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SurrogateId",
                table: "EntityTaxForm",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "EntityTaxForm",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SurrogateId",
                table: "EntityFinance",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "EntityFinance",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LoanApplicationId",
                table: "CreditReport",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SurrogateId",
                table: "CreditReport",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "CreditReport",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityTaxForm_CreatedOn",
                table: "EntityTaxForm",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTaxForm_LoanApplicationId",
                table: "EntityTaxForm",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTaxForm_Version",
                table: "EntityTaxForm",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinance_CreatedOn",
                table: "EntityFinance",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinance_Version",
                table: "EntityFinance",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_CreditReport_CreatedOn",
                table: "CreditReport",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_CreditReport_LoanApplicationId",
                table: "CreditReport",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditReport_Version",
                table: "CreditReport",
                column: "Version");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditReport_LoanApplication_LoanApplicationId",
                table: "CreditReport",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditReport_LoanApplication_LoanApplicationId",
                table: "CreditReport");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityTaxForm_LoanApplication_LoanApplicationId",
                table: "EntityTaxForm");

            migrationBuilder.DropIndex(
                name: "IX_EntityTaxForm_CreatedOn",
                table: "EntityTaxForm");

            migrationBuilder.DropIndex(
                name: "IX_EntityTaxForm_LoanApplicationId",
                table: "EntityTaxForm");

            migrationBuilder.DropIndex(
                name: "IX_EntityTaxForm_Version",
                table: "EntityTaxForm");

            migrationBuilder.DropIndex(
                name: "IX_EntityFinance_CreatedOn",
                table: "EntityFinance");

            migrationBuilder.DropIndex(
                name: "IX_EntityFinance_Version",
                table: "EntityFinance");

            migrationBuilder.DropIndex(
                name: "IX_CreditReport_CreatedOn",
                table: "CreditReport");

            migrationBuilder.DropIndex(
                name: "IX_CreditReport_LoanApplicationId",
                table: "CreditReport");

            migrationBuilder.DropIndex(
                name: "IX_CreditReport_Version",
                table: "CreditReport");

            migrationBuilder.DropColumn(
                name: "LoanApplicationId",
                table: "EntityTaxForm");

            migrationBuilder.DropColumn(
                name: "SurrogateId",
                table: "EntityTaxForm");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "EntityTaxForm");

            migrationBuilder.DropColumn(
                name: "SurrogateId",
                table: "EntityFinance");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "EntityFinance");

            migrationBuilder.DropColumn(
                name: "LoanApplicationId",
                table: "CreditReport");

            migrationBuilder.DropColumn(
                name: "SurrogateId",
                table: "CreditReport");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "CreditReport");
        }
    }
}
