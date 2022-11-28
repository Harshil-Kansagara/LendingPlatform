using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _13_Remove_DocumentExtractedValueSnapshot_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentExtractedValueSnapshot");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentExtractedValueSnapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractedValueJson = table.Column<string>(type: "jsonb", nullable: false),
                    LoanApplicationSnapshotId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByBankUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentExtractedValueSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentExtractedValueSnapshot_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentExtractedValueSnapshot_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentExtractedValueSnapshot_LoanApplicationSnapshot_Loan~",
                        column: x => x.LoanApplicationSnapshotId,
                        principalTable: "LoanApplicationSnapshot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentExtractedValueSnapshot_BankUser_UpdatedByBankUserId",
                        column: x => x.UpdatedByBankUserId,
                        principalTable: "BankUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentExtractedValueSnapshot_CreatedByUserId",
                table: "DocumentExtractedValueSnapshot",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentExtractedValueSnapshot_DocumentId",
                table: "DocumentExtractedValueSnapshot",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentExtractedValueSnapshot_LoanApplicationSnapshotId",
                table: "DocumentExtractedValueSnapshot",
                column: "LoanApplicationSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentExtractedValueSnapshot_UpdatedByBankUserId",
                table: "DocumentExtractedValueSnapshot",
                column: "UpdatedByBankUserId");
        }
    }
}
