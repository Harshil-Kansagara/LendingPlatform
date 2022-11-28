using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _18_RemoveUnusedNavigationProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalFinanceResponse_PersonalFinanceAttribute_PersonalFi~",
                table: "PersonalFinanceResponse");

            migrationBuilder.DropIndex(
                name: "IX_PersonalFinanceResponse_PersonalFinanceAttributeId",
                table: "PersonalFinanceResponse");

            migrationBuilder.DropColumn(
                name: "PersonalFinanceAttributeId",
                table: "PersonalFinanceResponse");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PersonalFinanceAttributeId",
                table: "PersonalFinanceResponse",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalFinanceResponse_PersonalFinanceAttributeId",
                table: "PersonalFinanceResponse",
                column: "PersonalFinanceAttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalFinanceResponse_PersonalFinanceAttribute_PersonalFi~",
                table: "PersonalFinanceResponse",
                column: "PersonalFinanceAttributeId",
                principalTable: "PersonalFinanceAttribute",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
