using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.Repository.Seed;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.Product;
using LendingPlatform.Utils.ApplicationClass.TaxForm;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using LoanPurposeSeedAC = LendingPlatform.Utils.ApplicationClass.Product.LoanPurposeSeedAC;

namespace LendingPlatform.Repository.Test.Seed
{
    [Collection("Register Dependency")]
    public class SeedDatabaseTest : BaseTest
    {
        #region Private variables
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly SeedDatabase _seedDatabase;
        private readonly Mock<IFileOperationsUtility> _fileOperationsUtility;
        private readonly Mock<IConfiguration> _configuration;
        #endregion

        #region Constructor
        public SeedDatabaseTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _seedDatabase = bootstrap.ServiceProvider.GetService<SeedDatabase>();
            _fileOperationsUtility = bootstrap.ServiceProvider.GetService<Mock<IFileOperationsUtility>>();
            _configuration = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _dataRepositoryMock.Reset();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedCompanyStructureAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<CompanyStructure>(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<CompanyStructure>(It.IsAny<List<CompanyStructure>>()), Times.Exactly(1));
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedIndustryExperienceAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<IndustryExperience>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<IndustryExperience>(It.IsAny<List<IndustryExperience>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedBusinessAgeAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<BusinessAge>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<BusinessAge>(It.IsAny<List<BusinessAge>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedEmployeeStrenghtAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<CompanySize>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<CompanySize>(It.IsAny<List<CompanySize>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedSICIndustryTypeAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<NAICSIndustryType>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<NAICSIndustryType>(It.IsAny<List<NAICSIndustryType>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedIntegratedServiceConfigurationAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<IntegratedServiceConfiguration>(), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<IntegratedServiceConfiguration>(It.IsAny<List<IntegratedServiceConfiguration>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedRelationshipAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<Relationship>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<Relationship>(It.IsAny<List<Relationship>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedCFinancialStatementAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<FinancialStatement>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<FinancialStatement>(It.IsAny<List<FinancialStatement>>()), Times.Once);


        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedLoanRangeTypeAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            //Assert
            _dataRepositoryMock.Verify(x => x.GetAll<LoanRangeType>(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<LoanRangeType>(It.IsAny<List<LoanRangeType>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedLoanTypeAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            //Assert
            _dataRepositoryMock.Verify(x => x.GetAll<LoanType>(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<LoanType>(It.IsAny<List<LoanType>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedLoanProductAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            //Assert
            _dataRepositoryMock.Verify(x => x.GetAll<Product>(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<Product>(It.IsAny<List<Product>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedLoanPurposeAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            //Assert
            _dataRepositoryMock.Verify(x => x.GetAll<LoanPurpose>(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.GetAll<LoanType>(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<LoanPurpose>(It.IsAny<List<LoanPurpose>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedLoanProductTypeMappingAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            //Assert
            _dataRepositoryMock.Verify(x => x.GetAll<ProductTypeMapping>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.GetAll<LoanType>(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.GetAll<Product>(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<ProductTypeMapping>(It.IsAny<List<ProductTypeMapping>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedLoanProductRangeTypeMappingAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            //Assert
            _dataRepositoryMock.Verify(x => x.GetAll<LoanRangeType>(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.GetAll<Product>(), Times.Exactly(2));
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedLoanProductPurposeTypeMappingAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            //Assert
            _dataRepositoryMock.Verify(x => x.GetAll<LoanPurpose>(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.GetAll<Product>(), Times.Exactly(2));
        }

        /// <summary>
        /// Method to verify that add operation is performed in consent table in database. 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedConsentsAsync_PerformsAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<Consent>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Consent>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify that add operation is performed in consent table in database. 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedSectionsAsync_PerformsAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<Section>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Section>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify add operation is performed in tax form table in database
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedTaxFormsAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<TaxForm>(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<TaxForm>>()), Times.Exactly(1));
        }

        /// <summary>
        /// Method is to verify add operation is performed in tax form company type mapping table in database
        /// </summary>
        [Fact]
        public async Task SeedAsync_SeedTaxFormCompanyStrctureMappingAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<TaxFormCompanyStructureMapping>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.GetAll<CompanyStructure>(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.GetAll<TaxForm>(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<TaxFormCompanyStructureMapping>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedLoanPurposeRangeTypeMappingAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            //Assert
            _dataRepositoryMock.Verify(x => x.GetAll<LoanPurposeRangeTypeMapping>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.GetAll<LoanRangeType>(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.GetAll<LoanPurpose>(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<LoanPurposeRangeTypeMapping>(It.IsAny<List<LoanPurposeRangeTypeMapping>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedTaxformLabelNameMapping_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<TaxFormLabelNameMapping>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.SingleAsync(It.IsAny<Expression<Func<TaxForm, bool>>>()), Times.Once());
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<TaxFormLabelNameMapping>(It.IsAny<List<TaxFormLabelNameMapping>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedOCRModelMappingAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<OCRModelMapping>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.GetAll<CompanyStructure>(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<OCRModelMapping>(It.IsAny<List<OCRModelMapping>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedPersonalFinanceStaticDataAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<PersonalFinanceConstant>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.GetAll<PersonalFinanceAccount>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.GetAll<PersonalFinanceCategory>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.GetAll<PersonalFinanceAttribute>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.GetAll<PersonalFinanceAttributeCategoryMapping>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.GetAll<PersonalFinanceParentChildCategoryMapping>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<PersonalFinanceConstant>(It.IsAny<List<PersonalFinanceConstant>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<PersonalFinanceAccount>(It.IsAny<List<PersonalFinanceAccount>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<PersonalFinanceCategory>(It.IsAny<List<PersonalFinanceCategory>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<PersonalFinanceAttribute>(It.IsAny<List<PersonalFinanceAttribute>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<PersonalFinanceAttributeCategoryMapping>(It.IsAny<List<PersonalFinanceAttributeCategoryMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<PersonalFinanceParentChildCategoryMapping>(It.IsAny<List<PersonalFinanceParentChildCategoryMapping>>()), Times.Once);
        }

        /// <summary>
        /// Method to verify db calls
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SeedAsync_SeedAdditionalDocumentTypeAsync_PerformAddOperation()
        {
            // Arrange
            Arrange();

            // Act
            await _seedDatabase.SeedAsync();

            // Assert
            _dataRepositoryMock.Verify(x => x.GetAll<AdditionalDocumentType>(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<AdditionalDocumentType>(It.IsAny<List<AdditionalDocumentType>>()), Times.Once);
        }
        #endregion

        #region Private Method
        private void Arrange()
        {
            // SeedSICIndustryTypeAsync
            var existingSICIndustryTypeList = new List<NAICSIndustryType>();
            var industryTypeAC = new IndustryTypeSeedAC()
            {
                IndustryTypes = new List<NAICSIndustryTypeAC>()
                {
                    new NAICSIndustryTypeAC
                    {
                        IndustryType = "SEC01",
                        IndustryCode = "SE01"
                    }
                }
            };

            _dataRepositoryMock.Setup(x => x.GetAll<NAICSIndustryType>())
               .Returns(existingSICIndustryTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingSICIndustryTypeList)).Verifiable();

            // SeedIntegratedServiceConfigurationAsync
            var existingIntegratedServiceConfigurationList = new List<IntegratedServiceConfiguration>();
            var integratedServiceConfigurationAC = new List<IntegratedServiceConfigurationSeedAC>()
            {
                new IntegratedServiceConfigurationSeedAC
                {
                    Name = "SmartyStreets",
                    IsServiceEnabled = true
                }
            };

            _dataRepositoryMock.Setup(x => x.GetAll<IntegratedServiceConfiguration>())
               .Returns(existingIntegratedServiceConfigurationList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingSICIndustryTypeList)).Verifiable();

            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns(@"{ }");

            // SeedBankAsync
            var bankList = new List<Bank>()
            {
                new Bank()
                {
                    Name = "SEC01",
                    SWIFTCode = "SE01"
                }
            };

            // SeedSectionsAsync
            var sectionList = new List<Section>
            {
                new Section
                {
                    Name = "Loan Needs",
                    Order = 1,
                    IsEnabled = true
                },
                new Section
                {
                    Name = "Company Info",
                    Order = 2,
                    IsEnabled = false
                }
            };

            // LoanProductData File
            var loanProductData = new ProductDataAC()
            {
                LoanRangeTypes = new List<LoanRangeTypeSeedAC>()
                {
                    new LoanRangeTypeSeedAC
                    {
                        Id = 1,
                        Name = "XYZ"
                    }
                },
                LoanTypes = new List<LoanTypeSeedAC>
                {
                    new LoanTypeSeedAC
                    {
                        Id = 1,
                        TypeName = "Xyz"
                    }
                },
                Products = new List<ProductsSeedAC>
                {
                    new ProductsSeedAC
                    {
                        Id = 1,
                        Name = "Regularly scheduled Term loan",
                        Description= "Business owners who want to make investments in specific business areas or have an ongoing need for working capital.",
                        StartDateAvailability= Convert.ToDateTime("1/1/20"),
                        EndDateAvailibility= Convert.ToDateTime("1/1/22")
                    }
                },
                LoanPurposes = new List<LoanPurposeSeedAC>
                {
                    new LoanPurposeSeedAC
                    {
                        Id = 1,
                        Order = 1,
                        PurposeName = "Xyz",
                        TypeId = 1,
                        IsEnabled = true
                    }
                },
                LoanPurposeRangeTypeMappings = new List<LoanPurposeRangeTypeMappingSeedAC>
                {
                    new LoanPurposeRangeTypeMappingSeedAC
                    {
                        LoanPurposeId = 1,
                        RangeTypeId = 1,
                        Minimum = 10000,
                        Maximum = 20000,
                        StepperAmount = 1000
                    }
                },
                ProductTypeMappings = new List<ProductTypeMappingSeedAC>
                {
                    new ProductTypeMappingSeedAC
                    {
                        ProductId = 1,
                        TypeId = 1
                    }
                },
                ProductRangeTypeMappings = new List<ProductRangeTypeMappingSeedAC>
                {
                    new ProductRangeTypeMappingSeedAC
                    {
                        ProductId = 1,
                        RangeTypeId = 1,
                        Minimum = 10000,
                        Maximum = 20000
                    }
                },
                ProductSubPurposeMappings = new List<ProductSubPurposeMappingSeedAC>
                {
                    new ProductSubPurposeMappingSeedAC
                    {
                        ProductId = 1,
                        SubPurposeId = 1
                    }
                },
                SubLoanPurposes = new List<SubLoanPurposeAC>()
                {
                    new SubLoanPurposeAC
                    {
                        Id = 1,
                        Name = "xyz",
                        IsEnabled = true,
                        LoanPurposeId = 1,
                        Order = 1
                    }
                }
            };

            // Consents json file
            var consentData = new List<Consent>
            {
                new Consent
                {
                    ConsentText = "I Allow You To Give This Sample Consent"
                }
            };

            // Tax Form json file
            var taxFormData = new TaxFormDataAC()
            {
                TaxForms = new List<TaxFormSeedAC>()
                {
                    new TaxFormSeedAC()
                    {
                        Id = 1,
                        Name = "Form 1040"
                    }
                },
                CompanyStructures = new List<CompanyStructureSeedAC>()
                {
                    new CompanyStructureSeedAC()
                    {
                        Id=1,
                        Structure = StringConstant.Proprietor,
                        Order = 1
                    }
                },
                TaxFormCompanyStructureMappings = new List<TaxFormCompanyStructureMappingAC>()
                {
                    new TaxFormCompanyStructureMappingAC()
                    {
                        CompanyStructureId = 1,
                        TaxFormId = 1,
                        IsSoleProprietors = true
                    }
                },
                Fields = new List<TaxFormLabelNameSeedAC>()
                {
                    new TaxFormLabelNameSeedAC()
                    {
                      FieldKey = "1_Filing Status - Single",
                      FieldType =  "selectionMark",
                      FieldFormat = "not-specified"
                    }
                }
            };

            // OCR Model json file
            var ocrModelMapping = new List<OCRModelMapping>()
            {
                new OCRModelMapping()
                {
                    ModelId = "XYZ",
                    Year = "2019",
                    IsEnabled = true
                },
                new OCRModelMapping()
                {
                    ModelId = "ABC",
                    CompanyStructureId = Guid.NewGuid(),
                    IsEnabled = true
                }
            };

            //Personal Finance JSON file content
            var personalFinanceData = new PersonalFinanceSeedDataAC
            {
                PersonalFinanceConstants = new List<PersonalFinanceConstantSeedDataAC>
                {
                    new PersonalFinanceConstantSeedDataAC
                    {
                        Id = 1,
                        Name = "Account1",
                        Options = new List<PersonalFinanceConstantOptionSeedDataAC>
                        {
                            new PersonalFinanceConstantOptionSeedDataAC
                            {
                                Id = 1,
                                IsEnabled = true,
                                Order = 1,
                                Value = "Option1"
                            },
                            new PersonalFinanceConstantOptionSeedDataAC
                            {
                                Id = 2,
                                IsEnabled = true,
                                Order = 2,
                                Value = "Option2"
                            }
                        }
                    }
                },
                PersonalFinanceAccounts = new List<PersonalFinanceAccountSeedDataAC>
                {
                    new PersonalFinanceAccountSeedDataAC
                    {
                        Id = 1,
                        Name = "Attribute1",
                        IsEnabled = true,
                        Order = 1
                    }
                },
                PersonalFinanceCategories = new List<PersonalFinanceCategorySeedDataAC>
                {
                    new PersonalFinanceCategorySeedDataAC
                    {
                        Id = 1,
                        IsEnabled = true,
                        Name = "Category1",
                        Order = 1,
                        PersonalFinanceAccountId = 1
                    },
                    new PersonalFinanceCategorySeedDataAC
                    {
                        Id = 2,
                        IsEnabled = true,
                        Name = "Category2",
                        Order = 2,
                        PersonalFinanceAccountId = 1
                    }
                },
                PersonalFinanceAttributes = new List<PersonalFinanceAttributeSeedDataAC>
                {
                    new PersonalFinanceAttributeSeedDataAC
                    {
                        Id = 1,
                        Text = "Attribute1",
                        IsEnabled = true,
                        FieldType = "Number"
                    },
                    new PersonalFinanceAttributeSeedDataAC
                    {
                        Id = 2,
                        Text = "Attribute2",
                        IsEnabled = true,
                        FieldType = "Number"
                    },
                    new PersonalFinanceAttributeSeedDataAC
                    {
                        Id = 3,
                        Text = "Attribute3",
                        IsEnabled = true,
                        FieldType = "Dropdown"
                    }
                },
                PersonalFinanceAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMappingSeedDataAC>
                {
                    new PersonalFinanceAttributeCategoryMappingSeedDataAC
                    {
                        Id = 1,
                        CategoryId = 1,
                        AttributeId = 1,
                        IsCurrent = false,
                        IsOriginal = false,
                        Order = 1,
                        ParentAttributeCategoryMappingId = null,
                        PersonalFinanceConstantId = null
                    },
                    new PersonalFinanceAttributeCategoryMappingSeedDataAC
                    {
                        Id = 2,
                        CategoryId = 1,
                        AttributeId = 2,
                        IsCurrent = true,
                        IsOriginal = true,
                        Order = 2,
                        ParentAttributeCategoryMappingId = 1,
                        PersonalFinanceConstantId = null
                    },
                    new PersonalFinanceAttributeCategoryMappingSeedDataAC
                    {
                        Id = 3,
                        CategoryId = 2,
                        AttributeId = 1,
                        IsCurrent = false,
                        IsOriginal = false,
                        Order = 1,
                        ParentAttributeCategoryMappingId = null,
                        PersonalFinanceConstantId = null
                    },
                    new PersonalFinanceAttributeCategoryMappingSeedDataAC
                    {
                        Id = 4,
                        CategoryId = 2,
                        AttributeId = 3,
                        IsCurrent = true,
                        IsOriginal = true,
                        Order = 2,
                        ParentAttributeCategoryMappingId = 3,
                        PersonalFinanceConstantId = 1
                    }
                },
                PersonalFinanceParentChildCategoryMappings = new List<PersonalFinanceParentChildCategoryMappingSeedDataAC>
                {
                    new PersonalFinanceParentChildCategoryMappingSeedDataAC
                    {
                        Id = 1,
                        ParentCategoryId = 1,
                        ChildCategoryId = 2,
                        ParentAttributeCategoryMappingId = null
                    }
                }
            };

            //Additional documents' types JSON file content
            var documentTypes = new List<AdditionalDocumentTypeSeedAC>
            {
                new AdditionalDocumentTypeSeedAC
                {
                    Id = 1,
                    Type = "Certificate",
                    DocumentTypeFor = "Company",
                    IsEnabled = true
                }
            };

            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
            .Returns(JsonConvert.SerializeObject(taxFormData))
            .Returns(JsonConvert.SerializeObject(industryTypeAC))
            .Returns(JsonConvert.SerializeObject(integratedServiceConfigurationAC))
            .Returns(JsonConvert.SerializeObject(loanProductData))
            .Returns(JsonConvert.SerializeObject(loanProductData))
            .Returns(JsonConvert.SerializeObject(loanProductData))
            .Returns(JsonConvert.SerializeObject(loanProductData))
            .Returns(JsonConvert.SerializeObject(loanProductData))
            .Returns(JsonConvert.SerializeObject(consentData))
            .Returns(JsonConvert.SerializeObject(bankList))
            .Returns(JsonConvert.SerializeObject(sectionList))
            .Returns(JsonConvert.SerializeObject(taxFormData))
            .Returns(JsonConvert.SerializeObject(taxFormData))
            .Returns(JsonConvert.SerializeObject(loanProductData))
            .Returns(JsonConvert.SerializeObject(taxFormData))
            .Returns(JsonConvert.SerializeObject(ocrModelMapping))
            .Returns(JsonConvert.SerializeObject(personalFinanceData))
            .Returns(JsonConvert.SerializeObject(documentTypes));

            // SeedCompanyStructureAsync
            var existingCompanyTypeList = new List<CompanyStructure>();
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
               .Returns(existingCompanyTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingCompanyTypeList)).Verifiable();

            // SeedIndustryExprienceAsync
            var existingIndustryExprienceList = new List<IndustryExperience>();
            _dataRepositoryMock.Setup(x => x.GetAll<IndustryExperience>())
               .Returns(existingIndustryExprienceList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingIndustryExprienceList)).Verifiable();

            // SeedBusinessAgeAsync
            var existingBusinessAge = new List<BusinessAge>();
            _dataRepositoryMock.Setup(x => x.GetAll<BusinessAge>())
                .Returns(existingBusinessAge.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingBusinessAge)).Verifiable();

            // SeedEmployeeStrengthAsync
            var existingStrengthList = new List<CompanySize>();
            _dataRepositoryMock.Setup(x => x.GetAll<CompanySize>())
                .Returns(existingStrengthList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingStrengthList)).Verifiable();

            // SeedLoanRangeTypeAsync()
            var existingLoanRangeTypeList = new List<LoanRangeType>();
            _dataRepositoryMock.Setup(x => x.GetAll<LoanRangeType>()).Returns(existingLoanRangeTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingLoanRangeTypeList)).Verifiable();

            // SeedLoanTypeAsync()
            var existingLoanTypeList = new List<LoanType>();
            _dataRepositoryMock.Setup(x => x.GetAll<LoanType>()).Returns(existingLoanTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingLoanTypeList)).Verifiable();

            // SeedLoanProductAsync()
            var existingLoanProductList = new List<Product>();
            _dataRepositoryMock.Setup(x => x.GetAll<Product>()).Returns(existingLoanProductList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingLoanProductList)).Verifiable();

            // SeedLoanPurposeTypeAsync()
            var existingLoanPurposeList = new List<LoanPurpose>();
            var existingSubLoanPurposeList = new List<SubLoanPurpose>();
            var loanApplications = new List<LoanApplication>();
            _dataRepositoryMock.Setup(x => x.GetAll<LoanPurpose>()).Returns(existingLoanPurposeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<SubLoanPurpose>()).Returns(existingSubLoanPurposeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AnyAsync<UserLoanSectionMapping>()).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingLoanPurposeList)).Verifiable();

            // SeedLoanProductTypeMappingAsync
            var existingLoanProductTypeMappingList = new List<ProductTypeMapping>();
            _dataRepositoryMock.Setup(x => x.GetAll<ProductTypeMapping>()).Returns(existingLoanProductTypeMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingLoanProductTypeMappingList));

            // SeedLoanProductRangeTypeMappingAsync 
            var existingLoanProductRangeTypeMappingList = new List<ProductRangeTypeMapping>();
            _dataRepositoryMock.Setup(x => x.GetAll<ProductRangeTypeMapping>()).Returns(existingLoanProductRangeTypeMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingLoanProductRangeTypeMappingList));

            // SeedLoanProductPurposeTypeMappingAsync
            var existingLoanProductPurposeMappingList = new List<ProductSubPurposeMapping>();
            _dataRepositoryMock.Setup(x => x.GetAll<ProductSubPurposeMapping>()).Returns(existingLoanProductPurposeMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingLoanProductPurposeMappingList));

            // SeedRelationshipAsync
            var existingRelationsList = new List<Relationship>();
            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(existingRelationsList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingRelationsList)).Verifiable();

            // SeedFinancialStatementAsync //SeedFinancialAccountTypeAsync
            var existingFinancialStatementsList = new List<FinancialStatement>();
            _dataRepositoryMock.Setup(x => x.GetAll<FinancialStatement>())
                 .Returns(existingFinancialStatementsList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingFinancialStatementsList)).Verifiable();

            // SeedConsentsAsync
            var existingConsents = new List<Consent>();
            _dataRepositoryMock.Setup(x => x.GetAll<Consent>()).Returns(existingConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingConsents)).Verifiable();

            // SeedBankAsync
            var existingBankList = new List<Bank>();
            _dataRepositoryMock.Setup(x => x.GetAll<Bank>()).Returns(existingBankList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingBankList)).Verifiable();

            // SeedSectionsAsync
            var existingSectionList = new List<Section>();
            _dataRepositoryMock.Setup(x => x.GetAll<Section>()).Returns(existingSectionList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingSectionList)).Verifiable();

            // SeedTaxFormAsync
            var existingTaxFormList = new List<TaxForm>();
            _dataRepositoryMock.Setup(x => x.GetAll<TaxForm>()).Returns(existingTaxFormList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingTaxFormList)).Verifiable();

            // SeedTaxFormCompanyTypeMappingAsync
            var existingTaxFormCompanyStructureMapping = new List<TaxFormCompanyStructureMapping>();
            _dataRepositoryMock.Setup(x => x.GetAll<TaxFormCompanyStructureMapping>()).Returns(existingTaxFormCompanyStructureMapping.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingTaxFormCompanyStructureMapping)).Verifiable();

            // SeedLoanPurposeRangeTypeMappingAsync
            var existingLoanPurposeRangeTypeMappingList = new List<LoanPurposeRangeTypeMapping>();

            _dataRepositoryMock.Setup(x => x.GetAll<LoanPurposeRangeTypeMapping>()).Returns(existingLoanPurposeRangeTypeMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingLoanPurposeRangeTypeMappingList)).Verifiable();

            // SeedTaxformLabelNameMappingAsync
            var existingTaxFormLabelNameMappings = new List<TaxFormLabelNameMapping>();
            var taxForm = new TaxForm()
            {
                Id = Guid.NewGuid(),
                Name = StringConstant.TaxReturns
            };
            _dataRepositoryMock.Setup(x => x.GetAll<TaxFormLabelNameMapping>()).Returns(existingTaxFormLabelNameMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<TaxForm, bool>>>())).ReturnsAsync(taxForm);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingTaxFormLabelNameMappings)).Verifiable();

            // SeedOCRModelMappingAsync
            var existingOCRModelMappings = new List<OCRModelMapping>();
            _dataRepositoryMock.Setup(x => x.GetAll<OCRModelMapping>()).Returns(existingOCRModelMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.AddRangeAsync(existingOCRModelMappings));

            //SeedPersonalFinanceStaticDataAsync
            var existingConstants = new List<PersonalFinanceConstant>();
            var existingAccounts = new List<PersonalFinanceAccount>();
            var existingCategories = new List<PersonalFinanceCategory>();
            var existingAttributes = new List<PersonalFinanceAttribute>();
            var existingAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>();
            var existingCategoryMappings = new List<PersonalFinanceParentChildCategoryMapping>();

            _dataRepositoryMock.Setup(x => x.GetAll<PersonalFinanceConstant>()).Returns(existingConstants.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<PersonalFinanceAccount>()).Returns(existingAccounts.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<PersonalFinanceCategory>()).Returns(existingCategories.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<PersonalFinanceAttribute>()).Returns(existingAttributes.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<PersonalFinanceAttributeCategoryMapping>()).Returns(existingAttributeCategoryMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<PersonalFinanceParentChildCategoryMapping>()).Returns(existingCategoryMappings.AsQueryable().BuildMock().Object);

            //SeedAdditionalDocumentTypeAsync
            var existingDocumentTypes = new List<AdditionalDocumentType>();
            _dataRepositoryMock.Setup(x => x.GetAll<AdditionalDocumentType>()).Returns(existingDocumentTypes.AsQueryable().BuildMock().Object);
        }
        #endregion
    }
}
