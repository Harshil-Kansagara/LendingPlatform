using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _16_AddAdditionalDocumentsRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalDocumentType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    DocumentTypeFor = table.Column<int>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalDocumentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityAdditionalDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    EntityId = table.Column<Guid>(nullable: false),
                    DocumentId = table.Column<Guid>(nullable: false),
                    AdditionalDocumentTypeId = table.Column<Guid>(nullable: false),
                    LoanApplicationId = table.Column<Guid>(nullable: true),
                    Version = table.Column<Guid>(nullable: true),
                    SurrogateId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityAdditionalDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityAdditionalDocument_AdditionalDocumentType_AdditionalD~",
                        column: x => x.AdditionalDocumentTypeId,
                        principalTable: "AdditionalDocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityAdditionalDocument_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityAdditionalDocument_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityAdditionalDocument_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityAdditionalDocument_LoanApplication_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityAdditionalDocument_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityAdditionalDocument_AdditionalDocumentTypeId",
                table: "EntityAdditionalDocument",
                column: "AdditionalDocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAdditionalDocument_CreatedByUserId",
                table: "EntityAdditionalDocument",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAdditionalDocument_DocumentId",
                table: "EntityAdditionalDocument",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAdditionalDocument_EntityId",
                table: "EntityAdditionalDocument",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAdditionalDocument_LoanApplicationId",
                table: "EntityAdditionalDocument",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAdditionalDocument_UpdatedByUserId",
                table: "EntityAdditionalDocument",
                column: "UpdatedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityAdditionalDocument");

            migrationBuilder.DropTable(
                name: "AdditionalDocumentType");
        }
    }
}
