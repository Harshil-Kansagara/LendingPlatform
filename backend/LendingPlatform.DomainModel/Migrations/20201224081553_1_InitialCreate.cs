using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LendingPlatform.DomainModel.Migrations
{
    public partial class _1_InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bank",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    SWIFTCode = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessAge",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Age = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessAge", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySize",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Size = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySize", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyStructure",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Structure = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyStructure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Consent",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ConsentText = table.Column<string>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialStatement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    IsAutoCalculated = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialStatement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndustryExperience",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Experience = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryExperience", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegratedServiceConfiguration",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsServiceEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegratedServiceConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanRangeType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRangeType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TypeName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NAICSIndustryType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IndustryType = table.Column<string>(nullable: false),
                    IndustryCode = table.Column<string>(nullable: false),
                    NAICSParentSectorId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NAICSIndustryType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NAICSIndustryType_NAICSIndustryType_NAICSParentSectorId",
                        column: x => x.NAICSParentSectorId,
                        principalTable: "NAICSIndustryType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    ProductStartDate = table.Column<DateTime>(nullable: false),
                    ProductEndDate = table.Column<DateTime>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relationship",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Relation = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationship", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Section",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Section", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxForm",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxForm", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanPurpose",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    LoanTypeId = table.Column<Guid>(nullable: false),
                    IsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanPurpose", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanPurpose_LoanType_LoanTypeId",
                        column: x => x.LoanTypeId,
                        principalTable: "LoanType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductRangeTypeMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    RangeTypeId = table.Column<Guid>(nullable: false),
                    Minimum = table.Column<decimal>(nullable: false),
                    Maximum = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRangeTypeMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRangeTypeMapping_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductRangeTypeMapping_LoanRangeType_RangeTypeId",
                        column: x => x.RangeTypeId,
                        principalTable: "LoanRangeType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypeMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    LoanTypeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypeMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTypeMapping_LoanType_LoanTypeId",
                        column: x => x.LoanTypeId,
                        principalTable: "LoanType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTypeMapping_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxFormCompanyStructureMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyStructureId = table.Column<Guid>(nullable: false),
                    TaxFormId = table.Column<Guid>(nullable: false),
                    IsSoleProprietors = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxFormCompanyStructureMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxFormCompanyStructureMapping_CompanyStructure_CompanyStru~",
                        column: x => x.CompanyStructureId,
                        principalTable: "CompanyStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaxFormCompanyStructureMapping_TaxForm_TaxFormId",
                        column: x => x.TaxFormId,
                        principalTable: "TaxForm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoanPurposeRangeTypeMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LoanPurposeId = table.Column<Guid>(nullable: false),
                    LoanRangeTypeId = table.Column<Guid>(nullable: false),
                    Minimum = table.Column<decimal>(nullable: false),
                    Maximum = table.Column<decimal>(nullable: false),
                    StepperAmount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanPurposeRangeTypeMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanPurposeRangeTypeMapping_LoanPurpose_LoanPurposeId",
                        column: x => x.LoanPurposeId,
                        principalTable: "LoanPurpose",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanPurposeRangeTypeMapping_LoanRangeType_LoanRangeTypeId",
                        column: x => x.LoanRangeTypeId,
                        principalTable: "LoanRangeType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductPurposeMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    LoanPurposeId = table.Column<Guid>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Entity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AddressId = table.Column<Guid>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityRelationshipMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PrimaryEntityId = table.Column<Guid>(nullable: false),
                    RelativeEntityId = table.Column<Guid>(nullable: false),
                    RelationshipId = table.Column<Guid>(nullable: false),
                    SharePercentage = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityRelationshipMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityRelationshipMapping_Entity_PrimaryEntityId",
                        column: x => x.PrimaryEntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityRelationshipMapping_Relationship_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationship",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityRelationshipMapping_Entity_RelativeEntityId",
                        column: x => x.RelativeEntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    MiddleName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: false),
                    SSN = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    DOB = table.Column<DateTime>(nullable: true),
                    ResidencyStatus = table.Column<int>(nullable: true),
                    SelfDeclaredCreditScore = table.Column<string>(nullable: true),
                    HasBankruptcySelfDeclared = table.Column<bool>(nullable: false),
                    HasAnyJudgementsSelfDeclared = table.Column<bool>(nullable: false),
                    IsRegistered = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Entity_Id",
                        column: x => x.Id,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    PrimaryNumber = table.Column<string>(nullable: true),
                    StreetLine = table.Column<string>(nullable: false),
                    City = table.Column<string>(nullable: false),
                    StateAbbreviation = table.Column<string>(nullable: false),
                    StreetSuffix = table.Column<string>(nullable: true),
                    SecondaryNumber = table.Column<string>(nullable: true),
                    SecondaryDesignator = table.Column<string>(nullable: true),
                    ZipCode = table.Column<string>(nullable: false),
                    Response = table.Column<string>(type: "jsonb", nullable: false),
                    IntegratedServiceConfigurationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Address_IntegratedServiceConfiguration_IntegratedServiceCon~",
                        column: x => x.IntegratedServiceConfigurationId,
                        principalTable: "IntegratedServiceConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Address_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    CIN = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CompanyStructureId = table.Column<Guid>(nullable: false),
                    NAICSIndustryTypeId = table.Column<Guid>(nullable: false),
                    BusinessAgeId = table.Column<Guid>(nullable: false),
                    CompanySizeId = table.Column<Guid>(nullable: false),
                    IndustryExperienceId = table.Column<Guid>(nullable: false),
                    CompanyRegisteredState = table.Column<string>(nullable: true),
                    CompanyFiscalYearStartMonth = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Company_BusinessAge_BusinessAgeId",
                        column: x => x.BusinessAgeId,
                        principalTable: "BusinessAge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Company_CompanySize_CompanySizeId",
                        column: x => x.CompanySizeId,
                        principalTable: "CompanySize",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Company_CompanyStructure_CompanyStructureId",
                        column: x => x.CompanyStructureId,
                        principalTable: "CompanyStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Company_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Company_Entity_Id",
                        column: x => x.Id,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Company_IndustryExperience_IndustryExperienceId",
                        column: x => x.IndustryExperienceId,
                        principalTable: "IndustryExperience",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Company_NAICSIndustryType_NAICSIndustryTypeId",
                        column: x => x.NAICSIndustryTypeId,
                        principalTable: "NAICSIndustryType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Company_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityBankDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    BankId = table.Column<Guid>(nullable: false),
                    AccountNumber = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityBankDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityBankDetail_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityBankDetail_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityBankDetail_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityTaxForm",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    EntityId = table.Column<Guid>(nullable: false),
                    TaxFormId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityTaxForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityTaxForm_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityTaxForm_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityTaxForm_TaxForm_TaxFormId",
                        column: x => x.TaxFormId,
                        principalTable: "TaxForm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityTaxForm_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderBank",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    EntityId = table.Column<Guid>(nullable: false),
                    Entityd = table.Column<Guid>(nullable: true),
                    BankId = table.Column<string>(nullable: false),
                    BankName = table.Column<string>(nullable: false),
                    BankInformationJson = table.Column<string>(type: "jsonb", nullable: true),
                    IntegratedServiceConfigurationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderBank", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderBank_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderBank_Entity_Entityd",
                        column: x => x.Entityd,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderBank_IntegratedServiceConfiguration_IntegratedServi~",
                        column: x => x.IntegratedServiceConfigurationId,
                        principalTable: "IntegratedServiceConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderBank_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanApplication",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    SectionId = table.Column<Guid>(nullable: false),
                    LoanAmount = table.Column<decimal>(nullable: false),
                    LoanPurposeId = table.Column<Guid>(nullable: false),
                    LoanPeriod = table.Column<decimal>(nullable: false),
                    LoanApplicationNumber = table.Column<string>(maxLength: 18, nullable: false),
                    LoanAmountDepositeeBankId = table.Column<Guid>(nullable: true),
                    EMIDeducteeBankId = table.Column<Guid>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: true),
                    EvaluationComments = table.Column<string>(nullable: true),
                    StatusUpdatedByBankUserId = table.Column<Guid>(nullable: true),
                    InterestRate = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanApplication_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanApplication_EntityBankDetail_EMIDeducteeBankId",
                        column: x => x.EMIDeducteeBankId,
                        principalTable: "EntityBankDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanApplication_EntityBankDetail_LoanAmountDepositeeBankId",
                        column: x => x.LoanAmountDepositeeBankId,
                        principalTable: "EntityBankDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanApplication_LoanPurpose_LoanPurposeId",
                        column: x => x.LoanPurposeId,
                        principalTable: "LoanPurpose",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanApplication_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanApplication_Section_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Section",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanApplication_BankUser_StatusUpdatedByBankUserId",
                        column: x => x.StatusUpdatedByBankUserId,
                        principalTable: "BankUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanApplication_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityTaxYearlyMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Period = table.Column<string>(nullable: false),
                    DocumentId = table.Column<Guid>(nullable: false),
                    EntityTaxFormId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityTaxYearlyMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityTaxYearlyMapping_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityTaxYearlyMapping_EntityTaxForm_EntityTaxFormId",
                        column: x => x.EntityTaxFormId,
                        principalTable: "EntityTaxForm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankAccountTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ProviderBankId = table.Column<Guid>(nullable: false),
                    AccountId = table.Column<string>(nullable: false),
                    AccountName = table.Column<string>(nullable: false),
                    CurrentBalance = table.Column<decimal>(nullable: false),
                    AccountType = table.Column<string>(nullable: false),
                    AccountInformationJson = table.Column<string>(type: "jsonb", nullable: true),
                    TransactionInformationJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccountTransaction_ProviderBank_ProviderBankId",
                        column: x => x.ProviderBankId,
                        principalTable: "ProviderBank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EntityId = table.Column<Guid>(nullable: false),
                    IsBankrupted = table.Column<bool>(nullable: false),
                    HasPendingLien = table.Column<bool>(nullable: false),
                    HasPendingJudgment = table.Column<bool>(nullable: false),
                    FsrScore = table.Column<decimal>(nullable: true),
                    CommercialScore = table.Column<decimal>(nullable: true),
                    Response = table.Column<string>(nullable: false),
                    IntegratedServiceConfigurationId = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    LoanApplicationId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditReport_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditReport_IntegratedServiceConfiguration_IntegratedServi~",
                        column: x => x.IntegratedServiceConfigurationId,
                        principalTable: "IntegratedServiceConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditReport_LoanApplication_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityFinance",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    EntityId = table.Column<Guid>(nullable: false),
                    FinancialInformationJson = table.Column<string>(type: "jsonb", nullable: true),
                    FinancialStatementId = table.Column<Guid>(nullable: false),
                    IntegratedServiceConfigurationId = table.Column<Guid>(nullable: false),
                    LoanApplicationId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityFinance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityFinance_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityFinance_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityFinance_FinancialStatement_FinancialStatementId",
                        column: x => x.FinancialStatementId,
                        principalTable: "FinancialStatement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityFinance_IntegratedServiceConfiguration_IntegratedServ~",
                        column: x => x.IntegratedServiceConfigurationId,
                        principalTable: "IntegratedServiceConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityFinance_LoanApplication_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntityFinance_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityLoanApplicationConsent",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    ConsenteeId = table.Column<Guid>(nullable: false),
                    LoanApplicationId = table.Column<Guid>(nullable: false),
                    ConsentId = table.Column<Guid>(nullable: false),
                    IsConsentGiven = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityLoanApplicationConsent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityLoanApplicationConsent_Consent_ConsentId",
                        column: x => x.ConsentId,
                        principalTable: "Consent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityLoanApplicationConsent_Entity_ConsenteeId",
                        column: x => x.ConsenteeId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityLoanApplicationConsent_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityLoanApplicationConsent_LoanApplication_LoanApplicatio~",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityLoanApplicationConsent_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityLoanApplicationMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EntityId = table.Column<Guid>(nullable: false),
                    LoanApplicationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityLoanApplicationMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityLoanApplicationMapping_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityLoanApplicationMapping_LoanApplication_LoanApplicatio~",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoanApplicationSnapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LoanApplicationId = table.Column<Guid>(nullable: false),
                    ApplicationDetailsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplicationSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanApplicationSnapshot_LoanApplication_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntityFinanceYearlyMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Period = table.Column<string>(nullable: false),
                    EntityFinanceId = table.Column<Guid>(nullable: false),
                    UploadedDocumentId = table.Column<Guid>(nullable: true),
                    LastAddedDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityFinanceYearlyMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityFinanceYearlyMapping_EntityFinance_EntityFinanceId",
                        column: x => x.EntityFinanceId,
                        principalTable: "EntityFinance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityFinanceYearlyMapping_Document_UploadedDocumentId",
                        column: x => x.UploadedDocumentId,
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityFinanceStandardAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    ParentId = table.Column<int>(nullable: true),
                    ExpectedValue = table.Column<decimal>(nullable: true),
                    EntityFinancialYearlyMappingId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityFinanceStandardAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityFinanceStandardAccount_EntityFinanceYearlyMapping_Ent~",
                        column: x => x.EntityFinancialYearlyMappingId,
                        principalTable: "EntityFinanceYearlyMapping",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_CreatedByUserId",
                table: "Address",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_IntegratedServiceConfigurationId",
                table: "Address",
                column: "IntegratedServiceConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_UpdatedByUserId",
                table: "Address",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bank_SWIFTCode",
                table: "Bank",
                column: "SWIFTCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountTransaction_ProviderBankId",
                table: "BankAccountTransaction",
                column: "ProviderBankId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAge_Age",
                table: "BusinessAge",
                column: "Age",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAge_Order",
                table: "BusinessAge",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_BusinessAgeId",
                table: "Company",
                column: "BusinessAgeId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_CIN",
                table: "Company",
                column: "CIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_CompanySizeId",
                table: "Company",
                column: "CompanySizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_CompanyStructureId",
                table: "Company",
                column: "CompanyStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_CreatedByUserId",
                table: "Company",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_IndustryExperienceId",
                table: "Company",
                column: "IndustryExperienceId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_NAICSIndustryTypeId",
                table: "Company",
                column: "NAICSIndustryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_UpdatedByUserId",
                table: "Company",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySize_Order",
                table: "CompanySize",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySize_Size",
                table: "CompanySize",
                column: "Size",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyStructure_Order",
                table: "CompanyStructure",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyStructure_Structure",
                table: "CompanyStructure",
                column: "Structure",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditReport_EntityId",
                table: "CreditReport",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditReport_IntegratedServiceConfigurationId",
                table: "CreditReport",
                column: "IntegratedServiceConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditReport_LoanApplicationId",
                table: "CreditReport",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Entity_AddressId",
                table: "Entity",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityBankDetail_BankId",
                table: "EntityBankDetail",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityBankDetail_CreatedByUserId",
                table: "EntityBankDetail",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityBankDetail_UpdatedByUserId",
                table: "EntityBankDetail",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinance_CreatedByUserId",
                table: "EntityFinance",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinance_EntityId",
                table: "EntityFinance",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinance_FinancialStatementId",
                table: "EntityFinance",
                column: "FinancialStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinance_IntegratedServiceConfigurationId",
                table: "EntityFinance",
                column: "IntegratedServiceConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinance_LoanApplicationId",
                table: "EntityFinance",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinance_UpdatedByUserId",
                table: "EntityFinance",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinanceStandardAccount_EntityFinancialYearlyMappingId",
                table: "EntityFinanceStandardAccount",
                column: "EntityFinancialYearlyMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinanceYearlyMapping_EntityFinanceId",
                table: "EntityFinanceYearlyMapping",
                column: "EntityFinanceId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityFinanceYearlyMapping_UploadedDocumentId",
                table: "EntityFinanceYearlyMapping",
                column: "UploadedDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLoanApplicationConsent_ConsentId",
                table: "EntityLoanApplicationConsent",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLoanApplicationConsent_ConsenteeId",
                table: "EntityLoanApplicationConsent",
                column: "ConsenteeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLoanApplicationConsent_CreatedByUserId",
                table: "EntityLoanApplicationConsent",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLoanApplicationConsent_LoanApplicationId",
                table: "EntityLoanApplicationConsent",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLoanApplicationConsent_UpdatedByUserId",
                table: "EntityLoanApplicationConsent",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLoanApplicationMapping_EntityId",
                table: "EntityLoanApplicationMapping",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLoanApplicationMapping_LoanApplicationId",
                table: "EntityLoanApplicationMapping",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityRelationshipMapping_PrimaryEntityId",
                table: "EntityRelationshipMapping",
                column: "PrimaryEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityRelationshipMapping_RelationshipId",
                table: "EntityRelationshipMapping",
                column: "RelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityRelationshipMapping_RelativeEntityId",
                table: "EntityRelationshipMapping",
                column: "RelativeEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTaxForm_CreatedByUserId",
                table: "EntityTaxForm",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTaxForm_EntityId",
                table: "EntityTaxForm",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTaxForm_TaxFormId",
                table: "EntityTaxForm",
                column: "TaxFormId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTaxForm_UpdatedByUserId",
                table: "EntityTaxForm",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTaxYearlyMapping_DocumentId",
                table: "EntityTaxYearlyMapping",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTaxYearlyMapping_EntityTaxFormId",
                table: "EntityTaxYearlyMapping",
                column: "EntityTaxFormId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatement_Name",
                table: "FinancialStatement",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndustryExperience_Experience",
                table: "IndustryExperience",
                column: "Experience",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndustryExperience_Order",
                table: "IndustryExperience",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegratedServiceConfiguration_Name",
                table: "IntegratedServiceConfiguration",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_CreatedByUserId",
                table: "LoanApplication",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_EMIDeducteeBankId",
                table: "LoanApplication",
                column: "EMIDeducteeBankId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_LoanAmountDepositeeBankId",
                table: "LoanApplication",
                column: "LoanAmountDepositeeBankId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_LoanApplicationNumber",
                table: "LoanApplication",
                column: "LoanApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_LoanPurposeId",
                table: "LoanApplication",
                column: "LoanPurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_ProductId",
                table: "LoanApplication",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_SectionId",
                table: "LoanApplication",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_StatusUpdatedByBankUserId",
                table: "LoanApplication",
                column: "StatusUpdatedByBankUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplication_UpdatedByUserId",
                table: "LoanApplication",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplicationSnapshot_LoanApplicationId",
                table: "LoanApplicationSnapshot",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPurpose_LoanTypeId",
                table: "LoanPurpose",
                column: "LoanTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPurpose_Name",
                table: "LoanPurpose",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanPurpose_Order",
                table: "LoanPurpose",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanPurposeRangeTypeMapping_LoanPurposeId",
                table: "LoanPurposeRangeTypeMapping",
                column: "LoanPurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPurposeRangeTypeMapping_LoanRangeTypeId",
                table: "LoanPurposeRangeTypeMapping",
                column: "LoanRangeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NAICSIndustryType_NAICSParentSectorId",
                table: "NAICSIndustryType",
                column: "NAICSParentSectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Name",
                table: "Product",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductPurposeMapping_LoanPurposeId",
                table: "ProductPurposeMapping",
                column: "LoanPurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPurposeMapping_ProductId",
                table: "ProductPurposeMapping",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRangeTypeMapping_ProductId",
                table: "ProductRangeTypeMapping",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRangeTypeMapping_RangeTypeId",
                table: "ProductRangeTypeMapping",
                column: "RangeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypeMapping_LoanTypeId",
                table: "ProductTypeMapping",
                column: "LoanTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypeMapping_ProductId",
                table: "ProductTypeMapping",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderBank_CreatedByUserId",
                table: "ProviderBank",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderBank_Entityd",
                table: "ProviderBank",
                column: "Entityd");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderBank_IntegratedServiceConfigurationId",
                table: "ProviderBank",
                column: "IntegratedServiceConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderBank_UpdatedByUserId",
                table: "ProviderBank",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationship_Relation",
                table: "Relationship",
                column: "Relation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxFormCompanyStructureMapping_CompanyStructureId",
                table: "TaxFormCompanyStructureMapping",
                column: "CompanyStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxFormCompanyStructureMapping_TaxFormId",
                table: "TaxFormCompanyStructureMapping",
                column: "TaxFormId");

            migrationBuilder.CreateIndex(
                name: "IX_User_CreatedByUserId",
                table: "User",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Phone",
                table: "User",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_SSN",
                table: "User",
                column: "SSN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_UpdatedByUserId",
                table: "User",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entity_Address_AddressId",
                table: "Entity",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Address_User_CreatedByUserId",
                table: "Address");

            migrationBuilder.DropForeignKey(
                name: "FK_Address_User_UpdatedByUserId",
                table: "Address");

            migrationBuilder.DropTable(
                name: "BankAccountTransaction");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "CreditReport");

            migrationBuilder.DropTable(
                name: "EntityFinanceStandardAccount");

            migrationBuilder.DropTable(
                name: "EntityLoanApplicationConsent");

            migrationBuilder.DropTable(
                name: "EntityLoanApplicationMapping");

            migrationBuilder.DropTable(
                name: "EntityRelationshipMapping");

            migrationBuilder.DropTable(
                name: "EntityTaxYearlyMapping");

            migrationBuilder.DropTable(
                name: "LoanApplicationSnapshot");

            migrationBuilder.DropTable(
                name: "LoanPurposeRangeTypeMapping");

            migrationBuilder.DropTable(
                name: "ProductPurposeMapping");

            migrationBuilder.DropTable(
                name: "ProductRangeTypeMapping");

            migrationBuilder.DropTable(
                name: "ProductTypeMapping");

            migrationBuilder.DropTable(
                name: "TaxFormCompanyStructureMapping");

            migrationBuilder.DropTable(
                name: "ProviderBank");

            migrationBuilder.DropTable(
                name: "BusinessAge");

            migrationBuilder.DropTable(
                name: "CompanySize");

            migrationBuilder.DropTable(
                name: "IndustryExperience");

            migrationBuilder.DropTable(
                name: "NAICSIndustryType");

            migrationBuilder.DropTable(
                name: "EntityFinanceYearlyMapping");

            migrationBuilder.DropTable(
                name: "Consent");

            migrationBuilder.DropTable(
                name: "Relationship");

            migrationBuilder.DropTable(
                name: "EntityTaxForm");

            migrationBuilder.DropTable(
                name: "LoanRangeType");

            migrationBuilder.DropTable(
                name: "CompanyStructure");

            migrationBuilder.DropTable(
                name: "EntityFinance");

            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "TaxForm");

            migrationBuilder.DropTable(
                name: "FinancialStatement");

            migrationBuilder.DropTable(
                name: "LoanApplication");

            migrationBuilder.DropTable(
                name: "EntityBankDetail");

            migrationBuilder.DropTable(
                name: "LoanPurpose");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Section");

            migrationBuilder.DropTable(
                name: "BankUser");

            migrationBuilder.DropTable(
                name: "Bank");

            migrationBuilder.DropTable(
                name: "LoanType");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Entity");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "IntegratedServiceConfiguration");
        }
    }
}
