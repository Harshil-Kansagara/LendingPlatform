using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _15_AddPersonalFinanceRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityFinance_IntegratedServiceConfiguration_IntegratedServ~",
                table: "EntityFinance");

            migrationBuilder.AlterColumn<Guid>(
                name: "IntegratedServiceConfigurationId",
                table: "EntityFinance",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "PersonalFinanceAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalFinanceAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonalFinanceAttribute",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Text = table.Column<string>(nullable: false),
                    FieldType = table.Column<int>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalFinanceAttribute", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonalFinanceConstant",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    ValueJson = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalFinanceConstant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonalFinanceCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PersonalFinanceAccountId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalFinanceCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalFinanceCategory_PersonalFinanceAccount_PersonalFina~",
                        column: x => x.PersonalFinanceAccountId,
                        principalTable: "PersonalFinanceAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttributeCategoryMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CategoryId = table.Column<Guid>(nullable: false),
                    AttributeId = table.Column<Guid>(nullable: false),
                    ParentAttributeCategoryMappingId = table.Column<Guid>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    IsOriginal = table.Column<bool>(nullable: false),
                    IsCurrent = table.Column<bool>(nullable: false),
                    PersonalFinanceConstantId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeCategoryMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeCategoryMapping_PersonalFinanceAttribute_Attribute~",
                        column: x => x.AttributeId,
                        principalTable: "PersonalFinanceAttribute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttributeCategoryMapping_PersonalFinanceCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PersonalFinanceCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttributeCategoryMapping_AttributeCategoryMapping_ParentAtt~",
                        column: x => x.ParentAttributeCategoryMappingId,
                        principalTable: "AttributeCategoryMapping",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttributeCategoryMapping_PersonalFinanceConstant_PersonalFi~",
                        column: x => x.PersonalFinanceConstantId,
                        principalTable: "PersonalFinanceConstant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParentChildCategoryMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ParentCategoryId = table.Column<Guid>(nullable: false),
                    ChildCategoryId = table.Column<Guid>(nullable: false),
                    ParentAttributeCategoryMappingId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentChildCategoryMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentChildCategoryMapping_PersonalFinanceCategory_ChildCat~",
                        column: x => x.ChildCategoryId,
                        principalTable: "PersonalFinanceCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentChildCategoryMapping_AttributeCategoryMapping_ParentA~",
                        column: x => x.ParentAttributeCategoryMappingId,
                        principalTable: "AttributeCategoryMapping",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParentChildCategoryMapping_PersonalFinanceCategory_ParentCa~",
                        column: x => x.ParentCategoryId,
                        principalTable: "PersonalFinanceCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalFinanceResponse",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EntityFinanceId = table.Column<Guid>(nullable: false),
                    PersonalFinanceAttributeCategoryMappingId = table.Column<Guid>(nullable: false),
                    Answer = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    PersonalFinanceAttributeId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalFinanceResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalFinanceResponse_EntityFinance_EntityFinanceId",
                        column: x => x.EntityFinanceId,
                        principalTable: "EntityFinance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalFinanceResponse_AttributeCategoryMapping_PersonalFi~",
                        column: x => x.PersonalFinanceAttributeCategoryMappingId,
                        principalTable: "AttributeCategoryMapping",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalFinanceResponse_PersonalFinanceAttribute_PersonalFi~",
                        column: x => x.PersonalFinanceAttributeId,
                        principalTable: "PersonalFinanceAttribute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeCategoryMapping_AttributeId",
                table: "AttributeCategoryMapping",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeCategoryMapping_CategoryId",
                table: "AttributeCategoryMapping",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeCategoryMapping_ParentAttributeCategoryMappingId",
                table: "AttributeCategoryMapping",
                column: "ParentAttributeCategoryMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeCategoryMapping_PersonalFinanceConstantId",
                table: "AttributeCategoryMapping",
                column: "PersonalFinanceConstantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentChildCategoryMapping_ChildCategoryId",
                table: "ParentChildCategoryMapping",
                column: "ChildCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentChildCategoryMapping_ParentAttributeCategoryMappingId",
                table: "ParentChildCategoryMapping",
                column: "ParentAttributeCategoryMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentChildCategoryMapping_ParentCategoryId",
                table: "ParentChildCategoryMapping",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalFinanceCategory_PersonalFinanceAccountId",
                table: "PersonalFinanceCategory",
                column: "PersonalFinanceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalFinanceResponse_EntityFinanceId",
                table: "PersonalFinanceResponse",
                column: "EntityFinanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalFinanceResponse_PersonalFinanceAttributeCategoryMap~",
                table: "PersonalFinanceResponse",
                column: "PersonalFinanceAttributeCategoryMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalFinanceResponse_PersonalFinanceAttributeId",
                table: "PersonalFinanceResponse",
                column: "PersonalFinanceAttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityFinance_IntegratedServiceConfiguration_IntegratedServ~",
                table: "EntityFinance",
                column: "IntegratedServiceConfigurationId",
                principalTable: "IntegratedServiceConfiguration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityFinance_IntegratedServiceConfiguration_IntegratedServ~",
                table: "EntityFinance");

            migrationBuilder.DropTable(
                name: "ParentChildCategoryMapping");

            migrationBuilder.DropTable(
                name: "PersonalFinanceResponse");

            migrationBuilder.DropTable(
                name: "AttributeCategoryMapping");

            migrationBuilder.DropTable(
                name: "PersonalFinanceAttribute");

            migrationBuilder.DropTable(
                name: "PersonalFinanceCategory");

            migrationBuilder.DropTable(
                name: "PersonalFinanceConstant");

            migrationBuilder.DropTable(
                name: "PersonalFinanceAccount");

            migrationBuilder.AlterColumn<Guid>(
                name: "IntegratedServiceConfigurationId",
                table: "EntityFinance",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityFinance_IntegratedServiceConfiguration_IntegratedServ~",
                table: "EntityFinance",
                column: "IntegratedServiceConfigurationId",
                principalTable: "IntegratedServiceConfiguration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
