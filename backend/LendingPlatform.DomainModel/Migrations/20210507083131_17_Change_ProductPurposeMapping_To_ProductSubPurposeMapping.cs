using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _17_Change_ProductPurposeMapping_To_ProductSubPurposeMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPurposeMapping");

            migrationBuilder.CreateTable(
                name: "DescriptionPoint",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Text = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescriptionPoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DescriptionPoint_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSubPurposeMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    SubLoanPurposeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSubPurposeMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSubPurposeMapping_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductSubPurposeMapping_SubLoanPurpose_SubLoanPurposeId",
                        column: x => x.SubLoanPurposeId,
                        principalTable: "SubLoanPurpose",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionPoint_ProductId",
                table: "DescriptionPoint",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSubPurposeMapping_ProductId",
                table: "ProductSubPurposeMapping",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSubPurposeMapping_SubLoanPurposeId",
                table: "ProductSubPurposeMapping",
                column: "SubLoanPurposeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DescriptionPoint");

            migrationBuilder.DropTable(
                name: "ProductSubPurposeMapping");

            migrationBuilder.CreateTable(
                name: "ProductPurposeMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanPurposeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPurposeMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPurposeMapping_LoanPurpose_LoanPurposeId",
                        column: x => x.LoanPurposeId,
                        principalTable: "LoanPurpose",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPurposeMapping_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPurposeMapping_LoanPurposeId",
                table: "ProductPurposeMapping",
                column: "LoanPurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPurposeMapping_ProductId",
                table: "ProductPurposeMapping",
                column: "ProductId");
        }
    }
}
