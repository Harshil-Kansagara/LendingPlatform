using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Products;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.Application;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass.Product;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using LendingPlatform.Utils.Utils.OCR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace LendingPlatform.Repository.Test.Application
{
    [Collection("Register Dependency")]
    public class ApplicationRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly IMapper _mapper;
        private readonly IApplicationRepository _applicationRepository;
        private readonly CurrentUserAC _currentUserAC;
        private readonly Mock<IRulesUtility> _rulesUtilityMock;
        private readonly Mock<IAmazonServicesUtility> _amazonServicesUtilityMock;
        private readonly Mock<IOCRUtility> _ocrUtilityMock;
        #endregion

        #region Constructor
        public ApplicationRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _configurationMock = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _applicationRepository = _scope.ServiceProvider.GetService<IApplicationRepository>();
            _rulesUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IRulesUtility>>();
            _amazonServicesUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IAmazonServicesUtility>>();
            _ocrUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IOCRUtility>>();
            _mapper = _scope.ServiceProvider.GetService<IMapper>();
            _rulesUtilityMock.Reset();
            _dataRepositoryMock.Reset();
            _amazonServicesUtilityMock.Reset();
            _ocrUtilityMock.Reset();
            _currentUserAC = FetchLoggedInUserAC();
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("75.02");
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Get Company Structure List.
        /// </summary>
        /// <returns></returns>
        private List<CompanyStructure> GetCompanyStructureList()
        {
            return new List<CompanyStructure>()
            {
                new CompanyStructure
                {
                    Id = Guid.NewGuid(),
                    Structure = StringConstant.Proprietorship,
                    Order = 1
                },
                new CompanyStructure
                {
                    Id = Guid.NewGuid(),
                    Structure = StringConstant.Partnership,
                    Order = 2
                },
                new CompanyStructure
                {
                    Id = Guid.NewGuid(),
                    Structure = StringConstant.LimitedLiabilityCompany,
                    Order = 1
                },
                new CompanyStructure
                {
                    Id = Guid.NewGuid(),
                    Structure = StringConstant.SCorporation,
                    Order = 1
                },
                new CompanyStructure
                {
                    Id = Guid.NewGuid(),
                    Structure = StringConstant.CCorporation,
                    Order = 1
                }
            };
        }

        /// <summary>
        /// Get List of EntityRelationshipMapping object.
        /// </summary>
        /// <returns></returns>
        private List<EntityRelationshipMapping> GetEntityRelationshipMappings()
        {
            return new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    PrimaryEntity = new DomainModel.Models.EntityInfo.Entity
                    {
                        Id = Guid.NewGuid(),
                        Type = EntityType.Company,
                        Company = new Company
                        {
                            Id = Guid.NewGuid(),
                            IndustryExperienceId = Guid.NewGuid(),
                            NAICSIndustryTypeId = Guid.NewGuid(),
                            BusinessAgeId = Guid.NewGuid(),
                            CIN = "456123789",
                            CompanyFiscalYearStartMonth = 1,
                            CompanyRegisteredState = "Texas",
                            CompanySizeId = Guid.NewGuid(),
                            CompanyStructureId = Guid.NewGuid(),
                            Name = "sd",
                            CreatedByUserId = Guid.NewGuid()
                        }
                    },
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 90,
                    RelationshipId = Guid.NewGuid()
                }
            };
        }

        /// <summary>
        /// Fetch the logged in user details (CurrentUserAC).
        /// </summary>
        /// <returns></returns>
        private CurrentUserAC FetchLoggedInUserAC()
        {
            return new CurrentUserAC()
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@doe.com",
                IsBankUser = false
            };
        }

        /// <summary>
        /// Get an object of LoanApplication class.
        /// </summary>
        /// <returns>Returns object of LoanApplication class</returns>
        private LoanApplication GetLoanApplicationObject()
        {
            return new LoanApplication()
            {
                Id = Guid.NewGuid(),
                LoanPurposeId = Guid.NewGuid(),
                LoanAmount = 100000,
                Status = LoanApplicationStatusType.Draft,
                LoanPurpose = new LoanPurpose() { Id = Guid.NewGuid(), Name = "Asset Purchase", LoanTypeId = Guid.NewGuid() },
                SubLoanPurpose = new SubLoanPurpose { Id = Guid.NewGuid(), Name = "Other Long Term Need" },
                LoanPeriod = 24,
                LoanApplicationNumber = "GHI5678KK",
                CreatedByUserId = _currentUserAC.Id,
                UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                {
                    new UserLoanSectionMapping
                    {
                        Id= Guid.NewGuid(),
                        LoanApplicationId = Guid.NewGuid(),
                        UserId = _currentUserAC.Id,
                        SectionId = Guid.NewGuid(),
                        Section = new Section { Id = Guid.NewGuid(), Name = "Company Info", Order = 2, IsEnabled = true }
                    }
                }
            };
        }

        /// <summary>
        /// Get an object of ApplicationBasicDetailAC class.
        /// </summary>
        /// <returns>Returns object of ApplicationBasicDetailAC class</returns>
        private ApplicationBasicDetailAC GetApplicationBasicDetailACObject()
        {
            return new ApplicationBasicDetailAC()
            {
                Id = Guid.NewGuid(),
                LoanPurposeId = Guid.NewGuid(),
                LoanPeriod = 24,
                Status = LoanApplicationStatusType.Draft,
                LoanAmount = 100000,
                LoanApplicationNumber = "GH3328515678KK"
            };
        }

        /// <summary>
        /// Get an object of Company with all the virtual objects included.
        /// </summary>
        /// <returns></returns>
        private Company GetCompanyInfoObjectWithDetailedObject()
        {
            return new Company
            {
                Name = "Google",
                BusinessAge = new BusinessAge { Id = Guid.NewGuid(), Age = StringConstant.SixMonthToOneYear, Order = 1 },
                CIN = "132-52-4312",
                NAICSIndustryType = new NAICSIndustryType { Id = Guid.NewGuid(), IndustryType = "Milk", IndustryCode = "5233", NAICSParentSectorId = Guid.NewGuid() },
                CompanySize = new CompanySize { Id = Guid.NewGuid(), Size = StringConstant.AboveTen, Order = 1 },
                CompanyStructure = new CompanyStructure { Id = Guid.NewGuid(), Structure = StringConstant.Proprietor, Order = 1 },
                IndustryExperience = new IndustryExperience { Id = Guid.NewGuid(), Experience = StringConstant.IndustryExperienceAboveFiveYears, Order = 1, IsEnabled = true },
                CompanyFiscalYearStartMonth = 12,
                CompanyRegisteredState = "California"
            };
        }

        /// <summary>
        /// Fetch Address object.
        /// </summary>
        /// <returns></returns>
        private Address GetAddressObject()
        {
            return new Address
            {
                Id = Guid.NewGuid(),
                City = "xyz",
                IntegratedServiceConfigurationId = Guid.NewGuid(),
                PrimaryNumber = "96",
                SecondaryNumber = "36",
                StreetLine = "abc df",
                StreetSuffix = "sd",
                StateAbbreviation = "AD",
                ZipCode = "125462",
                Response = @""
            };
        }

        /// <summary>
        /// Get list of relation mappings list for application details.
        /// </summary>
        /// <returns>List of EntityRelationshipMapping</returns>
        private List<EntityRelationshipMapping> GetListOfRelationMappingsForApplicationDetails()
        {
            var entityIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var entityTaxFormIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var taxYearlyMappingIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var documentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var companyOne = GetCompanyInfoObjectWithDetailedObject();
            companyOne.Name = "Google";
            companyOne.CIN = "135488721";
            var companyTwo = GetCompanyInfoObjectWithDetailedObject();
            companyTwo.Name = "Twitter";
            companyTwo.CIN = "125488721";
            var consentList = GetEntityLoanApplicationConsentList();
            consentList.AddRange(GetEntityLoanApplicationConsentList());
            consentList.ElementAt(0).ConsenteeId = entityIds.ElementAt(1);
            consentList.ElementAt(1).ConsenteeId = entityIds.ElementAt(2);
            consentList.ElementAt(2).ConsenteeId = entityIds.ElementAt(3);
            consentList.ElementAt(3).ConsenteeId = entityIds.ElementAt(4);

            return new List<EntityRelationshipMapping>
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityIds.First(),
                    RelativeEntityId = entityIds.ElementAt(1),
                    PrimaryEntity = new DomainModel.Models.EntityInfo.Entity {
                        Company = companyOne,
                        Address = GetAddressObject(),
                        CreditReports = new List<CreditReport>(){ new CreditReport { Id = Guid.NewGuid(), IntegratedServiceConfiguration = new IntegratedServiceConfiguration { Name = StringConstant.EquifaxAPI } } },
                        EntityConsents = new List<EntityLoanApplicationConsent> { consentList.ElementAt(0) },
                        EntityTaxForms = new List<EntityTaxForm>()
                        { new EntityTaxForm { Id = entityTaxFormIds.First(), EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>
                        { new EntityTaxYearlyMapping { Id = taxYearlyMappingIds.First(), DocumentId = documentIds.First(), Period = "2019", UploadedDocument = new Document{
                            Id = documentIds.First(), Name = "XYZ", Path = "ABC" } } } } }
                    },
                    RelativeEntity = new DomainModel.Models.EntityInfo.Entity {
                        User = new User { Email = "john@doe.com", FirstName = "John" },
                        Address = GetAddressObject(),
                        CreditReports = new List<CreditReport>(){ new CreditReport { Id = Guid.NewGuid(),IntegratedServiceConfiguration =new IntegratedServiceConfiguration { Name = StringConstant.EquifaxAPI} } },
                        EntityConsents = new List<EntityLoanApplicationConsent> { consentList.ElementAt(0) }
                    },
                    Relationship = new Relationship { Relation = "Shareholder" },
                    SharePercentage = 60
                },
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityIds.First(),
                    RelativeEntityId = entityIds.ElementAt(2),
                    PrimaryEntity = new DomainModel.Models.EntityInfo.Entity {
                        Company = companyOne,
                        Address = GetAddressObject(),
                        CreditReports = new List<CreditReport>(){ new CreditReport { Id = Guid.NewGuid(), IntegratedServiceConfiguration = new IntegratedServiceConfiguration { Name = StringConstant.EquifaxAPI } } },
                        EntityConsents = new List<EntityLoanApplicationConsent> { consentList.ElementAt(0) },
                        EntityTaxForms = new List<EntityTaxForm>()
                        { new EntityTaxForm { Id = entityTaxFormIds.First(), EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>
                        { new EntityTaxYearlyMapping { Id = taxYearlyMappingIds.First(), DocumentId = documentIds.First(), Period = "2019", UploadedDocument = new Document{
                            Id = documentIds.First(), Name = "XYZ", Path = "ABC" } } } } }
                    },
                    RelativeEntity = new DomainModel.Models.EntityInfo.Entity {
                        User = new User { Email = "lorem@epsum.com", FirstName = "Lorem" },
                        Address = GetAddressObject(),
                        CreditReports = new List<CreditReport>(){ new CreditReport { Id = Guid.NewGuid(), IntegratedServiceConfiguration = new IntegratedServiceConfiguration { Name = StringConstant.EquifaxAPI } } },
                        EntityConsents = new List<EntityLoanApplicationConsent> { consentList.ElementAt(1) },
                    },
                    Relationship = new Relationship { Relation = "Shareholder" },
                    SharePercentage = 60
                },
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityIds.Last(),
                    RelativeEntityId = entityIds.ElementAt(3),
                    PrimaryEntity = new DomainModel.Models.EntityInfo.Entity {
                        Company = companyTwo,
                        Address = GetAddressObject(),
                        CreditReports = new List<CreditReport>(){ new CreditReport { Id = Guid.NewGuid(), IntegratedServiceConfiguration = new IntegratedServiceConfiguration { Name = StringConstant.EquifaxAPI } } },
                        EntityConsents = new List<EntityLoanApplicationConsent> { consentList.ElementAt(0) },
                        EntityTaxForms = new List<EntityTaxForm>()
                        { new EntityTaxForm { Id = entityTaxFormIds.Last(), EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>
                        { new EntityTaxYearlyMapping { Id = taxYearlyMappingIds.Last(), DocumentId = documentIds.Last(), Period = "2019", UploadedDocument = new Document{
                            Id = documentIds.Last(), Name = "XYZ", Path = "ABC" } } } } }
                    },
                    RelativeEntity = new DomainModel.Models.EntityInfo.Entity {
                        User = new User { Email = "lorem@doe.com", FirstName = "Lorem" },
                        Address = GetAddressObject(),
                        CreditReports = new List<CreditReport>(){ new CreditReport { Id = Guid.NewGuid(), IntegratedServiceConfiguration = new IntegratedServiceConfiguration { Name = StringConstant.EquifaxAPI } } },
                        EntityConsents = new List<EntityLoanApplicationConsent> { consentList.ElementAt(2) },
                    },
                    Relationship = new Relationship { Relation = "Shareholder" },
                    SharePercentage = 30
                },
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityIds.Last(),
                    RelativeEntityId = entityIds.ElementAt(4),
                    PrimaryEntity = new DomainModel.Models.EntityInfo.Entity {
                        Company = companyTwo,
                        Address = GetAddressObject(),
                        CreditReports = new List<CreditReport>(){ new CreditReport { Id = Guid.NewGuid(), IntegratedServiceConfiguration = new IntegratedServiceConfiguration { Name = StringConstant.EquifaxAPI } } },
                        EntityConsents = new List<EntityLoanApplicationConsent> { consentList.ElementAt(0) },
                        EntityTaxForms = new List<EntityTaxForm>()
                        { new EntityTaxForm { Id = entityTaxFormIds.Last(), EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>
                        { new EntityTaxYearlyMapping { Id = taxYearlyMappingIds.Last(), DocumentId = documentIds.Last(), Period = "2019", UploadedDocument = new Document{
                            Id = documentIds.Last(), Name = "XYZ", Path = "ABC" } } } } }
                    },
                    RelativeEntity = new DomainModel.Models.EntityInfo.Entity {
                        User = new User { Email = "john@epsum.com", FirstName = "John" },
                        Address = GetAddressObject(),
                        CreditReports = new List<CreditReport>(){ new CreditReport { Id = Guid.NewGuid(), IntegratedServiceConfiguration = new IntegratedServiceConfiguration { Name = StringConstant.EquifaxAPI } } },
                        EntityConsents = new List<EntityLoanApplicationConsent> { consentList.ElementAt(3) },
                    },
                    Relationship = new Relationship { Relation = "Shareholder" },
                    SharePercentage = 70
                }
            };
        }

        /// <summary>
        /// Get EntityLoanApplicationConsent list.
        /// </summary>
        /// <returns>Returns list of EntityLoanApplicationConsent</returns>
        private List<EntityLoanApplicationConsent> GetEntityLoanApplicationConsentList()
        {
            var applicationId = Guid.NewGuid();
            var consenteeId = Guid.NewGuid();
            return new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    Consent = new Consent { ConsentText = "Consent 1" },
                    LoanApplicationId = applicationId,
                    ConsenteeId = consenteeId,
                    IsConsentGiven = true,
                    ConsentId = Guid.NewGuid(),
                    LoanApplication = GetLoanApplicationObject()
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                     Consent = new Consent { ConsentText = "Consent 2" },
                    LoanApplicationId = applicationId,
                    ConsenteeId = consenteeId,
                    IsConsentGiven = true,
                    ConsentId = Guid.NewGuid(),
                    LoanApplication = GetLoanApplicationObject()
                }
            };
        }

        /// <summary>
        /// Get EntityLoanApplicationMapping list.
        /// </summary>
        /// <returns></returns>
        private List<EntityLoanApplicationMapping> GetEntityLoanApplicationMappingList()
        {
            var application = GetLoanApplicationObject();
            return new List<EntityLoanApplicationMapping>
            {
                new EntityLoanApplicationMapping
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = application.Id,
                    LoanApplication = application,
                    Entity = new DomainModel.Models.EntityInfo.Entity
                    {
                        Id = Guid.NewGuid(),
                        EntityFinances= new List<EntityFinance>
                        {
                            new EntityFinance
                            {
                                Id = Guid.NewGuid()
                            }
                        },
                        EntityTaxForms = new List<EntityTaxForm>()
                        {
                            new EntityTaxForm
                            {
                                Id = Guid.NewGuid()
                            }
                        },
                        PrimaryEntityRelationships = new List<EntityRelationshipMapping>
                        {
                            new EntityRelationshipMapping
                            {
                                RelativeEntity = new DomainModel.Models.EntityInfo.Entity
                                {
                                    EntityFinances = new List<EntityFinance> { new EntityFinance { } }
                                }
                            }
                        },
                        AdditionalDocuments = new List<EntityAdditionalDocument>
                        {
                            new EntityAdditionalDocument
                            {
                                Id = Guid.NewGuid(),
                                LoanApplicationId = null
                            },
                            new EntityAdditionalDocument
                            {
                                Id = Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid()
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Get the consent text list.
        /// </summary>
        /// <returns></returns>
        private List<Consent> GetConsents()
        {
            return new List<Consent>
            {
                new Consent { Id = Guid.NewGuid(), ConsentText = "Consent1", IsEnabled = true },
                new Consent { Id = Guid.NewGuid(), ConsentText = "Consent2", IsEnabled = true }
            };
        }

        /// <summary>
        /// Fetch product details
        /// </summary>
        /// <returns></returns>
        private ProductDetailsAC FetchProductDetails()
        {
            return new ProductDetailsAC
            {
                MaxMonthlyPayment = (decimal)2648.83,
                MinMonthlyPayment = (decimal)463.15,
                MonthlyPayment = (decimal)3329.21,
                MaxProductTenure = 5,
                MinProductTenure = 1,
                TotalInterest = "$ 39,826.82",
                TotalPayment = "$ 139,826.82",
                InterestRate = (decimal)19.99
            };
        }

        /// <summary>
        /// Method is to fetch list of loan purpose range type mapping data
        /// </summary>
        /// <returns>List of loanPurposeRangeTypeMapping</returns>
        private List<LoanPurposeRangeTypeMapping> FetchLoanPurposeRangeTypeMappings()
        {
            return new List<LoanPurposeRangeTypeMapping>(){
                new LoanPurposeRangeTypeMapping()
                {
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.Lifecycle
                    },
                    Maximum = 5,
                    Minimum = 1,
                    StepperAmount = (decimal)0.5
                },
                new LoanPurposeRangeTypeMapping()
                {
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.LoanAmount
                    },
                    Maximum = 100000,
                    Minimum = 5000,
                    StepperAmount = (decimal)1000
                }
            };
        }

        /// <summary>
        /// Method is to fetch list of product range type mapping data
        /// </summary>
        /// <returns>List of ProductRangeTypeMapping</returns>
        private List<ProductRangeTypeMapping> FetchProductRangeTypeMappings()
        {
            return new List<ProductRangeTypeMapping>()
            {
                new ProductRangeTypeMapping()
                {
                    Maximum = 60,
                    Minimum = 12,
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.Lifecycle
                    }
                },
                new ProductRangeTypeMapping()
                {
                    Maximum = 100000,
                    Minimum = 5000,
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.LoanAmount
                    }
                }
            };
        }

        private List<ProductPercentageSuitabilityAC> FetchProductPercentageSuitabilityList()
        {
            return new List<ProductPercentageSuitabilityAC>() {
                new ProductPercentageSuitabilityAC()
                {
                    IsRecommended = true,
                    PercentageSuitability = 100
                }
            };
        }

        private List<ProductSubPurposeMapping> FetchProductPurposeMappings()
        {
            return new List<ProductSubPurposeMapping>()
            {
                new ProductSubPurposeMapping()
                {
                    SubLoanPurpose = new SubLoanPurpose()
                    {
                        Name = "Other Long Term Need"
                    }
                }
            };

        }

        #endregion

        #region Public methods
        /// <summary>
        /// Method to verify that it throws an error for unauthorized user access to it.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationDetailsByIdAsync_UnauthorizedUser_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _applicationRepository.GetLoanApplicationDetailsByIdAsync(loanApplicationId, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that it throws error for non-existing LoanApplicationId.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationDetailsByIdAsync_ApplicationNotExistForGivenId_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<LoanApplication> loanApplications = new List<LoanApplication>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _applicationRepository.GetLoanApplicationDetailsByIdAsync(loanApplicationId, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that if application is in locked state then it will fetch the data from snapshot table.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationDetailsByIdAsync_ApplicationExistInLockedStatus_FetchDataFromSnapshotTable()
        {
            //Arrange
            List<LoanApplication> loanApplications = new List<LoanApplication>();
            loanApplications.Add(GetLoanApplicationObject());
            loanApplications.First().CreatedByUserId = _currentUserAC.Id;
            loanApplications.First().LoanApplicationNumber = "LP2012202013526332";
            loanApplications.First().Status = LoanApplicationStatusType.Locked;
            List<LoanApplicationSnapshot> snapshots = new List<LoanApplicationSnapshot> {
                new LoanApplicationSnapshot
                {
                    Id = Guid.NewGuid(),
                    ApplicationDetailsJson = "{\"BasicDetails\":{\"Id\":\"1dab3e0e-6b61-4426-b966-0eed64dd3489\",\"EntityId\":\"1dab3e0e-6b61-4426-b966-0eed64d22222\",\"LoanApplicationNumber\":\"LP2012202013526332\"},\"BorrowingEntities\":[{\"Id\":\"1dab3e0e-6b61-4426-b966-0eed64d22222\"},{\"Id\":\"1dab3e0e-6b61-4426-b966-0eed64d22222\"}]}",
                    LoanApplicationId = loanApplications.First().Id,
                    LoanApplication = loanApplications.First()
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplicationSnapshot, bool>>>()))
                .Returns(snapshots.AsQueryable().BuildMock().Object);

            //Act
            ApplicationAC actual = await _applicationRepository.GetLoanApplicationDetailsByIdAsync(loanApplications.First().Id, _currentUserAC);

            //Assert
            Assert.Equal(loanApplications.First().Id, actual.BasicDetails.Id);
            Assert.Equal(loanApplications.First().LoanApplicationNumber, actual.BasicDetails.LoanApplicationNumber);
            Assert.Equal(loanApplications.First().UserLoanSectionMappings.First().Section.Name, actual.BasicDetails.SectionName);
        }

        /// <summary>
        /// Method to verify that if application data doesn't exist in snapshot table then it throws an exception.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationDetailsByIdAsync_ApplicationNotExistInSnapshot_ThrowsDataNotFoundException()
        {
            //Arrange
            List<LoanApplication> loanApplications = new List<LoanApplication>();
            loanApplications.Add(GetLoanApplicationObject());

            loanApplications.First().Status = LoanApplicationStatusType.Locked;
            List<LoanApplicationSnapshot> snapshots = new List<LoanApplicationSnapshot>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplicationSnapshot, bool>>>()))
                 .Returns(snapshots.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _applicationRepository.GetLoanApplicationDetailsByIdAsync(loanApplications.First().Id, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that if application is in draft state and product also found then it will fetch the current data with product details.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationDetailsByIdAsync_ApplicationExistInDraftStatus_ProductFound_FetchCurrentDataWithProductDetails()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<LoanApplication> loanApplications = new List<LoanApplication> { GetLoanApplicationObject() };
            loanApplications.First().Product = new Product() { Id = Guid.NewGuid() };
            loanApplications.First().ProductId = loanApplications.First().Product.Id;
            List<Guid> entityIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            List<EntityRelationshipMapping> entityRelationsMappings = GetListOfRelationMappingsForApplicationDetails();
            entityRelationsMappings.RemoveRange(2, 2);
            entityRelationsMappings.ElementAt(0).PrimaryEntityId = entityIds.First();
            entityRelationsMappings.ElementAt(0).RelativeEntityId = entityIds.ElementAt(1);
            entityRelationsMappings.ElementAt(1).PrimaryEntityId = entityIds.First();
            entityRelationsMappings.ElementAt(1).RelativeEntityId = entityIds.ElementAt(2);
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            List<EntityTaxYearlyMapping> entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() { new EntityTaxYearlyMapping() { Period = "2019", EntityTaxFormId = entityRelationsMappings.First().PrimaryEntity.EntityTaxForms.First().Id, UploadedDocument = new Document() { Id = Guid.NewGuid(), Name = "XYZ", Path = "XYZ" } }, new EntityTaxYearlyMapping() { Period = "2018", EntityTaxFormId = entityRelationsMappings.First().PrimaryEntity.EntityTaxForms.First().Id, UploadedDocument = new Document() { Id = Guid.NewGuid(), Name = "XYZ", Path = "XYZ" } } };
            var productRangeTypeMappings = FetchProductRangeTypeMappings();
            var loanPurposeRangeTypeMappings = FetchLoanPurposeRangeTypeMappings();
            loanPurposeRangeTypeMappings.ForEach(x => x.LoanPurposeId = loanApplications.First().LoanPurposeId);
            productRangeTypeMappings.ForEach(x => x.ProductId = loanApplications.First().Product.Id);
            var productDetails = FetchProductDetails();
            var productPurposeMappings = FetchProductPurposeMappings();
            productPurposeMappings.ForEach(x => { x.ProductId = loanApplications.First().ProductId ?? Guid.NewGuid(); x.SubLoanPurposeId = loanApplications.First().LoanPurposeId; });
            var productPercentageSuitabilityList = FetchProductPercentageSuitabilityList();
            productPercentageSuitabilityList.ForEach(x => x.ProductId = loanApplications.First().ProductId ?? Guid.NewGuid());

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationsMappings.AsQueryable().BuildMock().Object);
            _configurationMock.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value).Returns("false").Returns("$");
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductRangeTypeMapping>()).Returns(productRangeTypeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductSubPurposeMapping>()).Returns(productPurposeMappings.AsQueryable().BuildMock().Object);
            _rulesUtilityMock.Setup(x => x.ExecuteProductRules(It.IsAny<ProductRuleAC>())).ReturnsAsync(productPercentageSuitabilityList);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanPurposeRangeTypeMapping, bool>>>())).Returns(loanPurposeRangeTypeMappings.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CalculateLoanProductDetail(It.IsAny<List<ProductRangeTypeMapping>>(), It.IsAny<string>(), It.IsAny<LoanApplication>())).Returns(productDetails);
            _mapper.Map<RecommendedProductAC>(loanApplications.First().Product);

            //Act
            ApplicationAC actual = await _applicationRepository.GetLoanApplicationDetailsByIdAsync(loanApplicationId, _currentUserAC);

            //Assert
            Assert.Equal(loanApplications.First().Id, actual.BasicDetails.Id);
            Assert.Single(actual.BorrowingEntities);
            Assert.Equal(2, actual.BorrowingEntities.First().LinkedEntities.Count);
            Assert.NotNull(actual.SelectedProduct);
            Assert.Equal((decimal)2648.83, actual.SelectedProduct.ProductDetails.MaxMonthlyPayment);
            Assert.Equal((decimal)463.15, actual.SelectedProduct.ProductDetails.MinMonthlyPayment);
            Assert.Equal((decimal)3329.21, actual.SelectedProduct.ProductDetails.MonthlyPayment);
            Assert.Equal(5, actual.SelectedProduct.ProductDetails.MaxProductTenure);
            Assert.Equal(1, actual.SelectedProduct.ProductDetails.MinProductTenure);
            Assert.Contains("$ 39,826.82", actual.SelectedProduct.ProductDetails.TotalInterest);
            Assert.Contains("$ 139,826.82", actual.SelectedProduct.ProductDetails.TotalPayment);
        }

        /// <summary>
        /// Method to verify that if application is in draft state then it will fetch the current data.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationDetailsByIdAsync_ApplicationExistInDraftStatus_FetchCurrentData()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<LoanApplication> loanApplications = new List<LoanApplication> { GetLoanApplicationObject() };
            List<Guid> entityIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            List<EntityRelationshipMapping> entityRelationsMappings = GetListOfRelationMappingsForApplicationDetails();
            entityRelationsMappings.RemoveRange(2, 2);
            entityRelationsMappings.ElementAt(0).PrimaryEntityId = entityIds.First();
            entityRelationsMappings.ElementAt(0).RelativeEntityId = entityIds.ElementAt(1);
            entityRelationsMappings.ElementAt(1).PrimaryEntityId = entityIds.First();
            entityRelationsMappings.ElementAt(1).RelativeEntityId = entityIds.ElementAt(2);
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            List<EntityTaxYearlyMapping> entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() { new EntityTaxYearlyMapping() { Period = "2019", EntityTaxFormId = entityRelationsMappings.First().PrimaryEntity.EntityTaxForms.First().Id, UploadedDocument = new Document() { Id = Guid.NewGuid(), Name = "XYZ", Path = "XYZ" } }, new EntityTaxYearlyMapping() { Period = "2018", EntityTaxFormId = entityRelationsMappings.First().PrimaryEntity.EntityTaxForms.First().Id, UploadedDocument = new Document() { Id = Guid.NewGuid(), Name = "XYZ", Path = "XYZ" } } };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationsMappings.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("false");
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);

            //Act
            ApplicationAC actual = await _applicationRepository.GetLoanApplicationDetailsByIdAsync(loanApplicationId, _currentUserAC);

            //Assert
            Assert.Equal(loanApplications.First().Id, actual.BasicDetails.Id);
            Assert.Single(actual.BorrowingEntities);
            Assert.Equal(2, actual.BorrowingEntities.First().LinkedEntities.Count);
        }

        /// <summary>
        /// Test method to verify that it throws an error if amount or period or purpose id is null in the object comes in request.
        /// </summary>
        [Theory]
        [InlineData(0, 0, "00000000-0000-0000-0000-000000000000")]
        [InlineData(0, 0, "85100000-4512-5421-4521-550000000550")]
        [InlineData(0, 2.5, "00000000-0000-0000-0000-000000000000")]
        [InlineData(0, 3, "85100000-4512-5421-4521-550000000550")]
        [InlineData(35636, 0, "00000000-0000-0000-0000-000000000000")]
        [InlineData(35636, 0, "85100000-4512-5421-4521-550000000550")]
        [InlineData(35636, 4.5, "00000000-0000-0000-0000-000000000000")]
        public async Task SaveLoanApplicationAsync_AmountOrPeriodOrPurposeIdIsNull_ThrowsInvalidParameteException(decimal amount, decimal period, string purposeId)
        {
            //Arrange
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            loanApplicationBasicDetailAC.LoanAmount = amount;
            loanApplicationBasicDetailAC.LoanPeriod = period;
            loanApplicationBasicDetailAC.LoanPurposeId = Guid.Parse(purposeId);
            Guid loanApplicationId = Guid.NewGuid();

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, loanApplicationId));
        }

        /// <summary>
        /// Test method to verify that it throws an error if the loan purpose, given in request, is not enabled.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_LoanPurposeIsNotEnabled_ThrowsInvalidParameteException()
        {
            //Arrange
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            Guid loanApplicationId = Guid.NewGuid();
            LoanPurpose loanPurpose = new LoanPurpose
            {
                Id = loanApplicationBasicDetailAC.LoanPurposeId,
                IsEnabled = false
            };
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanPurpose, bool>>>())).ReturnsAsync(loanPurpose);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, loanApplicationId));
        }

        /// <summary>
        /// Method to verify that add operation is being performed if loan application id is null in argument.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_NullLoanApplicationId_PerformsAddOperation()
        {
            //Arrange
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            Section section = new Section
            {
                Id = Guid.NewGuid(),
                Name = "Company Info",
                Order = 2,
                IsEnabled = true
            };
            LoanPurpose loanPurpose = new LoanPurpose
            {
                Id = loanApplicationBasicDetailAC.LoanPurposeId,
                IsEnabled = true
            };
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("LP");
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<Section, bool>>>())).ReturnsAsync(section);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanPurpose, bool>>>())).ReturnsAsync(loanPurpose);

            //Act
            ApplicationBasicDetailAC actual = await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, null);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LoanApplication>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            Assert.NotNull(actual.SectionName);
            Assert.NotNull(actual.Id);
        }

        /// <summary>
        /// Method to verify that if bank code doesn't exist in configuration then it throws an exception.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_BankCodeConfigurationNotFound_ThrowsConfigurationNotFoundException()
        {
            //Arrange
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            string bankCode = null;
            LoanPurpose loanPurpose = new LoanPurpose
            {
                Id = loanApplicationBasicDetailAC.LoanPurposeId,
                IsEnabled = true
            };
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns(bankCode);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanPurpose, bool>>>())).ReturnsAsync(loanPurpose);

            //Act

            //Assert
            await Assert.ThrowsAsync<ConfigurationNotFoundException>(async () => await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, null));
        }

        /// <summary>
        /// Method to verify that private method is being called and it returns valid loan application number
        /// before add and save operation is being performed.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_GenerateLoanApplicationNumber_PrivateMethodReturnsValidNumber()
        {
            //Arrange
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            loanApplicationBasicDetailAC.LoanApplicationNumber = null;
            Section section = new Section
            {
                Id = Guid.NewGuid(),
                Name = "Company Info",
                Order = 2,
                IsEnabled = true
            };
            LoanPurpose loanPurpose = new LoanPurpose
            {
                Id = loanApplicationBasicDetailAC.LoanPurposeId,
                IsEnabled = true
            };
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("LP");
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<Section, bool>>>())).ReturnsAsync(section);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanPurpose, bool>>>())).ReturnsAsync(loanPurpose);

            //Act
            ApplicationBasicDetailAC actual = await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, null);

            //Assert
            Assert.Matches("^[A-Z]{2}[0-9]{1,16}$", actual.LoanApplicationNumber);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LoanApplication>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Method to verify that update operation is being performed if loan application exists for given id in argument.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_LoanApplicationExist_PerformsUpdateOperation()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            List<LoanApplication> loanApplications = new List<LoanApplication>()
            {
                new LoanApplication()
                {
                    Id = loanApplicationId,
                    Status = LoanApplicationStatusType.Draft,
                    CreatedByUser= new User
                    {
                        Id = _currentUserAC.Id
                    },
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                    {
                        new UserLoanSectionMapping
                        {
                            Id= Guid.NewGuid(),
                            LoanApplicationId = Guid.NewGuid(),
                            UserId = Guid.NewGuid(),
                            SectionId = Guid.NewGuid(),
                            Section = new Section { Id = Guid.NewGuid(), Name = "Loan Product", IsEnabled = true, Order = 2 }
                        }
                    }
                }
            };
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            LoanPurpose loanPurpose = new LoanPurpose
            {
                Id = loanApplicationBasicDetailAC.LoanPurposeId,
                IsEnabled = true
            };
            loanApplicationBasicDetailAC.Id = loanApplicationId;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanPurpose, bool>>>())).ReturnsAsync(loanPurpose);

            //Act
            await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<LoanApplication>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Test method to verify that it throws an error if loan application doesn't exists for given id in argument.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_LoanApplicationNotExists_ThrowsErrorNoUpdateOperation()
        {
            //Arrange
            var loanApplications = new List<LoanApplication>();
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            var loanApplicationId = Guid.NewGuid();
            LoanPurpose loanPurpose = new LoanPurpose
            {
                Id = loanApplicationBasicDetailAC.LoanPurposeId,
                IsEnabled = true
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanPurpose, bool>>>())).ReturnsAsync(loanPurpose);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, loanApplicationId));
        }

        /// <summary>
        /// Method to verify that it throws an error if user is unauthorized to access the loan application.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_UnauthorizedUser_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            LoanPurpose loanPurpose = new LoanPurpose
            {
                Id = loanApplicationBasicDetailAC.LoanPurposeId,
                IsEnabled = true
            };
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanPurpose, bool>>>())).ReturnsAsync(loanPurpose);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, loanApplicationId));
        }

        /// <summary>
        /// Test method to verify that it throws an error if update operation is not allowed on given loan application.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_UpdateOperationNotAllowed_ThrowsErrorNoUpdateOperation()
        {
            //Arrange
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            var loanApplicationId = Guid.NewGuid();
            LoanPurpose loanPurpose = new LoanPurpose
            {
                Id = loanApplicationBasicDetailAC.LoanPurposeId,
                IsEnabled = true
            };

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanPurpose, bool>>>())).ReturnsAsync(loanPurpose);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, loanApplicationId));
        }

        /// <summary>
        /// Test method to verify that it throws an error if id or application number don't exist in request object.
        /// </summary>
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", null)]
        [InlineData("85100000-4512-5421-4521-550000000550", null)]
        [InlineData("00000000-0000-0000-0000-000000000000", "GH3328515678KK")]
        public async Task SaveLoanApplicationAsync_IdOrApplicationNumberIsEmptyOrNull_ThrowsInvalidParameterException(string id, string number)
        {
            //Arrange
            var loanApplicationBasicDetailAC = GetApplicationBasicDetailACObject();
            loanApplicationBasicDetailAC.LoanApplicationNumber = number;
            var loanApplicationId = Guid.Parse(id);
            LoanPurpose loanPurpose = new LoanPurpose
            {
                Id = loanApplicationBasicDetailAC.LoanPurposeId,
                IsEnabled = true
            };

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanPurpose, bool>>>())).ReturnsAsync(loanPurpose);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.SaveLoanApplicationAsync(loanApplicationBasicDetailAC, _currentUserAC, loanApplicationId));
        }

        /// <summary>
        /// Check if logged in user does not have provided loan application access.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_NoLoanAccess_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _applicationRepository.LinkApplicationWithEntityAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if logged in user does not have provided loan application access.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_NoCompanyAccess_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>() {
                new LoanApplication
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();

            DomainModel.Models.EntityInfo.Entity entity = new DomainModel.Models.EntityInfo.Entity()
            {
                Id = Guid.NewGuid(),
                Type = EntityType.Company
            };


            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<DomainModel.Models.EntityInfo.Entity, bool>>>()))
                .ReturnsAsync(entity);
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _applicationRepository.LinkApplicationWithEntityAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if company structure proprietor and not 100 percentage throw InvalidParameterException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_ProprietorSharePercentageNot100_VerifyThrowsInvalidDataException()
        {
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>() {
                new LoanApplication
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappings();

            DomainModel.Models.EntityInfo.Entity entity = new DomainModel.Models.EntityInfo.Entity()
            {
                Id = Guid.NewGuid(),
                Type = EntityType.Company
            };

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            entityRelationshipMappings.Single().PrimaryEntity.Company.CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<DomainModel.Models.EntityInfo.Entity, bool>>>()))
                .ReturnsAsync(entity);
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.LinkApplicationWithEntityAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if company structure partnership and one share holder throw InvalidParameterException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_PartnershipOneShareHolder_VerifyThrowsInvalidDataException()
        {
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>() {
                new LoanApplication
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappings();

            DomainModel.Models.EntityInfo.Entity entity = new DomainModel.Models.EntityInfo.Entity()
            {
                Id = Guid.NewGuid(),
                Type = EntityType.Company
            };

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            entityRelationshipMappings.Single().PrimaryEntity.Company.CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<DomainModel.Models.EntityInfo.Entity, bool>>>()))
                .ReturnsAsync(entity);
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.LinkApplicationWithEntityAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if company structure LLC and not majority share holder throw InvalidParameterException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_LLCNotMajorityShareHolder_VerifyThrowsInvalidDataException()
        {
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>() {
                new LoanApplication
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappings();
            entityRelationshipMappings.Single().SharePercentage = 10;
            DomainModel.Models.EntityInfo.Entity entity = new DomainModel.Models.EntityInfo.Entity()
            {
                Id = Guid.NewGuid(),
                Type = EntityType.Company
            };

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            entityRelationshipMappings.Single().PrimaryEntity.Company.CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<DomainModel.Models.EntityInfo.Entity, bool>>>()))
                .ReturnsAsync(entity);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("75");
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.LinkApplicationWithEntityAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if company structure LLC and not total 100 share percentage throw InvalidParameterException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_LLCNotHundredSharepercentage_VerifyThrowsInvalidDataException()
        {
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>() {
                new LoanApplication
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappings();

            DomainModel.Models.EntityInfo.Entity entity = new DomainModel.Models.EntityInfo.Entity()
            {
                Id = Guid.NewGuid(),
                Type = EntityType.Company
            };

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            entityRelationshipMappings.Single().PrimaryEntity.Company.CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);
            entityRelationshipMappings.Add(entityRelationshipMappings.Single());
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<DomainModel.Models.EntityInfo.Entity, bool>>>()))
                .ReturnsAsync(entity);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("75");
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.LinkApplicationWithEntityAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if company structure LLC and more than one share holder and not 100 percent share throw InvalidParameterException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_NotHundredPercentShare_VerifyThrowsInvalidDataException()
        {
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>() {
                new LoanApplication
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappings();

            DomainModel.Models.EntityInfo.Entity entity = new DomainModel.Models.EntityInfo.Entity()
            {
                Id = Guid.NewGuid(),
                Type = EntityType.Company
            };

            entityRelationshipMappings.Single().SharePercentage = 10;
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            entityRelationshipMappings.Single().PrimaryEntity.Company.CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;
            entityRelationshipMappings.Add(entityRelationshipMappings.Single());
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<DomainModel.Models.EntityInfo.Entity, bool>>>()))
                .ReturnsAsync(entity);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("75");
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.LinkApplicationWithEntityAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if company structure LLC and more than one share holder and not 100 percent share throw InvalidParameterException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_LoanApplicationAlreadyLinked_VerifyThrowsInvalidDataException()
        {
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>() {
                new LoanApplication
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappings();

            DomainModel.Models.EntityInfo.Entity entity = new DomainModel.Models.EntityInfo.Entity()
            {
                Id = Guid.NewGuid(),
                Type = EntityType.Company
            };

            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping
                {
                    Id = Guid.NewGuid()
                }
            };

            entityRelationshipMappings.Single().SharePercentage = 100;
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            entityRelationshipMappings.Single().PrimaryEntity.Company.CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<DomainModel.Models.EntityInfo.Entity, bool>>>()))
                .ReturnsAsync(entity);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("75");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.LinkApplicationWithEntityAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if Loan linked successfully.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_AlreadyLinkedApplication_VerifyThrowsInvalidDataException()
        {
            Guid applicationId = Guid.NewGuid();
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>() {
                new LoanApplication
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappings();

            DomainModel.Models.EntityInfo.Entity entity = new DomainModel.Models.EntityInfo.Entity()
            {
                Id = Guid.NewGuid(),
                Type = EntityType.Company
            };

            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>() {
                new EntityLoanApplicationMapping
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = applicationId,
                    LoanApplication = new LoanApplication
                    {
                        Id = Guid.NewGuid(),
                        Status = LoanApplicationStatusType.Approved
                    }
                }
            };

            entityRelationshipMappings.Single().SharePercentage = 100;
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            entityRelationshipMappings.Single().PrimaryEntity.Company.CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<DomainModel.Models.EntityInfo.Entity, bool>>>()))
                .ReturnsAsync(entity);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("75");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.LinkApplicationWithEntityAsync(applicationId, Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if Loan linked successfully.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LinkApplicationWithEntityAsync_LoanApplicationLinked_AssertAdd()
        {
            //Arrange
            List<LoanApplication> applications = new List<LoanApplication>() {
                new LoanApplication
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappings();

            DomainModel.Models.EntityInfo.Entity entity = new DomainModel.Models.EntityInfo.Entity()
            {
                Id = Guid.NewGuid(),
                Type = EntityType.Company
            };

            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>();

            entityRelationshipMappings.Single().SharePercentage = 100;
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            entityRelationshipMappings.Single().PrimaryEntity.Company.CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<DomainModel.Models.EntityInfo.Entity, bool>>>()))
                .ReturnsAsync(entity);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("75");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            //Act
            await _applicationRepository.LinkApplicationWithEntityAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC);
            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);
        }

        #region Consent

        /// <summary>
        /// Test method to verify that it throws error when user is not authorized.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_UserIsNotAuthorized_ThrowsExceptionUnauthorizedAccess()
        {
            //Arrange
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _applicationRepository.SaveLoanConsentOfUserAsync(Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that it throws error when add or update operation is not allowed.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_AddOperationNotAllowed_ThrowsExceptionInvalidOperation()
        {
            //Arrange
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _applicationRepository.SaveLoanConsentOfUserAsync(Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that it throws error when any of the required section's value is missing.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_SectionNotFilled_ThrowsValidationException()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<Guid> entityIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            List<EntityRelationshipMapping> entityRelationsMappings = GetListOfRelationMappingsForApplicationDetails();
            entityRelationsMappings.RemoveRange(2, 2);
            entityRelationsMappings.ElementAt(0).PrimaryEntityId = entityIds.First();
            entityRelationsMappings.ElementAt(0).RelativeEntityId = entityIds.ElementAt(1);
            entityRelationsMappings.ElementAt(1).PrimaryEntityId = entityIds.First();
            entityRelationsMappings.ElementAt(1).RelativeEntityId = entityIds.ElementAt(2);
            List<EntityLoanApplicationConsent> entityConsents = new List<EntityLoanApplicationConsent>();
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            mappings.First().LoanApplication.CreatedByUser = new User
            {
                Id = _currentUserAC.Id,
                Email = _currentUserAC.Email,
                SelfDeclaredCreditScore = StringConstant.ExcellentCreditScore
            };
            mappings.First().LoanApplication.ProductId = Guid.NewGuid();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity
            {
                EntityTaxForms = new List<EntityTaxForm> { }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationsMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _applicationRepository.SaveLoanConsentOfUserAsync(loanApplicationId, _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that no any operation will be performed if the user has already given the consent.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_UserAlreadyGivenConsent_NoAnyOperationWillBeDone()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            entityConsents.ForEach(x => x.ConsenteeId = _currentUserAC.Id);

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            //Act
            await _applicationRepository.SaveLoanConsentOfUserAsync(loanApplicationId, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Never);
        }

        /// <summary>
        /// Test method to verify that exception will be thrown if entity loan application mapping is not found.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_EntityLoanMappingNotFound_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            List<EntityLoanApplicationMapping> mappings = new List<EntityLoanApplicationMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _applicationRepository.SaveLoanConsentOfUserAsync(loanApplicationId, _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that exception will be thrown if mapped loan application not found.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_MappedLoanApplicationNotFound_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication = null;
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _applicationRepository.SaveLoanConsentOfUserAsync(loanApplicationId, _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that if loan initiator's self declared credit score is null then it throws an exception.
        /// </summary>
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        public async Task SaveLoanConsentOfUserAsync_LoanInitiatorOrCreditScoreIsNull_ThrowsInvalidParameterException(bool isInitiatorAvailable, bool isCreditScoreAvailable)
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            mappings.First().LoanApplication.CreatedByUser = isInitiatorAvailable ? new User() : null;
            if (mappings.First().LoanApplication.CreatedByUser != null)
            {
                mappings.First().LoanApplication.CreatedByUser.SelfDeclaredCreditScore = isCreditScoreAvailable ? StringConstant.ExcellentCreditScore : null;
            }
            List<EntityRelationshipMapping> relationMappings = GetEntityRelationshipMappings();
            List<Consent> consents = new List<Consent>();
            mappings.First().LoanApplication.ProductId = Guid.NewGuid();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity
            {
                EntityFinances = new List<EntityFinance>
                {
                    new EntityFinance
                    {
                        EntityId = mappings.First().EntityId,
                        FinancialStatement= new FinancialStatement
                        {
                            Name=StringConstant.IncomeStatement
                        },
                        EntityFinanceYearlyMappings=new List<EntityFinanceYearlyMapping>
                        {
                            new EntityFinanceYearlyMapping
                            {
                                Id=Guid.NewGuid()
                            }
                        }
                    }
                },
                EntityTaxForms = new List<EntityTaxForm> { new EntityTaxForm { EntityId = mappings.First().EntityId } }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Consent>()).Returns(consents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relationMappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _applicationRepository.SaveLoanConsentOfUserAsync(loanApplicationId, _currentUserAC));
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<LoanApplication>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Never);
        }

        /// <summary>
        /// Test method to verify that if no consent texts are found in table then it throws an exception.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_NoAnyConsentTextsFound_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            mappings.First().LoanApplication.CreatedByUser = new User
            {
                Id = _currentUserAC.Id,
                Email = _currentUserAC.Email,
                SelfDeclaredCreditScore = StringConstant.ExcellentCreditScore
            };
            List<EntityRelationshipMapping> relationMappings = GetEntityRelationshipMappings();
            List<Consent> consents = new List<Consent>();
            mappings.First().LoanApplication.ProductId = Guid.NewGuid();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity
            {
                EntityFinances = new List<EntityFinance>
                {
                    new EntityFinance
                    {
                        EntityId = mappings.First().EntityId,
                        FinancialStatement= new FinancialStatement
                        {
                            Name=StringConstant.IncomeStatement
                        },
                        EntityFinanceYearlyMappings=new List<EntityFinanceYearlyMapping>
                        {
                            new EntityFinanceYearlyMapping
                            {
                                Id=Guid.NewGuid()
                            }
                        }
                    }
                },
                EntityTaxForms = new List<EntityTaxForm> { new EntityTaxForm { EntityId = mappings.First().EntityId } }
            };
            List<Section> sections = new List<Section>()
            {
                new Section
                {
                    Id = Guid.NewGuid(),
                    Name = StringConstant.PersonalFinancesSection
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Consent>()).Returns(consents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relationMappings.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.GetInterestRateForGivenSelfDeclaredCreditScore(It.IsAny<string>())).Returns((decimal)12.99);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Section, bool>>>()))
                .Returns(sections.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _applicationRepository.SaveLoanConsentOfUserAsync(loanApplicationId, _currentUserAC));
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<LoanApplication>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Never);
        }

        /// <summary>
        /// Test method to verify that if consent are found in table then save the consent for given user.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_ConsentTextsFound_PerformsAddOperation()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            mappings.First().LoanApplication.CreatedByUser = new User
            {
                Id = _currentUserAC.Id,
                Email = _currentUserAC.Email,
                SelfDeclaredCreditScore = StringConstant.ExcellentCreditScore
            };
            List<EntityRelationshipMapping> relationMappings = GetEntityRelationshipMappings();
            relationMappings.AddRange(GetEntityRelationshipMappings());
            List<Consent> consents = GetConsents();
            mappings.First().LoanApplication.ProductId = Guid.NewGuid();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity
            {
                EntityFinances = new List<EntityFinance>
                {
                    new EntityFinance
                    {
                        EntityId = mappings.First().EntityId,
                        FinancialStatement= new FinancialStatement
                        {
                            Name=StringConstant.IncomeStatement
                        },
                        EntityFinanceYearlyMappings=new List<EntityFinanceYearlyMapping>
                        {
                            new EntityFinanceYearlyMapping
                            {
                                Id=Guid.NewGuid()
                            }
                        }
                    }
                },
                EntityTaxForms = new List<EntityTaxForm> { new EntityTaxForm { EntityId = mappings.First().EntityId } }
            };
            List<Section> sections = new List<Section>()
            {
                new Section
                {
                    Id = Guid.NewGuid(),
                    Name = StringConstant.PersonalFinancesSection
                }
            };
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Consent>()).Returns(consents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relationMappings.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("EmailService:IsEmailServiceEnabled").Value).Returns("false");
            _globalRepositoryMock.Setup(x => x.GetInterestRateForGivenSelfDeclaredCreditScore(It.IsAny<string>())).Returns((decimal)12.99);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Section, bool>>>()))
                .Returns(sections.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("false");

            //Act
            await _applicationRepository.SaveLoanConsentOfUserAsync(loanApplicationId, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<LoanApplication>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Exactly(1));
        }

        /// <summary>
        /// Test method to verify that if loan initiator has not given a consent then before that no any shareholder can give consent.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_LoanInitiatorNotGivenConsent_ThrowsInvalidOperationException()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity();
            mappings.First().LoanApplication.CreatedByUserId = Guid.NewGuid();
            List<EntityRelationshipMapping> relationMappings = GetEntityRelationshipMappings();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relationMappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _applicationRepository.SaveLoanConsentOfUserAsync(loanApplicationId, _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that if the loan initiator is not giving the consent then don't call the email sending method.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_InitiatorNotGivingConsent_NoEmailSendingMethodCall()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = entityConsents.First().ConsenteeId;
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity();
            List<EntityRelationshipMapping> relationMappings = GetEntityRelationshipMappings();
            relationMappings.AddRange(GetEntityRelationshipMappings());
            List<Consent> consents = GetConsents();
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Consent>()).Returns(consents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relationMappings.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("EmailService:IsEmailServiceEnabled").Value).Returns("true");

            //Act
            await _applicationRepository.SaveLoanConsentOfUserAsync(loanApplicationId, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<List<LoanApplicationSnapshot>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Once);
        }

        /// <summary>
        /// Test method to verify that if no any consent given till now then dont send an email.
        /// </summary>
        [Fact]
        public async Task SendReminderEmailToShareHoldersAsync_NoAnyConsentGiven_DontSendEmail()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);

            //Act
            await _applicationRepository.SendReminderEmailToShareHoldersAsync(loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Never);
        }

        /// <summary>
        /// Test method to verify that if no any entity loan mapping found then dont send an email.
        /// </summary>
        [Fact]
        public async Task SendReminderEmailToShareHoldersAsync_NoEntityLoanMappingFound_DontSendEmail()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            List<EntityLoanApplicationMapping> mappings = new List<EntityLoanApplicationMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);

            //Act
            await _applicationRepository.SendReminderEmailToShareHoldersAsync(loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Never);
        }

        /// <summary>
        /// Test method to verify that if no any entity relation mapping found then dont send an email.
        /// </summary>
        [Fact]
        public async Task SendReminderEmailToShareHoldersAsync_NoEntityRelationMappingFound_DontSendEmail()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> relatives = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relatives.AsQueryable().BuildMock().Object);

            //Act
            await _applicationRepository.SendReminderEmailToShareHoldersAsync(loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Once);
        }

        /// <summary>
        /// Test method to verify that it throws error when user is not authorized to update the loan application.
        /// </summary>
        [Fact]
        public async Task LockLoanApplicationByIdAsync_UserIsNotAuthorized_ThrowsExceptionUnauthorizedAccess()
        {
            //Arrange
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _applicationRepository.LockLoanApplicationByIdAsync(Guid.NewGuid(), new List<EntityAC>(), _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that it throws error when add or update operation is not allowed to lock the application.
        /// </summary>
        [Fact]
        public async Task LockLoanApplicationByIdAsync_AddOperationNotAllowed_ThrowsExceptionInvalidOperation()
        {
            //Arrange
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _applicationRepository.LockLoanApplicationByIdAsync(Guid.NewGuid(), new List<EntityAC>(), _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that it throws an error if loan application doesn't exists for given id in argument.
        /// </summary>
        [Fact]
        public async Task LockLoanApplicationByIdAsync_LoanApplicationNotExists_ThrowsErrorNoUpdateOperation()
        {
            //Arrange
            LoanApplication loanApplication = null;
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).ReturnsAsync(loanApplication);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _applicationRepository.LockLoanApplicationByIdAsync(It.IsAny<Guid>(), new List<EntityAC>(), _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that exception will be thrown if entity loan application mapping is not found.
        /// </summary>
        [Fact]
        public async Task LockLoanApplicationByIdAsync_EntityLoanMappingNotFound_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            LoanApplication loanApplication = GetLoanApplicationObject();
            loanApplication.Id = loanApplicationId;
            List<EntityLoanApplicationMapping> mappings = new List<EntityLoanApplicationMapping>();

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).ReturnsAsync(loanApplication);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _applicationRepository.LockLoanApplicationByIdAsync(loanApplicationId, new List<EntityAC>(), _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that if all the relatives have not given the consent then do not lock the application.
        /// </summary>
        [Fact]
        public async Task LockLoanApplicationByIdAsync_AllRelativesNotGivenConsent_DontLockApplication()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            LoanApplication loanApplication = GetLoanApplicationObject();
            loanApplication.Id = loanApplicationId;
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            List<EntityRelationshipMapping> relationMappings = GetEntityRelationshipMappings();
            List<EntityLoanApplicationConsent> entityConsents = new List<EntityLoanApplicationConsent>();

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).ReturnsAsync(loanApplication);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _applicationRepository.LockLoanApplicationByIdAsync(loanApplicationId, new List<EntityAC>(), _currentUserAC));
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<List<LoanApplicationSnapshot>>()), Times.Never);
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), _currentUserAC), Times.Never);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>(), _currentUserAC), Times.Never);
        }

        /// <summary>
        /// Test method to verify that if no any relatives found then do not lock the application.
        /// </summary>
        [Fact]
        public async Task LockLoanApplicationByIdAsync_NoRelativesFound_DontLockApplication()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            LoanApplication loanApplication = GetLoanApplicationObject();
            loanApplication.Id = loanApplicationId;
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            List<EntityRelationshipMapping> relationMappings = new List<EntityRelationshipMapping>();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).ReturnsAsync(loanApplication);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _applicationRepository.LockLoanApplicationByIdAsync(loanApplicationId, new List<EntityAC>(), _currentUserAC));
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<List<LoanApplicationSnapshot>>()), Times.Never);
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), _currentUserAC), Times.Never);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>(), _currentUserAC), Times.Never);
        }

        /// <summary>
        /// Test method to verify that if linked relatives and consents given count is not matched then do not lock the application.
        /// </summary>
        [Fact]
        public async Task LockLoanApplicationByIdAsync_RelativesAndConsentCountNotMatched_DontLockApplication()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            LoanApplication loanApplication = GetLoanApplicationObject();
            loanApplication.Id = loanApplicationId;
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            List<EntityRelationshipMapping> relationMappings = GetEntityRelationshipMappings();
            relationMappings.AddRange(GetEntityRelationshipMappings());
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).ReturnsAsync(loanApplication);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _applicationRepository.LockLoanApplicationByIdAsync(loanApplicationId, new List<EntityAC>(), _currentUserAC));
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<List<LoanApplicationSnapshot>>()), Times.Never);
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), _currentUserAC), Times.Never);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>(), _currentUserAC), Times.Never);
        }

        /// <summary>
        /// Test method to verify that if all the relatives have given the consents then it locks the application and updates its section and status.
        /// </summary>
        [Fact]
        public async Task LockLoanApplicationByIdAsync_RelativesGivenConsents_LockApplication()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<LoanApplication> loanApplications = new List<LoanApplication> { GetLoanApplicationObject() };
            loanApplications.First().Id = loanApplicationId;
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().LoanApplication.CreatedByUserId = _currentUserAC.Id;
            List<EntityRelationshipMapping> relationMappings = GetEntityRelationshipMappings();
            List<EntityLoanApplicationConsent> entityConsents = GetEntityLoanApplicationConsentList();

            List<Guid> entityIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            List<EntityRelationshipMapping> entityRelationsMappings = GetListOfRelationMappingsForApplicationDetails();
            entityRelationsMappings.RemoveRange(2, 2);
            entityRelationsMappings.ElementAt(0).PrimaryEntityId = entityIds.First();
            entityRelationsMappings.ElementAt(0).RelativeEntityId = entityIds.ElementAt(1);
            entityRelationsMappings.ElementAt(1).PrimaryEntityId = entityIds.First();
            entityRelationsMappings.ElementAt(1).RelativeEntityId = entityIds.ElementAt(2);
            var entityFinances = new List<EntityFinance>
            {
                new EntityFinance { EntityId = Guid.NewGuid(), LoanApplicationId = loanApplicationId, Version = Guid.NewGuid(), SurrogateId = 1 }
            };
            mappings.First().Entity.PrimaryEntityRelationships.First().RelativeEntity.EntityFinances.First().EntityId = entityFinances.First().EntityId;
            var entities = new List<EntityAC>
            {
                new EntityAC
                {
                    Id = entityFinances.First().EntityId,
                    PersonalFinance = new PersonalFinanceAC
                    {
                        Id = Guid.NewGuid(),
                        Accounts = new List<PersonalFinanceAccountAC>(),
                        FinancialStatement = "PersonalFinance",
                        Summary = new PersonalFinanceSummaryAC { Accounts = new List<PersonalFinanceAccountSummaryAC>() }
                    }
                }
            };

            List<EntityTaxYearlyMapping> entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() { new EntityTaxYearlyMapping() { Period = "2019", EntityTaxFormId = entityRelationsMappings.First().PrimaryEntity.EntityTaxForms.First().Id, UploadedDocument = new Document() { Id = Guid.NewGuid(), Name = "XYZ", Path = "XYZ" } }, new EntityTaxYearlyMapping() { Period = "2018", EntityTaxFormId = entityRelationsMappings.First().PrimaryEntity.EntityTaxForms.First().Id, UploadedDocument = new Document() { Id = Guid.NewGuid(), Name = "XYZ", Path = "XYZ" } } };

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .ReturnsAsync(loanApplications.First()).ReturnsAsync(loanApplications.First());
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.SetupSequence(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(mappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(relationMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationsMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(entityFinances.AsQueryable().BuildMock().Object);

            //Act
            await _applicationRepository.LockLoanApplicationByIdAsync(loanApplicationId, entities, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LoanApplicationSnapshot>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<EntityFinance>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(3));
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), _currentUserAC), Times.Once);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>(), _currentUserAC), Times.Once);
        }
        #endregion

        /// <summary>
        /// Test method to verify that if current user is not banker then don't allow to get the loan list.
        /// </summary>
        [Fact]
        public async Task GetAllLoanApplicationsAsync_NotBankUser_ThrowsInvalidResourceAccessException()
        {
            //Arrange

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _applicationRepository.GetAllLoanApplicationsAsync(null, _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that if no any application exists in db then returns an empty list.
        /// </summary>
        [Fact]
        public async Task GetAllLoanApplicationsAsync_NoApplicationExists_ReturnsEmptyList()
        {
            //Arrange
            _currentUserAC.IsBankUser = true;
            List<LoanApplication> applications = new List<LoanApplication>();
            _dataRepositoryMock.Setup(x => x.GetAll<LoanApplication>()).Returns(applications.AsQueryable().BuildMock().Object);

            //Act
            PagedLoanApplicationsAC actual = await _applicationRepository.GetAllLoanApplicationsAsync(null, _currentUserAC);

            //Assert
            Assert.Empty(actual.Applications);
            Assert.Equal(applications.Count, actual.TotalApplicationsCount);
        }

        /// <summary>
        /// Test method to verify that if no any filter exists in request then it returns all the applications without filtering.
        /// </summary>
        [Fact]
        public async Task GetAllLoanApplicationsAsync_NoFilterExists_ReturnAllLoans()
        {
            //Arrange
            _currentUserAC.IsBankUser = true;
            List<LoanApplication> applications = new List<LoanApplication>
            {
                GetLoanApplicationObject(),
                GetLoanApplicationObject()
            };
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity { Company = new Company { Id = Guid.NewGuid(), Name = "ABC" } };
            foreach (var application in applications)
            {
                application.EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping> { mappings.First() };
            }
            _dataRepositoryMock.Setup(x => x.GetAll<LoanApplication>()).Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(mappings.AsQueryable().BuildMock().Object);

            //Act
            PagedLoanApplicationsAC actual = await _applicationRepository.GetAllLoanApplicationsAsync(null, _currentUserAC);

            //Assert
            Assert.Equal(actual.Applications.Count, applications.Count);
        }

        /// <summary>
        /// Test method to verify that if application number/created date/amount/status/purpose filter exists in request then it returns filtered list.
        /// </summary>
        [Theory]
        [InlineData(StringConstant.FilterApplicationNumber, "GHI3568393", "LoanApplicationNumber")]
        [InlineData(StringConstant.FilterCreatedDate, "13-07-2020", "CreatedOn")]
        [InlineData(StringConstant.FilterLoanAmount, "248503", "LoanAmount")]
        [InlineData(StringConstant.FilterLoanStatus, "5", "Status")]
        [InlineData(StringConstant.FilterLoanPurpose, "Cash Flow", "Name")]
        public async Task GetAllLoanApplicationsAsync_FilterExists_ReturnsFilteredList(string fieldName, string fieldValue, string propertyName)
        {
            //Arrange
            _currentUserAC.IsBankUser = true;
            List<LoanApplication> applications = new List<LoanApplication>
            {
                GetLoanApplicationObject(),
                GetLoanApplicationObject()
            };
            if (propertyName.ToLowerInvariant().Contains("created"))
            {
                string date = fieldValue.Split('T')[0];
                int day = int.Parse(date.Split('-')[0]);
                int month = int.Parse(date.Split('-')[1]);
                int year = int.Parse(date.Split('-')[2]);
                applications.First().GetType().GetProperty(propertyName).SetValue(applications.First(), new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc));
            }
            else if (propertyName.ToLowerInvariant().Contains("status"))
            {
                applications.First().GetType().GetProperty(propertyName).SetValue(applications.First(), (LoanApplicationStatusType)Enum.Parse(typeof(LoanApplicationStatusType), fieldValue));
            }
            else if (propertyName.ToLowerInvariant().Contains("amount"))
            {
                applications.First().GetType().GetProperty(propertyName).SetValue(applications.First(), Convert.ToDecimal(fieldValue));
            }
            else if (propertyName.ToLowerInvariant().Contains("number"))
            {
                applications.First().GetType().GetProperty(propertyName).SetValue(applications.First(), fieldValue);
            }
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity { Company = new Company { Id = Guid.NewGuid(), Name = "ABC" } };
            foreach (var application in applications)
            {
                application.EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping> { mappings.First() };
            }
            if (propertyName.ToLowerInvariant().Contains("name"))
            {
                applications.First().LoanPurpose.Name = fieldValue;
            }
            string filterString = "[{\"Field\":\"\", \"Operator\":\"=\", \"Value\":\"\"}]";

            FilterModelAC filter;
            if (propertyName.ToLowerInvariant().Contains("name"))
            {
                filter = new FilterModelAC
                {
                    Filter = string.Format("{0}{1}{2}{3}{4}", filterString.Substring(0, 11), fieldName, filterString.Substring(11, 28),
                    applications.First().LoanPurpose.GetType().GetProperty(propertyName).GetValue(applications.First().LoanPurpose, null).ToString(), filterString.Substring(39, 3))
                };
            }
            else if (propertyName.ToLowerInvariant().Contains("status"))
            {
                filter = new FilterModelAC
                {
                    Filter = string.Format("{0}{1}{2}{3}{4}", filterString.Substring(0, 11), fieldName, filterString.Substring(11, 28),
                    ((LoanApplicationStatusType)Enum.Parse(typeof(LoanApplicationStatusType), fieldValue)).ToString(), filterString.Substring(39, 3))
                };
            }
            else
            {
                filter = new FilterModelAC
                {
                    Filter = string.Format("{0}{1}{2}{3}{4}", filterString.Substring(0, 11), fieldName, filterString.Substring(11, 28),
                    fieldValue, filterString.Substring(39, 3))
                };
            }

            _dataRepositoryMock.Setup(x => x.GetAll<LoanApplication>()).Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(mappings.AsQueryable().BuildMock().Object);

            //Act
            PagedLoanApplicationsAC actual = await _applicationRepository.GetAllLoanApplicationsAsync(filter, _currentUserAC);

            //Assert
            Assert.Single(actual.Applications);
            if (propertyName.ToLowerInvariant().Contains("name"))
            {
                Assert.NotEqual(actual.Applications.First().BasicDetails.LoanPurposeId, Guid.Empty);
            }
            else
            {
                Assert.Equal(applications.First().GetType().GetProperty(propertyName).GetValue(applications.First(), null).ToString(),
                actual.Applications.First().BasicDetails.GetType().GetProperty(propertyName).GetValue(actual.Applications.First().BasicDetails, null).ToString());
            }
        }

        /// <summary>
        /// Test method to verify that if company name filter exists in request then it returns filtered list.
        /// </summary>
        [Fact]
        public async Task GetAllLoanApplicationsAsync_CompanyNameFilterExists_ReturnsFilteredList()
        {
            //Arrange
            _currentUserAC.IsBankUser = true;
            List<LoanApplication> applications = new List<LoanApplication>
            {
                GetLoanApplicationObject(),
                GetLoanApplicationObject()
            };
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity { Company = new Company { Id = Guid.NewGuid(), Name = "ABC" } };
            mappings.First().LoanApplicationId = applications.First().Id;
            applications.First().EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping> { mappings.First() };
            applications.Last().EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping>();
            string filterString = "[{\"Field\":\"\", \"Operator\":\"=\", \"Value\":\"\"}]";
            FilterModelAC filter = new FilterModelAC { Filter = string.Format("{0}{1}{2}{3}{4}", filterString.Substring(0, 11), StringConstant.FilterCompanyName, filterString.Substring(11, 28), mappings.First().Entity.Company.Name, filterString.Substring(39, 3)) };
            _dataRepositoryMock.Setup(x => x.GetAll<LoanApplication>()).Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(mappings.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("somejson");
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("somearray");
            var oneSectionMock = new Mock<IConfigurationSection>();
            oneSectionMock.Setup(s => s.Value).Returns("somemail");
            _configurationMock.Setup(x => x.GetChildren()).Returns(new List<IConfigurationSection> { oneSectionMock.Object });
            //Act
            PagedLoanApplicationsAC actual = await _applicationRepository.GetAllLoanApplicationsAsync(filter, _currentUserAC);

            //Assert
            Assert.Single(actual.Applications);
            Assert.Equal(mappings.First().Entity.Company.Name, actual.Applications.First().BorrowingEntities.First().Company.Name);
        }

        /// <summary>
        /// Test method to verify that if sorting by date is given then it returns sorted list 
        /// otherwise returns the list in its same order.
        /// </summary>
        [Theory]
        [InlineData(StringConstant.FilterCreatedDate)]
        [InlineData(StringConstant.FilterCompanyName)]
        public async Task GetAllLoanApplicationsAsync_SortByDate_ReturnsSortedList(string filerField)
        {
            //Arrange
            _currentUserAC.IsBankUser = true;
            List<LoanApplication> applications = new List<LoanApplication>
            {
                GetLoanApplicationObject(),
                GetLoanApplicationObject()
            };
            applications.First().CreatedOn = DateTime.UtcNow;
            applications.Last().CreatedOn = DateTime.UtcNow.AddDays(-1);
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity { Company = new Company { Id = Guid.NewGuid(), Name = "ABC" } };
            applications.First().EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping> { mappings.First() };
            applications.Last().EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping>();
            FilterModelAC filter = new FilterModelAC { SortField = filerField, SortBy = "asc" };
            _dataRepositoryMock.Setup(x => x.GetAll<LoanApplication>()).Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(mappings.AsQueryable().BuildMock().Object);
            var mappedList = new List<ApplicationAC>
            {
                new ApplicationAC { BasicDetails = _mapper.Map<ApplicationBasicDetailAC>(applications.Last()) },
                new ApplicationAC
                {
                    BasicDetails = _mapper.Map<ApplicationBasicDetailAC>(applications.First()),
                    BorrowingEntities = new List<EntityAC> { new EntityAC { Id = Guid.NewGuid(), Company = new CompanyAC { Name = "ABC" } } }
                }
            };

            //Act
            PagedLoanApplicationsAC actual = await _applicationRepository.GetAllLoanApplicationsAsync(filter, _currentUserAC);

            //Assert
            Assert.Equal(actual.Applications.Count, applications.Count);
            if (!filerField.Equals(StringConstant.FilterCreatedDate))
            {
                Assert.NotEqual(actual.Applications.First().BasicDetails.CreatedOn, mappedList.First().BasicDetails.CreatedOn);
                Assert.Equal(actual.Applications.First().BasicDetails.CreatedOn, mappedList.Last().BasicDetails.CreatedOn);
            }
            else
            {
                Assert.Equal(actual.Applications.First().BasicDetails.CreatedOn, mappedList.First().BasicDetails.CreatedOn);
                Assert.NotEqual(actual.Applications.First().BasicDetails.CreatedOn, mappedList.Last().BasicDetails.CreatedOn);
            }
        }

        /// <summary>
        /// Test method to verify that if pagination is required then it returns paginated list.
        /// </summary>
        [Fact]
        public async Task GetAllLoanApplicationsAsync_PaginationRequired_ReturnsPaginatedList()
        {
            //Arrange
            _currentUserAC.IsBankUser = true;
            List<LoanApplication> applications = new List<LoanApplication>
            {
                GetLoanApplicationObject(),
                GetLoanApplicationObject()
            };
            applications.First().CreatedOn = DateTime.UtcNow;
            applications.Last().CreatedOn = DateTime.UtcNow.AddDays(-1);
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity { Company = new Company { Id = Guid.NewGuid(), Name = "ABC" } };
            applications.First().EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping> { mappings.First() };
            applications.Last().EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping>();
            FilterModelAC filter = new FilterModelAC { PageNo = 0, PageRecordCount = 1 };
            _dataRepositoryMock.Setup(x => x.GetAll<LoanApplication>()).Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(mappings.AsQueryable().BuildMock().Object);

            //Act
            PagedLoanApplicationsAC actual = await _applicationRepository.GetAllLoanApplicationsAsync(filter, _currentUserAC);

            //Assert
            Assert.Single(actual.Applications);
            Assert.Equal(applications.Count, actual.TotalApplicationsCount);
        }


        /// <summary>
        /// Test method to verify that if no application left in the list after filteration then it returns empty list.
        /// </summary>
        [Fact]
        public async Task GetAllLoanApplicationsAsync_NoApplicationLeftAfterFilteration_ReturnsEmptyList()
        {
            //Arrange
            _currentUserAC.IsBankUser = true;
            List<LoanApplication> applications = new List<LoanApplication>
            {
                GetLoanApplicationObject(),
                GetLoanApplicationObject()
            };
            List<EntityLoanApplicationMapping> mappings = GetEntityLoanApplicationMappingList();
            mappings.First().Entity = new DomainModel.Models.EntityInfo.Entity { Company = new Company { Id = Guid.NewGuid(), Name = "ABC" } };
            foreach (var application in applications)
            {
                application.EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping> { mappings.First() };
            }
            FilterModelAC filter = new FilterModelAC
            {
                Filters = new List<FilterAC> { new FilterAC { Field = StringConstant.FilterApplicationNumber,
                Operator = StringConstant.FilterEqualOperator, Value = "GHI3568393" }}
            };
            _dataRepositoryMock.Setup(x => x.GetAll<LoanApplication>()).Returns(applications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(mappings.AsQueryable().BuildMock().Object);

            //Act
            PagedLoanApplicationsAC actual = await _applicationRepository.GetAllLoanApplicationsAsync(filter, _currentUserAC);

            //Assert
            Assert.Empty(actual.Applications);
        }

        /// <summary>
        /// Test method to verify that invalid resource access exception is thrown when unauthorized email attempts to delete db
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteLoanApplicationsAsync_UnauthorizedAccess_ThrowsInvalidAccessException()
        {
            // Arrange

            var oneSectionMock = new Mock<IConfigurationSection>();
            oneSectionMock.Setup(s => s.Value).Returns("somemail@email.com");
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(oneSectionMock.Object);

            _configurationMock.Setup(x => x.GetChildren()).Returns(new List<IConfigurationSection> { oneSectionMock.Object });

            var currentUser = new CurrentUserAC
            {
                Email = "someunauthorizedemail@email.com"
            };

            //Act
            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _applicationRepository.DeleteLoanApplicationsAsync(currentUser));

        }

        /// <summary>
        /// Method to verify if the data deletes without any exception when authorized user does it
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteLoanApplicationsAsync_AuthorizedAccess_ExecutesSuccessfully()
        {
            // Arrange
            Dictionary<string, string> inMemorySettings =
    new Dictionary<string, string> {
        {"BankPreference:DbResetAuthorizedEmails", "[\"somemail@email.com\"]"},
        {"BankPreference:DbResetAuthorizedEmails:0","somemail@email.com" }
    };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _configurationMock.Setup(x => x.GetSection("BankPreference:DbResetAuthorizedEmails").GetChildren()).Returns(configuration.GetSection("BankPreference:DbResetAuthorizedEmails").GetChildren());
            _dataRepositoryMock.Setup(x => x.GetAll<LoanApplication>()).Returns(new List<LoanApplication>()
            {
                new LoanApplication()
                {
                    Id=Guid.NewGuid(),
                    LoanAmount=100
                },
                new LoanApplication()
                {
                    Id=Guid.NewGuid(),
                    LoanAmount=100
                },
            }.AsQueryable());

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.RemoveRange(It.IsAny<IEnumerable<LoanApplication>>())).Verifiable();


            var currentUser = new CurrentUserAC
            {
                Email = "somemail@email.com"
            };

            //Act
            await _applicationRepository.DeleteLoanApplicationsAsync(currentUser);
            //Assert
            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<LoanApplication>>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.GetAll<LoanApplication>(), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(1));

        }
        #endregion
    }
}
