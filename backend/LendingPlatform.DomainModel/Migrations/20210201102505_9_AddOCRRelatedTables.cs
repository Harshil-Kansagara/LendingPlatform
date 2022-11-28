using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _9_AddOCRRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentExtractedValueSnapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ExtractedValueJson = table.Column<string>(type: "jsonb", nullable: false),
                    LoanApplicationSnapshotId = table.Column<Guid>(nullable: false),
                    DocumentId = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByBankUserId = table.Column<Guid>(nullable: true)
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

            migrationBuilder.CreateTable(
                name: "OCRModelMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ModelId = table.Column<string>(nullable: false),
                    Year = table.Column<string>(nullable: true),
                    CompanyStructureId = table.Column<Guid>(nullable: true),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OCRModelMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OCRModelMapping_CompanyStructure_CompanyStructureId",
                        column: x => x.CompanyStructureId,
                        principalTable: "CompanyStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxFormLabelNameMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LabelFieldName = table.Column<string>(nullable: true),
                    TaxFormId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxFormLabelNameMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxFormLabelNameMapping_TaxForm_TaxFormId",
                        column: x => x.TaxFormId,
                        principalTable: "TaxForm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxFormValueLabelMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    CorrectedValue = table.Column<string>(nullable: true),
                    Confidence = table.Column<float>(nullable: false),
                    TaxformLabelNameMappingId = table.Column<Guid>(nullable: false),
                    EntityTaxYearlyMappingId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxFormValueLabelMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxFormValueLabelMapping_EntityTaxYearlyMapping_EntityTaxYe~",
                        column: x => x.EntityTaxYearlyMappingId,
                        principalTable: "EntityTaxYearlyMapping",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaxFormValueLabelMapping_TaxFormLabelNameMapping_TaxformLab~",
                        column: x => x.TaxformLabelNameMappingId,
                        principalTable: "TaxFormLabelNameMapping",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_OCRModelMapping_CompanyStructureId",
                table: "OCRModelMapping",
                column: "CompanyStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxFormLabelNameMapping_TaxFormId",
                table: "TaxFormLabelNameMapping",
                column: "TaxFormId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxFormValueLabelMapping_EntityTaxYearlyMappingId",
                table: "TaxFormValueLabelMapping",
                column: "EntityTaxYearlyMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxFormValueLabelMapping_TaxformLabelNameMappingId",
                table: "TaxFormValueLabelMapping",
                column: "TaxformLabelNameMappingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentExtractedValueSnapshot");

            migrationBuilder.DropTable(
                name: "OCRModelMapping");

            migrationBuilder.DropTable(
                name: "TaxFormValueLabelMapping");

            migrationBuilder.DropTable(
                name: "TaxFormLabelNameMapping");
        }
    }
}
