using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _3_AddPropertiesInSectionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Order",
                table: "Section",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentSectionId",
                table: "Section",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "Section",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Section_ParentSectionId",
                table: "Section",
                column: "ParentSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Section_Section_ParentSectionId",
                table: "Section",
                column: "ParentSectionId",
                principalTable: "Section",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Section_Section_ParentSectionId",
                table: "Section");

            migrationBuilder.DropIndex(
                name: "IX_Section_ParentSectionId",
                table: "Section");

            migrationBuilder.DropColumn(
                name: "ParentSectionId",
                table: "Section");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Section");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "Section",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal));
        }
    }
}
