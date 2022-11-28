using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _14_Add_UserLoanSectionMapping_And_SubLoanPurpose_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplication_Section_SectionId",
                table: "LoanApplication");

            migrationBuilder.AlterColumn<Guid>(
                name: "SectionId",
                table: "LoanApplication",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "SubLoanPurposeId",
                table: "LoanApplication",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubLoanPurpose",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    LoanPurposeId = table.Column<Guid>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubLoanPurpose", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubLoanPurpose_LoanPurpose_LoanPurposeId",
                        column: x => x.LoanPurposeId,
                        principalTable: "LoanPurpose",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLoanSectionMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    LoanApplicationId = table.Column<Guid>(nullable: false),
                    SectionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLoanSectionMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLoanSectionMapping_LoanApplication_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLoanSectionMapping_Section_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Section",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLoanSectionMapping_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_SubLoanPurposeId",
                table: "LoanApplication",
                column: "SubLoanPurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubLoanPurpose_LoanPurposeId",
                table: "SubLoanPurpose",
                column: "LoanPurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubLoanPurpose_Name",
                table: "SubLoanPurpose",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLoanSectionMapping_LoanApplicationId",
                table: "UserLoanSectionMapping",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLoanSectionMapping_SectionId",
                table: "UserLoanSectionMapping",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLoanSectionMapping_UserId",
                table: "UserLoanSectionMapping",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplication_Section_SectionId",
                table: "LoanApplication",
                column: "SectionId",
                principalTable: "Section",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplication_SubLoanPurpose_SubLoanPurposeId",
                table: "LoanApplication",
                column: "SubLoanPurposeId",
                principalTable: "SubLoanPurpose",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplication_Section_SectionId",
                table: "LoanApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplication_SubLoanPurpose_SubLoanPurposeId",
                table: "LoanApplication");

            migrationBuilder.DropTable(
                name: "SubLoanPurpose");

            migrationBuilder.DropTable(
                name: "UserLoanSectionMapping");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplication_SubLoanPurposeId",
                table: "LoanApplication");

            migrationBuilder.DropColumn(
                name: "SubLoanPurposeId",
                table: "LoanApplication");

            migrationBuilder.AlterColumn<Guid>(
                name: "SectionId",
                table: "LoanApplication",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplication_Section_SectionId",
                table: "LoanApplication",
                column: "SectionId",
                principalTable: "Section",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
