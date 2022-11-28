using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using LendingPlatform.Utils.Utils.Transunion;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using EntityModel = LendingPlatform.DomainModel.Models.EntityInfo.Entity;
namespace LendingPlatform.Repository.Test.Entity
{
    [Collection("Register Dependency")]
    public class EntityRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly IEntityRepository _entityRepository;
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IConfiguration> _configuration;
        private readonly User _loggedInUser;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly CurrentUserAC _currentUserAC;
        private readonly Mock<ISmartyStreetsUtility> _smartyStreetUtilityMock;
        private readonly Mock<IFileOperationsUtility> _fileOperationsUtility;
        private readonly Mock<IExperianUtility> _experianUtilityMock;
        private readonly Mock<IEquifaxUtility> _equifaxUtilityMock;
        private readonly Mock<ITransunionUtility> _transunionUtilityMock;
        private readonly Mock<IAmazonServicesUtility> _amazonServiceUtility;

        #endregion

        #region Constructor
        public EntityRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _entityRepository = bootstrap.ServiceProvider.GetService<IEntityRepository>();
            _configuration = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();

            _experianUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IExperianUtility>>();
            _equifaxUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IEquifaxUtility>>();
            _transunionUtilityMock = bootstrap.ServiceProvider.GetService<Mock<ITransunionUtility>>();
            _amazonServiceUtility = bootstrap.ServiceProvider.GetService<Mock<IAmazonServicesUtility>>();

            _dataRepositoryMock.Reset();
            _globalRepositoryMock.Reset();
            _experianUtilityMock.Reset();
            _equifaxUtilityMock.Reset();
            _transunionUtilityMock.Reset();
            _amazonServiceUtility.Reset();
            _loggedInUser = FetchLoggedInUser();
            _currentUserAC = FetchLoggedInUserAC();
            _smartyStreetUtilityMock = bootstrap.ServiceProvider.GetService<Mock<ISmartyStreetsUtility>>();
            _fileOperationsUtility = bootstrap.ServiceProvider.GetService<Mock<IFileOperationsUtility>>();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get UserInfoAC object
        /// </summary>
        /// <returns></returns>
        private UserInfoAC GetUserInfo()
        {
            return new UserInfoAC()
            {
                ConsumerCreditReportResponse = @"",
                Score = 750,
                Bankruptcy = false
            };
        }

        /// <summary>
        /// Get List of entityLoanApplicationMapping
        /// </summary>
        /// <returns></returns>
        private List<EntityLoanApplicationMapping> GetEntityLoanApplicationMappingList()
        {
            return new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = Guid.NewGuid()
                }
            };
        }

        /// <summary>
        /// Get validated address of smarty streets
        /// </summary>
        /// <returns>Returns object of SmartyStreets.USStreetApi.Candidate</returns>
        private SmartyStreets.USStreetApi.Candidate GetValidatedAdress()
        {
            return new SmartyStreets.USStreetApi.Candidate()
            {
                Components = new SmartyStreets.USStreetApi.Components
                {
                    PrimaryNumber = "86",
                    StreetName = "frontage",
                    CityName = "belmont",
                    State = "california",
                    ZipCode = "22001"
                }
            };
        }

        /// <summary>
        /// Get EntityAC object
        /// </summary>
        /// <returns></returns>
        private EntityAC GetEntity()
        {
            return new EntityAC
            {
                Id = null,
                RelationMapping = null,
                Address = new AddressAC
                {
                    Id = Guid.NewGuid(),
                    City = "XYZ",
                    IntegratedServiceConfigurationId = Guid.NewGuid(),
                    PrimaryNumber = "86",
                    SecondaryNumber = "21",
                    SecondaryDesignator = "ASD",
                    StateAbbreviation = "GH",
                    StreetLine = "LMN BV",
                    StreetSuffix = "HJ",
                    ZipCode = "23568"
                },
                Company = new CompanyAC
                {
                    CompanyStructure = new CompanyStructureAC
                    {
                        Id = Guid.NewGuid(),
                        Structure = StringConstant.Proprietorship,
                        Order = 1
                    },
                    CompanySize = new CompanySizeAC
                    {
                        Id = Guid.NewGuid(),
                        Size = StringConstant.ZeroToTen,
                        Order = 1
                    },
                    BusinessAge = new BusinessAgeAC
                    {
                        Id = Guid.NewGuid(),
                        Age = StringConstant.SixMonthToOneYear,
                        Order = 1
                    },
                    IndustryExperience = new IndustryExperienceAC
                    {
                        Id = Guid.NewGuid(),
                        Experience = StringConstant.IndustryExperienceZeroToFiveYears,
                        Order = 1
                    },
                    IndustryType = new IndustryTypeAC
                    {
                        Id = Guid.NewGuid(),
                        IndustryCode = "1234",
                        IndustryType = "Forstry"
                    },
                    CIN = "123456789",
                    CompanyFiscalYearStartMonth = 1,
                    Name = "Google",
                    CompanyRegisteredState = "Texas"
                },
                User = new UserAC
                {
                    DOB = DateTime.Now,
                    Email = "arjun@promactinfo.com",
                    FirstName = "Arjunsinh",
                    MiddleName = "",
                    LastName = "Jadeja",
                    HasAnyJudgementsSelfDeclared = false,
                    HasBankruptcySelfDeclared = false,
                    Phone = "232332321",
                    SelfDeclaredCreditScore = "701-750",
                    SSN = "456123789"
                }
            };
        }

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
        /// Get CurrentUserAC object
        /// </summary>
        /// <returns></returns>
        private CurrentUserAC GetCurrentUser()
        {
            return new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                Email = "arjun@promactinfo.com",
                IsBankUser = false,
                Name = "Arjun"
            };
        }

        /// <summary>
        /// Get IntegratedServiceConfiguration object
        /// </summary>
        /// <returns></returns>
        private List<IntegratedServiceConfiguration> GetIntegratedServiceConfiguration()
        {
            return new List<IntegratedServiceConfiguration>()
            {
                new IntegratedServiceConfiguration
                {
                    Id = Guid.NewGuid(),
                    IsServiceEnabled = true,
                    Name = StringConstant.SmartyStreets,
                    ConfigurationJson = @"{ }"
                }
            };
        }

        /// <summary>
        /// Fetch NAICSIndustryType object.
        /// </summary>
        /// <returns></returns>
        private NAICSIndustryType GetNAICSIndustryType()
        {
            return new NAICSIndustryType
            {
                Id = Guid.NewGuid(),
                IndustryCode = "2121",
                IndustryType = "Forestry",
                NAICSParentSectorId = Guid.NewGuid()
            };
        }

        /// <summary>
        /// Fetch List of Proprietor Relationships object.
        /// </summary>
        /// <returns></returns>
        private List<Relationship> GetProprietorRelationships()
        {
            return new List<Relationship>()
            {
                new Relationship
                {
                    Id = Guid.NewGuid(),
                    Relation = StringConstant.Proprietor
                }
            };
        }

        /// <summary>
        /// Fetch List of company object.
        /// </summary>
        /// <returns></returns>
        private List<Company> GetCompanies()
        {
            return new List<Company>()
            {
                new Company
                {
                    Id = Guid.NewGuid(),
                    CIN = "1232331",
                    BusinessAgeId = Guid.NewGuid(),
                    CompanyStructureId = Guid.NewGuid(),
                    CompanyRegisteredState = "Texas",
                    CompanySizeId = Guid.NewGuid(),
                    IndustryExperienceId = Guid.NewGuid(),
                    NAICSIndustryTypeId = Guid.NewGuid()
                }
            };
        }

        /// <summary>
        /// Fetch Address object.
        /// </summary>
        /// <returns></returns>
        private Address GetAddress()
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
        /// Fetch List of User object.
        /// </summary>
        /// <returns></returns>
        private List<User> GetUsers()
        {
            return new List<User>()
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    SSN = "123456"
                }
            };
        }

        /// <summary>
        /// Fetch List of Entity object.
        /// </summary>
        /// <returns></returns>
        private List<EntityModel> GetEntities()
        {
            return new List<EntityModel>()
            {
                new EntityModel
                    {
                        Id = Guid.NewGuid(),
                        AddressId = Guid.NewGuid(),
                        Address = new Address
                        {
                            Id = Guid.NewGuid(),
                            City = "ASD",
                            IntegratedServiceConfigurationId = Guid.NewGuid(),
                            PrimaryNumber = "12",
                            Response = @"",
                            SecondaryNumber ="12",
                            StateAbbreviation = "as",
                            SecondaryDesignator = "JF",
                            StreetLine = "TRC",
                            StreetSuffix = "DC",
                            ZipCode = "15361"
                        },
                        Type = EntityType.Company,
                        Company = new Company
                        {
                            Id = Guid.NewGuid(),
                            IndustryExperienceId = Guid.NewGuid(),
                            IndustryExperience = new IndustryExperience
                            {
                                Id = Guid.NewGuid(),
                                Experience = "xyz",
                                Order = 1
                            },
                            NAICSIndustryTypeId = Guid.NewGuid(),
                            NAICSIndustryType = new NAICSIndustryType
                            {
                                Id = Guid.NewGuid(),
                                IndustryCode = "2231",
                                IndustryType = "Foerstry",
                                NAICSParentSectorId = Guid.NewGuid()
                            },
                            BusinessAgeId = Guid.NewGuid(),
                            BusinessAge = new BusinessAge
                            {
                                Age = "21",
                                Id = Guid.NewGuid(),
                                Order = 2
                            },
                            CIN = "456123789",
                            CompanyFiscalYearStartMonth = 1,
                            CompanyRegisteredState = "Texas",
                            CompanySizeId = Guid.NewGuid(),
                            CompanySize = new CompanySize
                            {
                                Id = Guid.NewGuid(),
                                Size = "AS",
                                Order = 1
                            },
                            CompanyStructureId = Guid.NewGuid(),
                            CompanyStructure = new CompanyStructure
                            {
                                Id = Guid.NewGuid(),
                                Structure = "CDF",
                                Order = 1
                            },
                            Name = "sd",
                            CreatedByUserId = Guid.NewGuid()
                        }
                    },
                new EntityModel
                {
                    Id = Guid.NewGuid(),
                    Type = EntityType.User,
                        Address = new Address
                        {
                            Id = Guid.NewGuid(),
                            City = "ASD",
                            IntegratedServiceConfigurationId = Guid.NewGuid(),
                            PrimaryNumber = "12",
                            Response = @"",
                            SecondaryNumber ="12",
                            StateAbbreviation = "as",
                            SecondaryDesignator = "JF",
                            StreetLine = "TRC",
                            StreetSuffix = "DC",
                            ZipCode = "15361"
                        },
                        User = new User
                        {
                            Id = Guid.NewGuid(),
                            DOB = DateTime.Now,
                            Email = "arjun@promactinfo.com",
                            FirstName = "Arjunsinh",
                            MiddleName = "",
                            LastName = "Jadeja",
                            Phone = "9638521471",
                            HasAnyJudgementsSelfDeclared = false,
                            HasBankruptcySelfDeclared = false,
                            IsRegistered = true,
                            SSN = "123456789",
                            ResidencyStatus = ResidencyStatus.USCitizen,
                            SelfDeclaredCreditScore = ">750"
                        }
                }
            };
        }
        /// <summary>
        /// Fetch entity relation object.
        /// </summary>
        /// <returns></returns>
        private EntityRelationMappingAC GetEntityRelation()
        {
            return new EntityRelationMappingAC
            {
                Id = Guid.NewGuid(),
                PrimaryEntityId = Guid.NewGuid(),
                Relation = new RelationshipAC
                {
                    Id = Guid.NewGuid(),
                    Name = "xyz"
                },
                SharePercentage = 20
            };
        }

        /// <summary>
        /// Fetch the logged in user details.
        /// </summary>
        /// <returns></returns>
        private User FetchLoggedInUser()
        {
            return new User()
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@doe.com",
                Phone = "9898989898",
                SSN = "123456789",
                SelfDeclaredCreditScore = "651-700",
                HasAnyJudgementsSelfDeclared = false,
                HasBankruptcySelfDeclared = false
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
        /// Set the user profile configuration.
        /// </summary>
        /// <param name="isAllowedJudgement">Is judgement allowed or not</param>
        /// <param name="isAllowedBankruptcy">Is bankruptcy allowed or not</param>
        /// <param name="minCreditScore">Minimum credit score</param>
        private void SetUserCreditProfileConfig(bool isAllowedJudgement, bool isAllowedBankruptcy, int minCreditScore)
        {
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns(isAllowedJudgement.ToString()).Returns(isAllowedBankruptcy.ToString()).Returns(minCreditScore.ToString());
        }

        /// <summary>
        /// Method to get list of entity additional documents
        /// </summary>
        /// <returns>List of EntityAdditionalDocument</returns>
        private List<EntityAdditionalDocument> GetEntityAdditionalDocuments()
        {
            var additionalDocumentTypeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var documentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            return new List<EntityAdditionalDocument>
            {
                new EntityAdditionalDocument
                {
                    Id = Guid.NewGuid(),
                    AdditionalDocumentTypeId = additionalDocumentTypeIds.First(),
                    AdditionalDocumentType = new AdditionalDocumentType
                    {
                        Id = additionalDocumentTypeIds.First(),
                        Type = "Certificate",
                        IsEnabled = true,
                        DocumentTypeFor = ResourceType.Company
                    },
                    DocumentId = documentIds.First(),
                    Document = new Document
                    {
                        Id = documentIds.First(),
                        Name = "ABC.pdf",
                        Path = "abc.download.com/path"
                    }
                },
                new EntityAdditionalDocument
                {
                    Id = Guid.NewGuid(),
                    AdditionalDocumentTypeId = additionalDocumentTypeIds.Last(),
                    AdditionalDocumentType = new AdditionalDocumentType
                    {
                        Id = additionalDocumentTypeIds.Last(),
                        Type = "Certificate",
                        IsEnabled = true,
                        DocumentTypeFor = ResourceType.Company
                    },
                    DocumentId = documentIds.Last(),
                    Document = new Document
                    {
                        Id = documentIds.Last(),
                        Name = "ABC.pdf",
                        Path = "abc.download.com/path"
                    }
                }
            };
        }

        /// <summary>
        /// Method to get object of EntityAC object with list of additional documents AC objects
        /// </summary>
        /// <returns>EntityAC object</returns>
        private EntityAC GetEntityACWithAdditionalDocumentACList()
        {
            return new EntityAC
            {
                AdditionalDocuments = new List<AdditionalDocumentAC>
                {
                    new AdditionalDocumentAC
                    {
                        Id = Guid.NewGuid(),
                        Document = new DocumentAC
                        {
                            Id = Guid.NewGuid(),
                            Name = "abc.pdf",
                            Path = "temp_/abcd",
                            DownloadPath = "abc.download.com/adg"
                        },
                        DocumentType = new AdditionalDocumentTypeAC
                        {
                            Id = Guid.NewGuid(),
                            Type = "Certificate",
                            DocumentTypeFor = ResourceType.Company
                        }
                    }
                }
            };
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Check if update entity with Invalid type and throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityInvalidType_VerifyThrowsInvalidResourceAccessException()
        {
            // Arrange
            EntityAC entity = GetEntity();

            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.IsBankUser = true;
            List<EntityModel> entities = new List<EntityModel>()
            {
                new EntityModel
                {
                    Id = Guid.NewGuid(),
                    Type = (EntityType)3
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, null));
        }
        /// <summary>
        /// Check if bank user trys to add entity is not allowed and throws invalid resource access exception.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyCurrentUserIsBank_VerifyThrowsInvalidResourceAccessException()
        {
            // Arrange
            EntityAC entity = GetEntity();

            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.IsBankUser = true;

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if type is company and entity object does not contain company details throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyNull_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.Company = null;

            CurrentUserAC currentUser = GetCurrentUser();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if entity(company) has invalid CIN format throw validation exception.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyInvalidCIN_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            CurrentUserAC currentUser = GetCurrentUser();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(false);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if entity(company) has not valid industry type throw InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyNotValidIndustryType_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            CurrentUserAC currentUser = GetCurrentUser();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync((NAICSIndustryType)null);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if add entity(company) has not valid company structure throw InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyNotValidCompanyStructure_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();

            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if add entity(company) has not valid company registered state throw InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyLLCInvalidState_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();


            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            entity.Company.CompanyStructure.Id = companyStructures.Where(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Select(x => x.Id).Single();

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"California\"\r\n }\r\n]");
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if add c corporation entity(company) has not valid Company Fiscal Year Start Month throw InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyCCorpInvalidCompanyFiscalYearStartMonth_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Company.CompanyFiscalYearStartMonth = 13;

            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            entity.Company.CompanyStructure.Id = companyStructures.Where(x => x.Structure.Equals(StringConstant.CCorporation)).Select(x => x.Id).Single();

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if add entity(company) has not unique CIN throw ValidationException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyNotUniqueCIN_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();

            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();


            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(false);

            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if update entity(company) has not unique CIN throw ValidationException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityCompanyNotUniqueCIN_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            CurrentUserAC currentUser = GetCurrentUser();
            List<Company> companies = new List<Company>()
            {
                new Company
                {
                    Id = Guid.NewGuid(),
                    CompanyStructureId = Guid.NewGuid()
                }
            };
            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();


            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync(companies.Single());
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if entity(company) has not valid address throw validation exception..
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyAddressNotValid_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();

            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            SmartyStreets.USStreetApi.Candidate validAddress = null;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns(validAddress);

            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if entity(company) has not unique user SSN throw validation exception..
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyProprietorNotUniqueSSN_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();

            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            List<Relationship> relationships = GetProprietorRelationships();
            User user = new User
            {
                Id = Guid.NewGuid()
            };

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns(validAddress);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(relationships.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Once);
        }

        /// <summary>
        /// Check if entity(company) proprientorship added successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyProprietorSuccessfully_AssertAddCompany()
        {
            // Arrange
            EntityAC entity = GetEntity();

            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            List<Relationship> relationships = GetProprietorRelationships();
            List<User> users = GetUsers();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            List<Company> companies = new List<Company>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns(validAddress);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(relationships.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);

            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company");
            // Assert

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if entity(company) partnership added successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityCompanyPartnershipSuccessfully_AssertAddCompany()
        {
            // Arrange
            EntityAC entity = GetEntity();

            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            List<Relationship> relationships = GetProprietorRelationships();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns(validAddress);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(relationships.AsQueryable().BuildMock().Object);

            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company");
            // Assert

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if Update entity(company), user has not access to company throw InvalidResourceAccessException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityCompanyNoUpdateAccess_VerifyThrowsInvalidResourceAccessException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<Company> companies = new List<Company>();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if Update entity(company) invalid address id throw InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityCompanyInvalidAddressId_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<Company> companies = GetCompanies();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            List<EntityModel> entities = new List<EntityModel>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync((Address)null);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns(GetValidatedAdress());
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if Update entity(company) invalid address throw ValidationException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityCompanyInvalidAddress_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<Company> companies = GetCompanies();
            Address address = GetAddress();
            List<EntityModel> entities = new List<EntityModel>()
            {
                new EntityModel
                {
                    Id = Guid.NewGuid(),
                    Address = new Address
                        {
                            Id = Guid.NewGuid(),
                            City = "ASD",
                            IntegratedServiceConfigurationId = Guid.NewGuid(),
                            PrimaryNumber = "12",
                            Response = @"",
                            SecondaryNumber ="12",
                            StateAbbreviation = "as",
                            SecondaryDesignator = "JF",
                            StreetLine = "TRC",
                            StreetSuffix = "DC",
                            ZipCode = "15361"
                        }
                }
            };

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync((Address)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns((SmartyStreets.USStreetApi.Candidate)null);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if Update entity(company) proprientor not unique SSN throw ValidationException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityCompanyProprietorNotUniqueSSN_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<Company> companies = GetCompanies();
            Address address = GetAddress();
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            User user = new User
            {
                Id = Guid.NewGuid()
            };

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync((Address)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns(validAddress);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type.Equals(EntityType.Company)).AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company"));
        }

        /// <summary>
        /// Check if Update entity(company) proprientor Successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityCompanyProprietorshipSuccessfully_AssertUpdateCompany()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            CurrentUserAC currentUser = GetCurrentUser();

            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<Company> companies = GetCompanies();
            Address address = GetAddress();
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            List<User> users = GetUsers();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid()
                }
            };
            List<Relationship> relationships = GetProprietorRelationships();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync((Address)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns(validAddress);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type.Equals(EntityType.Company)).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(relationships.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, null);
            // Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<List<EntityRelationshipMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if Update entity(company) partnership successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityCompanyPartnershipSuccessfully_AssertUpdateCompany()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 20
                }
            };
            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<Company> companies = GetCompanies();
            Address address = GetAddress();
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            List<User> users = GetUsers();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync((Address)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns(validAddress);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
               .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company");
            // Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<List<EntityRelationshipMapping>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if Update entity(company) partnership successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityCompanyLLCSuccessfully_AssertUpdateCompany()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 20
                }
            };
            NAICSIndustryType naicsIndustry = GetNAICSIndustryType();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<Company> companies = GetCompanies();
            Address address = GetAddress();
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            List<User> users = GetUsers();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<String>()))
                .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<NAICSIndustryType, bool>>>()))
                .ReturnsAsync(naicsIndustry);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _fileOperationsUtility.SetupSequence(x => x.ReadFileContent(It.IsAny<string>()))
                  .Returns("[\r\n {\r\n   \"Name\": \"Texas\"\r\n }\r\n]");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync((Address)null);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
                .Returns(validAddress);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
               .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<String>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "Company");
            // Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<List<EntityRelationshipMapping>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if type is user and entity object does not contain company details throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityUserNull_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.User = null;

            CurrentUserAC currentUser = GetCurrentUser();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if new entity(user) has not unique phone number throw validation exception.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityNewUserNotUniquePhoneNumber_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            CurrentUserAC currentUser = GetCurrentUser();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync(GetUsers().Single());
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if new entity(user) has not unique ssn throw validation exception.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityNewUserNotUniqueSSN_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            CurrentUserAC currentUser = GetCurrentUser();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync(GetUsers().Single());
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added new entity(user) successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityNewUserSuccessfully_AssertAddUser()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            CurrentUserAC currentUser = GetCurrentUser();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People");

            // Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) with address successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityNewUserWithAddressSuccessfully_AssertAddUserAddAddress()
        {
            // Arrange
            EntityAC entity = GetEntity();
            CurrentUserAC currentUser = GetCurrentUser();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
            .Returns(validAddress);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People");

            // Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) with invalid address throw validation exception
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityNewUserWithInvalidAddressSuccessfully_VerifyThrowsValidatonException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            CurrentUserAC currentUser = GetCurrentUser();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            SmartyStreets.USStreetApi.Candidate validAddress = null;
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
            .Returns(validAddress);
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added existing entity(user) is registered with bank and not current user throws InvalidResourceAccessException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityExistingUserNoUpdateAccess_VerifyThrowsInvalidResourceAccessException()
        {
            // Arrange
            EntityAC entity = GetEntity();

            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.IsBankUser = true;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single());

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added existing entity(user) with not unique SSN throws ValidationException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityExistingUserNotUniqueSSN_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single())
                .ReturnsAsync(GetUsers().Single());
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added existing entity(user) with not unique Phone throws ValidationException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityExistingUserNotUniquePhone_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single())
                .ReturnsAsync((User)null)
                .ReturnsAsync(GetUsers().Single());
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added new email entity(user) with not unique Phone throws ValidationException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityNewEmailUserNotUniquePhone_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync(GetUsers().Single());
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added existing entity(user) successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityExistingUserSuccessfully_AssertUpdateUser()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<EntityModel> entities = new List<EntityModel>()
            {
                new EntityModel
                {
                    Id = Guid.NewGuid(),
                    Type = EntityType.User
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single())
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<Company> companies = new List<Company>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, null);
            // Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if added existing entity(user) successfully and also update all linked company CIN which are proprietorship.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityExistingUserSuccessfullyWithProprietorConpanyCIN_AssertUpdateUser()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<EntityModel> entities = new List<EntityModel>()
            {
                new EntityModel
                {
                    Id = Guid.NewGuid(),
                    Type = EntityType.User
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single())
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<Company> companies = GetCompanies();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, null);
            // Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<Company>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if update existing entity(user) with registered in bank successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityExistingUserAnIsRegisteredWithBankSuccessfully_AssertUpdateUser()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<EntityModel> entities = new List<EntityModel>()
            {
                new EntityModel
                {
                    Id = Guid.NewGuid(),
                    Type = EntityType.User
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            User user = GetUsers().Single();
            user.IsRegistered = true;
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<Company> companies = new List<Company>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, null);
            // Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if updated existing entity(user) successfully with not registered in bank.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_UpdateEntityUserNewEmailSuccessfully_AssertUpdateUser()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            entity.Address = null;
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<EntityModel> entities = new List<EntityModel>()
            {
                new EntityModel
                {
                    Id = Guid.NewGuid(),
                    Type = EntityType.User
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single())
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<Company> companies = new List<Company>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, null);
            // Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if added existing entity(user) with address successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityExistingUserWithAddressSuccessfully_AssertUpdateUserAddress()
        {
            // Arrange
            EntityAC entity = GetEntity();
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single())
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<Company> companies = new List<Company>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
            .Returns(validAddress);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type.Equals(EntityType.Company)).AsQueryable().BuildMock().Object);

            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People");
            // Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if added existing entity(user) with new address for already link address throws exception InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityExistingUserWithNewAddressForExistingAddress_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address.Id = null;
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single())
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<Company> companies = new List<Company>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
            .Returns(validAddress);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type.Equals(EntityType.Company)).AsQueryable().BuildMock().Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if updated existing entity(user) with existing address successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityExistingUserWithNewAddressForExistingAddress_AssertUpdateUserAddress()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address.Id = null;
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<EntityModel> entities = GetEntities().Where(x => x.Type.Equals(EntityType.Company)).ToList();
            entities.Single().AddressId = null;
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single())
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<Company> companies = new List<Company>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
            .Returns(validAddress);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);

            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People");
            // Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if added existing entity(user) with invalid address throw exception.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityExistingUserWithInvalidAddressSuccessfully_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.Id = GetUsers().Single().Id;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(GetUsers().Single())
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            List<Company> companies = new List<Company>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            SmartyStreets.USStreetApi.Candidate validAddress = null;
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
            .Returns(validAddress);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type.Equals(EntityType.Company)).AsQueryable().BuildMock().Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added new entity(user) successfully and add relationship mapping and has no company access throws InvalidResourceAccessException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityAddRelationMappingNoCompanyAccess_VerifyThrowsInvalidResourceAccessException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();
            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            List<Company> companies = new List<Company>();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(GetUsers().AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) successfully and add relationship mapping has proprietor company structure throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityAddRelationMappingProprietor_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();

            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 20
                }
            };
            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            List<Company> companies = GetCompanies();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) successfully and add relationship mapping has proprietor company structure throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityAddRelationMappingInvalidCompanyStructure_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            entity.RelationMapping.Id = null;
            CurrentUserAC currentUser = GetCurrentUser();

            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 90
                },
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 10
                }
            };
            List<Company> companies = GetCompanies();

            entity.Company.CompanyStructure.Id = Guid.NewGuid();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }


        /// <summary>
        /// Check if added new entity(user) successfully and add relationship mapping with majority share holder already exists throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityAddRelationMappingMajorityShareholderExists_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            entity.RelationMapping.Id = null;
            CurrentUserAC currentUser = GetCurrentUser();

            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 90
                }
            };
            List<Company> companies = GetCompanies();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) successfully and add relationship mapping with share holder already exists throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityAddRelationMappingShareholderAlreadyExists_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            entity.RelationMapping.Id = null;
            CurrentUserAC currentUser = GetCurrentUser();

            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 50
                }
            };

            entityRelationshipMappings.Single().RelativeEntityId = Guid.Empty;

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("true")
                .Returns("75");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<Company> companies = GetCompanies();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) successfully and add relationship mapping with sharepercent with make it more than 100 throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityAddRelationMappingAlready100Percent_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.RelationMapping = GetEntityRelation();
            entity.RelationMapping.Id = null;
            CurrentUserAC currentUser = GetCurrentUser();

            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 90
                },
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 10
                }
            };
            List<Company> companies = GetCompanies();

            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            SmartyStreets.USStreetApi.Candidate validAddress = GetValidatedAdress();
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()))
            .Returns(validAddress);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) successfully and update relationship mapping with invalid Id throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityUpdateRelationMappingIdInvalid_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();

            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 80
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappingsNull = new List<EntityRelationshipMapping>();
            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappingsNull.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<Company> companies = GetCompanies();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added new entity(user) successfully and update relationship mapping with share percentage total more than 100 throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityUpdateRelationMappingSharePercentageMoreThan100_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            entity.RelationMapping.SharePercentage = 90;
            CurrentUserAC currentUser = GetCurrentUser();

            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 90
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappingWithPercentage = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 10
                }
            };
            List<CompanyStructure> companyStructures = GetCompanyStructureList();

            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappingWithPercentage.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<Company> companies = GetCompanies();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added new entity(user) successfully and updated relationship mapping successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityUpdateRelationMappingSuccessfully_AsertUpdateEntityRealtionMapping()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();
            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 80
                }
            };
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<Company> companies = GetCompanies();
            companies.Single().CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People");

            // Assert

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityRelationshipMapping>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) successfully and updated relationship mapping that not exist then throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityUpdateRelationMappingDoesNotExist_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();
            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 80
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappingNull = new List<EntityRelationshipMapping>();
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<Company> companies = GetCompanies();
            companies.Single().CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappingNull.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert

            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added new entity(user) successfully and add relationship mapping successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityAddRelationMappingSuccessfully_AsertAddEntityRealtionMapping()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            entity.RelationMapping.Id = null;
            CurrentUserAC currentUser = GetCurrentUser();
            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 80
                }
            };
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<Company> companies = GetCompanies();
            companies.Single().CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act
            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People");

            // Assert

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) successfully and add relationship mapping with invalid throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityAddRelationMappingInvalidShares_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            entity.RelationMapping.SharePercentage = 80;
            entity.RelationMapping.Id = null;
            CurrentUserAC currentUser = GetCurrentUser();
            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 80
                }
            };
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<Company> companies = GetCompanies();
            companies.Single().CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("true")
                .Returns("75");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert

            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added new entity(user) successfully and add relationship mapping which already exists then invalid throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityAddRelationMappingUserAlreadyExists_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Id = Guid.NewGuid();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            entity.RelationMapping.Id = null;
            CurrentUserAC currentUser = GetCurrentUser();
            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = entity.Id.Value,
                    SharePercentage = 50
                }
            };
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<Company> companies = GetCompanies();
            companies.Single().CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            User user = GetUsers().Single();
            user.IsRegistered = true;
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("true")
                .Returns("75");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert

            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added new entity(user) successfully and update relationship mapping with share percentage null(relatives is false) successfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityUpdateRelationMappingRelativeFalseSuccessfully_AssertUpdateEntityRealtionMapping()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();
            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 80
                }
            };
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<Company> companies = GetCompanies();
            companies.Single().CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Partnership)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("false");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);

            // Act

            await _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People");

            // Assert

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityRelationshipMapping>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Check if added new entity(user) successfully and update relationship mapping with proprietor then throws InvalidParameterException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityProprietorUpdateRelationMapping_VerifyThrowsInvalidDataException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            CurrentUserAC currentUser = GetCurrentUser();
            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 80
                }
            };
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<Company> companies = GetCompanies();
            companies.Single().CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("false");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert

            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if added new entity(user) successfully and update relationship mapping with invalid shares then throws ValidationException.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateEntityAsync_AddEntityUpdateRelationMappingInvalidSharePercentage_VerifyThrowsValidationException()
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.Address = null;
            entity.RelationMapping = GetEntityRelation();
            entity.RelationMapping.SharePercentage = 180;
            CurrentUserAC currentUser = GetCurrentUser();
            List<User> users = new List<User>();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 180
                }
            };
            List<CompanyStructure> companyStructures = GetCompanyStructureList();
            List<Company> companies = GetCompanies();
            companies.Single().CompanyStructureId = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;
            entity.Company.CompanyStructure.Id = companyStructures.Single(x => x.Structure.Equals(StringConstant.Proprietorship)).Id;

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyStructure>())
                 .Returns(companyStructures.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null)
                .ReturnsAsync((User)null);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true");
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Relationship, bool>>>()))
                .Returns(GetProprietorRelationships().AsQueryable().BuildMock().Object);
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            // Act

            // Assert

            await Assert.ThrowsAsync<ValidationException>(() => _entityRepository.AddOrUpdateEntityAsync(entity, currentUser, "People"));
        }

        /// <summary>
        /// Check if remove link entity(company) logged In user has no access to company throws InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveLinkEntityAsync_NoCompanyAccess_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            EntityAC entity = new EntityAC
            {
                Id = Guid.NewGuid(),
                RelationMapping = new EntityRelationMappingAC
                {
                    PrimaryEntityId = Guid.NewGuid()
                }
            };
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.RemoveLinkEntityAsync(entity, currentUser));
        }

        /// <summary>
        /// Check if remove link entity(company) logged In user has no access to company throws InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveLinkEntityAsync_RemovedSuccessfully_AssertRemove()
        {
            //Arrange
            EntityAC entity = new EntityAC
            {
                Id = Guid.NewGuid(),
                RelationMapping = new ApplicationClass.Others.EntityRelationMappingAC
                {
                    PrimaryEntityId = Guid.NewGuid()
                }
            };
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid()
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act
            await _entityRepository.RemoveLinkEntityAsync(entity, currentUser);
            //Assert
            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<List<EntityRelationshipMapping>>()), Times.Once);
        }

        /// <summary>
        /// Check if all entities are returned if logged in user is bank user and no filters available.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityListAsync_IsBankUserNoFilters_AssetCount()
        {
            //Arrange
            FilterModelAC filterModel = null;
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.IsBankUser = true;
            List<EntityModel> entities = GetEntities();
            BankUser bankUser = new BankUser { Id = currentUser.Id, Email = currentUser.Email, Name = currentUser.Name, Phone = "+912154165" };

            _dataRepositoryMock.Setup(x => x.GetAll<EntityModel>())
                .Returns(entities.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync<BankUser>(It.IsAny<Expression<Func<BankUser, bool>>>()))
                .ReturnsAsync(bankUser);

            //Act

            var actual = await _entityRepository.GetEntityListAsync(filterModel, currentUser);

            //Assert
            Assert.Equal(entities.Count, actual.Count);
        }

        /// <summary>
        /// Check only current user returns if filter for it is given.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityListAsync_FilterAddedToGetOnlyCurrentUser_AssetCount()
        {
            //Arrange
            FilterModelAC filterModel = new FilterModelAC()
            {
                Filter = "[{\"Field\":\"type\", \"Operator\":\"=\", \"Value\":\"banker\"}]"
            };
            CurrentUserAC currentUser = GetCurrentUser();
            currentUser.IsBankUser = true;
            List<EntityModel> entities = GetEntities();
            BankUser bankUser = new BankUser { Id = currentUser.Id, Email = currentUser.Email, Name = currentUser.Name, Phone = "+912154165" };

            _dataRepositoryMock.Setup(x => x.GetAll<EntityModel>())
                .Returns(entities.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleAsync<BankUser>(It.IsAny<Expression<Func<BankUser, bool>>>()))
                .ReturnsAsync(bankUser);

            //Act

            var actual = await _entityRepository.GetEntityListAsync(filterModel, currentUser);

            //Assert
            Assert.Single(actual);
        }

        /// <summary>
        /// Check if all entities are returned if logged in user is bank user.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityListAsync_IsNotBankUser_AssetCount()
        {
            //Arrange
            FilterModelAC filterModel = null;

            CurrentUserAC currentUser = GetCurrentUser();

            List<EntityModel> entities = GetEntities();

            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object)
                .Returns(entities.Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            //Act

            var actual = await _entityRepository.GetEntityListAsync(filterModel, currentUser);

            //Assert
            Assert.Equal(entities.Count, actual.Count);
        }

        /// <summary>
        /// Check if filter object is passed with type = people then return correct result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityListAsync_FilterTypePeople_AssetCount()
        {
            //Arrange
            FilterModelAC filterModel = new FilterModelAC()
            {
                Filter = "[{\"Field\":\"type\", \"Operator\":\"=\", \"Value\":\"people\"}]"
            };

            CurrentUserAC currentUser = GetCurrentUser();

            List<EntityModel> entities = GetEntities();

            var excepted = entities.Where(x => x.Type == EntityType.User).ToList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object)
                .Returns(entities.Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            //Act

            var actual = await _entityRepository.GetEntityListAsync(filterModel, currentUser);

            //Assert
            Assert.Equal(excepted.Count, actual.Count);
        }

        /// <summary>
        /// Check if filter object is passed with type = company then return correct result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityListAsync_FilterTypeCompany_AssetCount()
        {
            //Arrange
            FilterModelAC filterModel = new FilterModelAC()
            {
                Filter = "[{\"Field\":\"type\", \"Operator\":\"=\", \"Value\":\"company\"}]"
            };

            CurrentUserAC currentUser = GetCurrentUser();

            List<EntityModel> entities = GetEntities();

            var excepted = entities.Where(x => x.Type == EntityType.Company).ToList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object)
                .Returns(entities.Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            //Act

            var actual = await _entityRepository.GetEntityListAsync(filterModel, currentUser);

            //Assert
            Assert.Equal(excepted.Count, actual.Count);
        }

        /// <summary>
        /// Check if filter object is passed with id then return correct result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityListAsync_FilterId_AssetId()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();

            List<EntityModel> entities = GetEntities();
            Guid Id = entities.First().Id;
            FilterModelAC filterModel = new FilterModelAC()
            {
                Filter = "[{\"Field\":\"id\", \"Operator\":\"=\", \"Value\":\"" + Id.ToString() + "\"}]"
            };
            var excepted = entities.Where(x => x.Id == Id).ToList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object)
                .Returns(entities.Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act

            var actual = await _entityRepository.GetEntityListAsync(filterModel, currentUser);

            //Assert
            Assert.Equal(excepted.Single().Id, actual.Single().Id);
        }

        /// <summary>
        /// Check if filter object is passed with page count and number then return correct result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityListAsync_FilterPageCount_AssetCount()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityModel> entities = GetEntities();

            FilterModelAC filterModel = new FilterModelAC()
            {
                PageNo = 2,
                PageRecordCount = 1
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object)
                .Returns(entities.Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            //Act

            var actual = await _entityRepository.GetEntityListAsync(filterModel, currentUser);

            //Assert
            Assert.Single(actual);
        }

        /// <summary>
        /// Check if entity of type company is fetched successfully
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityAsync_TypeCompany_AssetId()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityModel> entities = GetEntities();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntity = entities.Single(x => x.Type == EntityType.Company),
                    PrimaryEntityId = entities.Single(x => x.Type == EntityType.Company).Id,
                    RelativeEntity = entities.Single(x => x.Type == EntityType.User),
                    RelativeEntityId = entities.Single(x => x.Type == EntityType.User).Id,
                    SharePercentage = 100,
                    Relationship = new Relationship
                    {
                        Id = Guid.NewGuid(),
                        Relation = "Proprietor"
                    },
                    RelationshipId = Guid.NewGuid()
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            var expectedId = entities.Single(x => x.Type == EntityType.Company).Id;
            //Act

            var actual = await _entityRepository.GetEntityAsync(entities.Single(x => x.Type == EntityType.Company).Id, currentUser);

            //Assert
            Assert.Equal(expectedId, actual.Id);
        }

        /// <summary>
        /// Check if entity of type company is fetched and user has no access then throw InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityAsync_TypeCompanyNoAccess_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityModel> entities = GetEntities();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.GetEntityAsync(entities.Single(x => x.Type == EntityType.Company).Id, currentUser));
        }

        /// <summary>
        /// Check if entity of type people and has no access throw InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityAsync_TypePeople_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            List<User> users = new List<User>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.GetEntityAsync(Guid.NewGuid(), currentUser));
        }

        /// <summary>
        /// Check if entity of invalid type then return empty object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityAsync_InvalidType_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityModel> entities = GetEntities().Where(x => x.Type == EntityType.User).ToList();
            entities.Single().Type = (EntityType)3;
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityRepository.GetEntityAsync(Guid.NewGuid(), currentUser);
            //Assert
            Assert.Null(actual.Id);
        }

        /// <summary>
        /// Check if entity of type people is fetched successfully.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEntityAsync_TypePeople_AssertId()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            List<EntityModel> entities = GetEntities();
            List<User> users = new List<User>() {
                entities.Single(x => x.Type == EntityType.User).User
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(entities.Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMock().Object);
            Guid expectedId = entities.Single(x => x.Type == EntityType.User).User.Id;

            //Act
            var actual = await _entityRepository.GetEntityAsync(entities.Single(x => x.Type == EntityType.User).Id, currentUser);

            //Assert
            Assert.Equal(expectedId, actual.Id);
        }

        /// <summary>
        /// Check if entity(user) has no access to entity(company) then throw InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckEntityAllowToStartNewApplicationAsync_EntityNotLinkedWithUser_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.CheckEntityAllowToStartNewApplicationAsync(Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Check if entity has no open application and return true.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckEntityAllowToStartNewApplicationAsync_IsAllowed_AssertTrue()
        {
            //Arrange
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            //Act
            var actual = await _entityRepository.CheckEntityAllowToStartNewApplicationAsync(Guid.NewGuid(), _currentUserAC);
            //Assert
            Assert.True(actual);
        }

        /// <summary>
        /// Check if entity has open application and return false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckEntityAllowToStartNewApplicationAsync_IsNotAllowed_AssertFalse()
        {
            //Arrange
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping
                {
                    Id = Guid.NewGuid()
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            //Act
            var actual = await _entityRepository.CheckEntityAllowToStartNewApplicationAsync(Guid.NewGuid(), _currentUserAC);
            //Assert
            Assert.False(actual);
        }
        #region User Credit profile information
        /// <summary>
        /// Method to check if the credit score is null, empty or less than minimum credit score, then it returns false.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(StringConstant.PoorCreditScore)]
        [InlineData(StringConstant.AverageCreditScore)]
        public async Task UpdateUserCreditProfileInformationAsync_NullEmptyOrLessThanMinScore_UpdateUserScoreAndReturnsFalse(string creditScore)
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.User.SelfDeclaredCreditScore = creditScore;

            // Set appsettings for credit profile information.
            SetUserCreditProfileConfig(false, false, 651);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<User, bool>>>())).Returns(Task.FromResult(_loggedInUser));

            // Act
            bool status = await _entityRepository.UpdateUserCreditProfileInformationAsync(entity);

            // Assert
            _dataRepositoryMock.Verify(x => x.Update<User>(It.IsAny<User>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(1));
            Assert.False(status);
        }

        /// <summary>
        /// Method to check if user's bankruptcy, judgement and residency status is according to the requirement or not.
        /// If yes then it returns true otherwise it returns false. 
        /// </summary>
        [Theory]
        [InlineData(true, true, ResidencyStatus.USCitizen, true, true, true)]
        [InlineData(true, true, ResidencyStatus.USCitizen, true, false, true)]
        [InlineData(true, true, ResidencyStatus.USCitizen, false, true, true)]
        [InlineData(true, true, ResidencyStatus.USCitizen, false, false, true)]
        [InlineData(false, false, ResidencyStatus.USCitizen, true, true, false)]
        [InlineData(false, false, ResidencyStatus.USCitizen, true, false, false)]
        [InlineData(false, false, ResidencyStatus.USCitizen, false, true, false)]
        [InlineData(false, false, ResidencyStatus.USCitizen, false, false, true)]
        [InlineData(false, true, ResidencyStatus.USCitizen, false, true, true)]
        [InlineData(false, true, ResidencyStatus.USCitizen, false, false, true)]
        [InlineData(false, true, ResidencyStatus.USCitizen, true, false, false)]
        [InlineData(false, true, ResidencyStatus.USCitizen, true, true, false)]
        [InlineData(true, false, ResidencyStatus.USCitizen, true, false, true)]
        [InlineData(true, false, ResidencyStatus.USCitizen, false, false, true)]
        [InlineData(true, false, ResidencyStatus.USCitizen, false, true, false)]
        [InlineData(true, false, ResidencyStatus.USCitizen, true, true, false)]
        [InlineData(true, true, ResidencyStatus.NonResident, false, true, true)]
        [InlineData(false, false, ResidencyStatus.NonResident, false, false, true)]
        [InlineData(false, true, ResidencyStatus.NonResident, false, true, true)]
        [InlineData(true, false, ResidencyStatus.NonResident, true, false, true)]
        public async Task UpdateUserCreditProfileInformationAsync_CheckJudgementBankruptcyResidencyStatus_AssertReturnBooleanValule(bool isAllowedJudgement,
            bool isAllowedBankruptcy, ResidencyStatus residencyStatus, bool isUserHavingAnyJudgements, bool isUserHavingBankruptcy, bool returnValue)
        {
            // Arrange
            EntityAC entity = GetEntity();
            entity.User.SelfDeclaredCreditScore = StringConstant.GoodCreditScore;
            entity.User.HasAnyJudgementsSelfDeclared = isUserHavingAnyJudgements;
            entity.User.HasBankruptcySelfDeclared = isUserHavingBankruptcy;
            _loggedInUser.ResidencyStatus = residencyStatus;
            // Set appsettings for credit profile information.
            SetUserCreditProfileConfig(isAllowedJudgement, isAllowedBankruptcy, 650);
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<User, bool>>>())).Returns(Task.FromResult(_loggedInUser));

            // Act
            var status = await _entityRepository.UpdateUserCreditProfileInformationAsync(entity);

            // Assert
            _dataRepositoryMock.Verify(x => x.Update<User>(It.IsAny<User>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(1));
            if (returnValue)
            {
                Assert.True(status);
            }
            else
            {
                Assert.False(status);
            }
        }
        #endregion

        #region Entity's loan applications
        /// <summary>
        /// Test method to verify count of loan applications of user when loan applications exist.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationsWithBasicDetailsByEntityIdAsync_LoanApplicationExists_VerifyCountOfLoanApplications()
        {
            //Arrange
            var entityId = Guid.NewGuid();
            var loanApplicationId1 = Guid.NewGuid();
            var loanApplicationId2 = Guid.NewGuid();
            var entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    LoanApplicationId = loanApplicationId1,
                    LoanApplication = new LoanApplication()
                    {
                        Id = loanApplicationId1,
                        LoanApplicationNumber = "LP2805202006352244",
                        Status = LoanApplicationStatusType.Draft,
                        CreatedByUserId = _currentUserAC.Id,
                        UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = _currentUserAC.Id,
                                SectionId = Guid.NewGuid(),
                                Section = new Section
                                {
                                    Name = "Company Info"
                                }
                            }
                        }
                    }
                },
                new EntityLoanApplicationMapping()
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    LoanApplicationId = loanApplicationId2,
                    LoanApplication = new LoanApplication()
                    {
                        Id = loanApplicationId2,
                        LoanApplicationNumber = "LP2805202006386688",
                        Status = LoanApplicationStatusType.Draft,
                        CreatedByUserId = _currentUserAC.Id,
                        UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = _currentUserAC.Id,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Name = "Company Info" }
                            }
                        }
                    }
                }
            };
            var loanApplicationListAC = new List<ApplicationBasicDetailAC>()
            {
                new ApplicationBasicDetailAC
                {
                    Id = loanApplicationId1,
                    LoanApplicationNumber = "LP2805202006352244",
                    Status = LoanApplicationStatusType.Draft
                },
                new ApplicationBasicDetailAC
                {
                    Id = loanApplicationId2,
                    LoanApplicationNumber = "LP2805202006386688",
                    Status = LoanApplicationStatusType.Draft
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<List<ApplicationBasicDetailAC>>(), true)).ReturnsAsync(loanApplicationListAC);

            //Act
            List<ApplicationBasicDetailAC> actualList = await _entityRepository.GetLoanApplicationsWithBasicDetailsByEntityIdAsync(entityId, _currentUserAC);

            //Assert
            Assert.Equal(entityLoanApplicationMappings.ToList().Count, actualList.Count);
        }

        /// <summary>
        /// Test method to verify that it throws an exception if user is not linked with the entity.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationsWithBasicDetailsByEntityIdAsync_UserNotLinkedWithEntity_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            var entityId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.GetLoanApplicationsWithBasicDetailsByEntityIdAsync(entityId, _currentUserAC));
        }

        /// <summary>
        /// Test method to verify empty list of loan applications when loan applications does't exist.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationsWithBasicDetailsByEntityIdAsync_LoanAplicationNotExists_VerifyEmptyListOfLoanApplications()
        {
            //Arrange
            var entityId = Guid.NewGuid();
            var entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>() { };
            var loanApplicationListAC = new List<ApplicationBasicDetailAC>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<List<ApplicationBasicDetailAC>>(), true)).ReturnsAsync(loanApplicationListAC);

            //Act
            List<ApplicationBasicDetailAC> actualList = await _entityRepository.GetLoanApplicationsWithBasicDetailsByEntityIdAsync(entityId, _currentUserAC);

            //Assert
            Assert.Empty(actualList);
        }

        /// <summary>
        /// Test method to verify that no call will be made to EntityLoanApplicationMapping table if entity is user.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationsWithBasicDetailsByEntityIdAsync_EntityIsUser_VerifyDBCallToLoanApplicationTable()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            List<LoanApplication> loanApplications = new List<LoanApplication>
            {
                new LoanApplication { Id = Guid.NewGuid(), CreatedByUserId=_currentUserAC.Id, UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = _currentUserAC.Id,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Name = "Company Info" }
                            }
                        },EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping>() }
            };
            List<ApplicationBasicDetailAC> loanApplicationList = new List<ApplicationBasicDetailAC>
            {
                new ApplicationBasicDetailAC { Id = loanApplications.First().Id }
            };
            List<EntityLoanApplicationMapping> entityLoanMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    LoanApplicationId = loanApplications.First().Id,
                    LoanApplication = new LoanApplication()
                    {
                        Id = loanApplications.First().Id,
                        LoanApplicationNumber = "LP2805202006352244",
                        Status = LoanApplicationStatusType.Draft,
                        CreatedByUserId = _currentUserAC.Id,
                        UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = _currentUserAC.Id,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Name = "Company Info" }
                            }
                        }
                    }
                }
            };
            List<EntityRelationshipMapping> entityRelationships = new List<EntityRelationshipMapping>
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityId,
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 90,
                    RelationshipId = Guid.NewGuid()
                }
            };

            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object)
                .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationships.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<List<ApplicationBasicDetailAC>>(), true)).ReturnsAsync(loanApplicationList);
            _globalRepositoryMock.Setup(x => x.IsLoanReadOnlyAsync(It.IsAny<LoanApplication>(), false)).ReturnsAsync(false);

            //Act
            List<ApplicationBasicDetailAC> actualList = await _entityRepository.GetLoanApplicationsWithBasicDetailsByEntityIdAsync(entityId, _currentUserAC);

            //Assert
            Assert.NotEmpty(actualList);
            Assert.Equal(actualList.First().MappedEntityId, Guid.Empty);
            Assert.False(actualList.First().IsReadOnlyMode);
        }
        #endregion

        #region Credit Report

        /// <summary>
        /// Check if entity is not linked with loan application throw InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityCompanyNotLinked_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>() { };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);


            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.FetchCreditReportAsync(Guid.NewGuid(), Guid.NewGuid(), currentUser));
        }

        /// <summary>
        /// Check if entity is not linked with current user throw InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityCompanyUserNoAccess_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>() { };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.FetchCreditReportAsync(Guid.NewGuid(), Guid.NewGuid(), currentUser));
        }

        /// <summary>
        /// Check if entity is not linked with current user throw InvalidParameterException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityCompanyNotAllEntityGivenConsent_VerifyThrowsInvalidDataException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid(),
                    RelativeEntityId = currentUser.Id
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>() { };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.FetchCreditReportAsync(Guid.NewGuid(), Guid.NewGuid(), currentUser));
        }

        /// <summary>
        /// Check if credit report is fetched successfully and saved of company.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityCompanySuccessfullyFetchedCreditReport_AssertAddAsync()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid(),
                    RelativeEntityId = currentUser.Id
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent()
                {
                    Id = Guid.NewGuid()
                }
            };
            PremierProfilesResponseAC premierProfilesResponse = new PremierProfilesResponseAC()
            {
                JsonResponse = @"",
                Results = new ResultsAC()
                {
                    ExpandedCreditSummary = new ExpandedCreditSummaryAC()
                    {
                        BankruptcyIndicator = false,
                        JudgmentIndicator = false,
                        TaxLienIndicator = false
                    },
                    ScoreInformation = new ScoreInformationAC()
                    {
                        CommercialScore = new CommercialScoreAC()
                        {
                            Score = 800
                        },
                        FsrScore = new FsrScoreAC()
                        {
                            Score = 750
                        }
                    }
                }
            };
            List<Company> companies = GetEntities().Where(x => x.Type == EntityType.Company).Select(s => s.Company).ToList();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("Experian");
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .ReturnsAsync(integratedServiceConfiguration.First());
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            List<CreditReport> creditReports = new List<CreditReport>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<String>(), It.IsAny<String>()))
                .ReturnsAsync(premierProfilesResponse);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            //Act
            await _entityRepository.FetchCreditReportAsync(Guid.NewGuid(), Guid.NewGuid(), currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Check if credit report exist and older than 1 year is fetched successfully and added of company.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityCompanyExistCreditReport_AssertAddAsync()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid(),
                    RelativeEntityId = currentUser.Id
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent()
                {
                    Id = Guid.NewGuid()
                }
            };
            PremierProfilesResponseAC premierProfilesResponse = new PremierProfilesResponseAC()
            {
                JsonResponse = @"",
                Results = new ResultsAC()
                {
                    ExpandedCreditSummary = new ExpandedCreditSummaryAC()
                    {
                        BankruptcyIndicator = false,
                        JudgmentIndicator = false,
                        TaxLienIndicator = false
                    },
                    ScoreInformation = new ScoreInformationAC()
                    {
                        CommercialScore = new CommercialScoreAC()
                        {
                            Score = 800
                        },
                        FsrScore = new FsrScoreAC()
                        {
                            Score = 750
                        }
                    }
                }
            };
            List<Company> companies = GetEntities().Where(x => x.Type == EntityType.Company).Select(s => s.Company).ToList();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<CreditReport> creditReports = new List<CreditReport>()
            {
                new CreditReport()
                {
                    Id = Guid.NewGuid(),
                    CreatedOn = DateTime.UtcNow.AddYears(-2),
                    FsrScore = 750,
                    IsBankrupted = false,
                    Response = @"",
                    IntegratedServiceConfigurationId = integratedServiceConfiguration.First().Id,
                    Version = Guid.NewGuid()
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.Company).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("Experian");

            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .ReturnsAsync(integratedServiceConfiguration.First());
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<String>(), It.IsAny<String>()))
                .ReturnsAsync(premierProfilesResponse);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companies.AsQueryable().BuildMock().Object);
            //Act
            await _entityRepository.FetchCreditReportAsync(Guid.NewGuid(), Guid.NewGuid(), currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Check if there is another invalid type for entity then throw InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_InvalidEntityType_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);


            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.FetchCreditReportAsync(Guid.NewGuid(), Guid.NewGuid(), currentUser));
        }

        /// <summary>
        /// Check if current user has no access to entity then throw InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityUserNoAccess_VerifyThrowsInvalidResourceAccessException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>() { };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityRepository.FetchCreditReportAsync(currentUser.Id, Guid.NewGuid(), currentUser));
        }

        /// <summary>
        /// Check if current user has not given his consent for a given loan then throw InvalidResourceAccessException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_UserHasNotGivenConsent_VerifyThrowsInvalidParameterException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.FetchCreditReportAsync(currentUser.Id, Guid.NewGuid(), currentUser));
        }

        /// <summary>
        /// Check if entity User credit report is fetched successfully and saved for experian.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityUserFetchedSuccessfullyExperian_AssertAddAsync()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent()
                {
                    LoanApplicationId = entityLoanApplicationMappings.First().LoanApplicationId,
                    ConsenteeId = currentUser.Id
                }
            };
            UserInfoAC userInfo = GetUserInfo();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("Experian");
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .ReturnsAsync(integratedServiceConfiguration.First());
            List<CreditReport> creditReports = new List<CreditReport>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>(), It.IsAny<String>()))
                .ReturnsAsync(userInfo);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);

            //Act
            await _entityRepository.FetchCreditReportAsync(currentUser.Id, Guid.NewGuid(), currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Check if entity User credit report is fetched successfully and saved for equifax.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityUserFetchedSuccessfullyEquifax_AssertAddAsync()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent()
                {
                    LoanApplicationId = entityLoanApplicationMappings.First().LoanApplicationId,
                    ConsenteeId = currentUser.Id
                }
            };
            UserInfoAC userInfo = GetUserInfo();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("Equifax");
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .ReturnsAsync(integratedServiceConfiguration.First());
            List<CreditReport> creditReports = new List<CreditReport>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _equifaxUtilityMock.Setup(x => x.FetchUserCreditScoreEquifaxAsync(It.IsAny<UserInfoAC>(), It.IsAny<String>()))
                .ReturnsAsync(userInfo);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);

            //Act
            await _entityRepository.FetchCreditReportAsync(currentUser.Id, Guid.NewGuid(), currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Check if entity User credit report is fetched successfully and saved for Transunion.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityUserFetchedSuccessfullyTransunion_AssertAddAsync()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent()
                {
                    LoanApplicationId = entityLoanApplicationMappings.First().LoanApplicationId,
                    ConsenteeId = currentUser.Id
                }
            };
            UserInfoAC userInfo = GetUserInfo();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("Transunion");
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .ReturnsAsync(integratedServiceConfiguration.First());
            List<CreditReport> creditReports = new List<CreditReport>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _transunionUtilityMock.Setup(x => x.FetchConsumerCreditReportAsync(It.IsAny<UserInfoAC>(), It.IsAny<String>()))
                .ReturnsAsync(userInfo);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);

            //Act
            await _entityRepository.FetchCreditReportAsync(currentUser.Id, Guid.NewGuid(), currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Check if invalid bureau is selected from appsettings then throw InvalidParameterException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityUserInvalidSelectedBureau_VerifyThrowsInvalidDataException()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent()
                {
                    LoanApplicationId = entityLoanApplicationMappings.First().LoanApplicationId,
                    ConsenteeId = currentUser.Id
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("xyz");
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .ReturnsAsync(integratedServiceConfiguration.First());
            List<CreditReport> creditReports = new List<CreditReport>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.FetchCreditReportAsync(currentUser.Id, Guid.NewGuid(), currentUser));
        }

        /// <summary>
        /// Check if entity User credit report exist and is older than 1 year is fetched successfully and added for experian.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityUserCreditRepotExistFetchedSuccessfully_AssertAdd()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid()
                }
            };
            UserInfoAC userInfo = GetUserInfo();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<CreditReport> creditReports = new List<CreditReport>()
            {
                new CreditReport()
                {
                    Id = Guid.NewGuid(),
                    CreatedOn = DateTime.UtcNow.AddYears(-2),
                    FsrScore = 750,
                    IsBankrupted = false,
                    Response = @"",
                    IntegratedServiceConfigurationId = integratedServiceConfiguration.First().Id,
                    Version = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent()
                {
                    LoanApplicationId = entityLoanApplicationMappings.First().LoanApplicationId,
                    ConsenteeId = currentUser.Id
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("Experian");
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .ReturnsAsync(integratedServiceConfiguration.First());
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>(), It.IsAny<String>()))
                .ReturnsAsync(userInfo);

            //Act
            await _entityRepository.FetchCreditReportAsync(currentUser.Id, Guid.NewGuid(), currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Check if entity User credit report exist and is not older than 1 year is fetched successfully and updated for experian.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchCreditReportAsync_EntityUserCreditRepotExistNotFetched_AssertUpdateAndAdd()
        {
            //Arrange
            CurrentUserAC currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = GetEntityLoanApplicationMappingList();
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid()
                }
            };
            UserInfoAC userInfo = GetUserInfo();
            List<IntegratedServiceConfiguration> integratedServiceConfiguration = GetIntegratedServiceConfiguration();
            List<CreditReport> creditReports = new List<CreditReport>()
            {
                new CreditReport()
                {
                    Id = Guid.NewGuid(),
                    CreatedOn = DateTime.UtcNow.AddMonths(-2),
                    FsrScore = 750,
                    IsBankrupted = false,
                    Response = @"",
                    IntegratedServiceConfigurationId = integratedServiceConfiguration.First().Id,
                    Version = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent()
                {
                    LoanApplicationId = entityLoanApplicationMappings.First().LoanApplicationId,
                    ConsenteeId = currentUser.Id
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(GetEntities().Where(x => x.Type == EntityType.User).AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("Experian");
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .ReturnsAsync(integratedServiceConfiguration.First());
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<CreditReport>>(), true))
                .Returns(creditReports);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(integratedServiceConfiguration.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>(), It.IsAny<String>()))
                .ReturnsAsync(userInfo);

            //Act
            await _entityRepository.FetchCreditReportAsync(currentUser.Id, Guid.NewGuid(), currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<CreditReport>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Check if invalid entity type is encounter then throws InvalidParameterException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetCreditReportAsync_InvalidEntityType_VerifyThrowsInvalidDataException()
        {
            //Arrange
            EntityModel entity = GetEntities().Single(x => x.Type == EntityType.User);
            entity.Type = (EntityType)3;
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(Task.FromResult(entity));
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityRepository.GetCreditReportAsync(Guid.NewGuid(), null));
        }

        /// <summary>
        /// Check if credit report not exist for particular company then throws DataNotFoundException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetCreditReportAsync_CreditReportNotExists_VerifyThrowsDataNotFoundException()
        {
            //Arrange
            EntityModel entity = GetEntities().Single(x => x.Type == EntityType.User);
            IntegratedServiceConfiguration integratedServiceConfiguration = GetIntegratedServiceConfiguration().Single();
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(Task.FromResult(entity));
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(Task.FromResult(integratedServiceConfiguration));
            List<CreditReport> creditReports = new List<CreditReport>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(() => _entityRepository.GetCreditReportAsync(Guid.NewGuid(), null));
        }

        /// <summary>
        /// Check if credit report exist then get successfully.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        [InlineData("85100000-4512-5421-4521-550000000550")]
        public async Task GetCreditReportAsync_CreditReportGetSuccessfullyLoanIdNotProvided_AssertCreditReport(Guid loanId)
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            List<CreditReport> creditReports = new List<CreditReport>()
            {
                new CreditReport
                {
                    Id = Guid.NewGuid(),
                    Response = @"<?xml version=""1.0""?><Root></Root>",
                    IntegratedServiceConfigurationId = Guid.NewGuid(),
                    EntityId = entityId,
                    IntegratedServiceConfiguration = new IntegratedServiceConfiguration
                    {
                        Id=Guid.NewGuid(),
                        Name= StringConstant.TransunionAPI
                    },
                    Version = Guid.NewGuid()
                }
            };
            EntityModel entity = GetEntities().Single(x => x.Type == EntityType.User);
            IntegratedServiceConfiguration integratedServiceConfiguration = GetIntegratedServiceConfiguration().Single();
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<EntityModel, bool>>>()))
                .Returns(Task.FromResult(entity));
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>()))
                .Returns(Task.FromResult(integratedServiceConfiguration));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReports.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.ConvertXmlToJson(It.IsAny<string>())).Returns(JObject.Parse(@"{'FirstName': 'Arjunsinh'}"));
            //Act
            var actual = await _entityRepository.GetCreditReportAsync(entityId, loanId);

            //Assert
            Assert.Equal(actual.Id, entityId);
            Assert.NotNull(actual.CreditReport.CreditReportJson);
        }
        #endregion

        #region Additional Documents

        /// <summary>
        /// Method to check if the id sent in request is empty guid then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAdditionalDocumentsByResourceIdAsync_EmptyId_ThrowsInvalidParameterException()
        {
            //Arrange

            //Act            

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(Guid.Empty, ResourceType.Company, _currentUserAC));
        }

        /// <summary>
        /// Method to check if current user is trying to access unauthorized entity's additional documents then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAdditionalDocumentsByResourceIdAsync_CurrentUserAccessingOtherEntitysDocuments_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(entityId, ResourceType.Company, _currentUserAC));
        }

        /// <summary>
        /// Method to check if current user is trying to access unauthorized loan's additional documents then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAdditionalDocumentsByResourceIdAsync_CurrentUserAccessingLoansDocuments_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(entityId, ResourceType.Loan, _currentUserAC));
        }

        /// <summary>
        /// Method to check if no documents are present for given entity then it returns empty object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAdditionalDocumentsByResourceIdAsync_NoDocumentsExistForGivenEnity_ReturnsEmptyObject()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            var documents = new List<EntityAdditionalDocument>();

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityAdditionalDocument, bool>>>()))
                .Returns(documents.AsQueryable().BuildMock().Object);

            //Act
            EntityAC actual = await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(entityId, ResourceType.Company, _currentUserAC);

            //Assert
            Assert.Empty(actual.AdditionalDocuments);
        }

        /// <summary>
        /// Method to check if documents are exist for given entity with loan id null then they are the latest ones and it returns those documents.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAdditionalDocumentsByResourceIdAsync_DocumentsExistWithNullLoanId_ReturnsThoseDocuments()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            var documents = GetEntityAdditionalDocuments();
            documents.ForEach(x => x.EntityId = entityId);

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityAdditionalDocument, bool>>>()))
                .Returns(documents.AsQueryable().BuildMock().Object);

            //Act
            EntityAC actual = await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(entityId, ResourceType.Company, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.AdditionalDocuments);
            Assert.Equal(actual.AdditionalDocuments.Count, documents.Count);
            Assert.Null(actual.LoanId);
        }

        /// <summary>
        /// Method to check if current user is bank user and has access to the given entity then it doesn't allow bank user to create version related changes in GET call.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAdditionalDocumentsByResourceIdAsync_CurrentUserIsBankUserAndNeedsToAddNewNonVersionedData_DoesntAllowBankUserToAddNewNonVersionedData()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            Guid loanApplicationId = Guid.NewGuid();
            var documents = GetEntityAdditionalDocuments();
            documents.ForEach(x => x.EntityId = entityId);
            documents.ForEach(x => x.LoanApplicationId = loanApplicationId);
            _currentUserAC.IsBankUser = true;

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityAdditionalDocument, bool>>>()))
                .Returns(documents.AsQueryable().BuildMock().Object);

            //Act
            EntityAC actual = await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(entityId, ResourceType.Company, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityAdditionalDocument>(It.IsAny<List<EntityAdditionalDocument>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Never);
            Assert.Empty(actual.AdditionalDocuments);
            Assert.Null(actual.LoanId);
        }

        /// <summary>
        /// Method to check if no documents are exist for given entity with loan id null 
        /// then it will fetch the latest versioned documents for given entity and save them with null loan id 
        /// and then it returns those documents.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAdditionalDocumentsByResourceIdAsync_DocumentsNotExistWithNullLoanId_SaveDocumentsWithNullLoanIdAndReturnsThoseDocuments()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            Guid loanApplicationId = Guid.NewGuid();
            var additionalDocuments = GetEntityAdditionalDocuments();
            var versionedAdditionalDocuments = GetEntityAdditionalDocuments();
            additionalDocuments.ForEach(x => x.EntityId = entityId);
            additionalDocuments.ForEach(x => x.LoanApplicationId = loanApplicationId);
            List<Document> documents = new List<Document>()
            {
                new Document { Id = additionalDocuments.First().DocumentId },
                new Document { Id = additionalDocuments.Last().DocumentId }
            };

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityAdditionalDocument, bool>>>()))
                .Returns(additionalDocuments.AsQueryable().BuildMock().Object)
                .Returns(versionedAdditionalDocuments.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns(documents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<EntityAdditionalDocument>>(), true)).Returns(additionalDocuments);
            _dataRepositoryMock.SetupSequence(x => x.DetachEntities(It.IsAny<IQueryable<Document>>(), true))
                .Returns(new List<Document> { documents.First() })
                .Returns(new List<Document> { documents.Last() });

            //Act
            EntityAC actual = await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(entityId, ResourceType.Company, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityAdditionalDocument>(It.IsAny<List<EntityAdditionalDocument>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            Assert.NotEmpty(actual.AdditionalDocuments);
            Assert.Null(actual.LoanId);
        }

        /// <summary>
        /// Method to check if resource type is loan then it returns the latest version of documents for given loan.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAdditionalDocumentsByResourceIdAsync_ResourceTypeIsLoan_ReturnsLatestVersionOfDocumentsForGivenLoan()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            Guid loanApplicationId = Guid.NewGuid();
            var additionalDocuments = GetEntityAdditionalDocuments();
            additionalDocuments.ForEach(x => x.EntityId = entityId);
            additionalDocuments.ForEach(x => x.LoanApplicationId = loanApplicationId);

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityAdditionalDocument, bool>>>()))
                .Returns(additionalDocuments.AsQueryable().BuildMock().Object);

            //Act
            EntityAC actual = await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(loanApplicationId, ResourceType.Loan, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityAdditionalDocument>(It.IsAny<List<EntityAdditionalDocument>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Never);
            Assert.NotEmpty(actual.AdditionalDocuments);
            Assert.NotNull(actual.LoanId);
        }

        /// <summary>
        /// Method to check if resource type is loan and no any documents found for given loan then it returns an empty list.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAdditionalDocumentsByResourceIdAsync_NoDocumentsForGivenLoan_ReturnsEmptyList()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            var additionalDocuments = new List<EntityAdditionalDocument>();

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(_currentUserAC, It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityAdditionalDocument, bool>>>()))
                .Returns(additionalDocuments.AsQueryable().BuildMock().Object);

            //Act
            EntityAC actual = await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(loanApplicationId, ResourceType.Loan, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityAdditionalDocument>(It.IsAny<List<EntityAdditionalDocument>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Never);
            Assert.Empty(actual.AdditionalDocuments);
            Assert.NotNull(actual.LoanId);
        }

        /// <summary>
        /// Method to check if the current user is accessing unauthorized entity then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveAdditionalDocumentOfEntityAsync_CurrentUserAccessingOtherEntity_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            EntityAC entityAC = new EntityAC();
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _entityRepository.SaveAdditionalDocumentOfEntityAsync(entityId, entityAC, _currentUserAC));
        }

        /// <summary>
        /// Method to check if the id sent in request is empty guid then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveAdditionalDocumentOfEntityAsync_EmptyId_ThrowsInvalidParameterException()
        {
            //Arrange
            EntityAC entityAC = new EntityAC
            {
                AdditionalDocuments = new List<AdditionalDocumentAC>
                {
                   new AdditionalDocumentAC
                   {
                       Document = new DocumentAC
                       {
                           Name = "."
                       }
                   }
                }
            };
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityRepository.SaveAdditionalDocumentOfEntityAsync(Guid.Empty, entityAC, _currentUserAC));
        }

        /// <summary>
        /// Method to check if any file with invalid extension provided in request then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("abc.def")]
        [InlineData("abc.c sv")]
        [InlineData("abc_def.PDF")]
        [InlineData("abc.pdfpdf")]
        public async Task SaveAdditionalDocumentOfEntityAsync_FileWithInvalidExtension_ThrowsInvalidParameterException(string fileName)
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            EntityAC entityAC = new EntityAC
            {
                AdditionalDocuments = new List<AdditionalDocumentAC>
                {
                   new AdditionalDocumentAC
                   {
                       Document = new DocumentAC
                       {
                           Name = fileName
                       }
                   }
                }
            };
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityRepository.SaveAdditionalDocumentOfEntityAsync(entityId, entityAC, _currentUserAC));
        }

        /// <summary>
        /// Method to check if no any document found with null loan id for given entity then it add new documents in DB.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveAdditionalDocumentOfEntityAsync_NoDocumentsWithNullLoanIdForGivenEntity_PerformsAddOperation()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            var additionalDocuments = new List<EntityAdditionalDocument>();
            EntityAC entityAC = GetEntityACWithAdditionalDocumentACList();

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityAdditionalDocument, bool>>>()))
                .Returns(additionalDocuments.AsQueryable().BuildMock().Object);
            _amazonServiceUtility.Setup(x => x.CopyObject(It.IsAny<string>(), It.IsAny<string>()));
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act
            await _entityRepository.SaveAdditionalDocumentOfEntityAsync(entityId, entityAC, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityAdditionalDocument>(It.IsAny<List<EntityAdditionalDocument>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method to check if documents exist for given entity then it add/remove new/existing documents after checking conditions.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveAdditionalDocumentOfEntityAsync_DocumentsExistForGivenEntity_PerformsAddRemoveOperationsAsPerData()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            var additionalDocuments = GetEntityAdditionalDocuments();
            EntityAC entityAC = GetEntityACWithAdditionalDocumentACList();
            List<Document> documentsToRemove = new List<Document>
            {
                new Document { Id = additionalDocuments.First().DocumentId },
                new Document { Id = additionalDocuments.Last().DocumentId }
            };

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityAdditionalDocument, bool>>>()))
                .Returns(additionalDocuments.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns(documentsToRemove.AsQueryable().BuildMock().Object);
            _amazonServiceUtility.Setup(x => x.CopyObject(It.IsAny<string>(), It.IsAny<string>()));
            _amazonServiceUtility.Setup(x => x.DeleteObjectsAsync(It.IsAny<List<string>>())).Verifiable();
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act
            await _entityRepository.SaveAdditionalDocumentOfEntityAsync(entityId, entityAC, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityAdditionalDocument>(It.IsAny<List<EntityAdditionalDocument>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityAdditionalDocument>(It.IsAny<List<EntityAdditionalDocument>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.RemoveRange<Document>(It.IsAny<List<Document>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method to check if documents exist but no documents are there to add/remove then it doesn't perform any operations.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveAdditionalDocumentOfEntityAsync_NoNewDocumentToAddOrRemove_PerformsNoOperations()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            var additionalDocuments = GetEntityAdditionalDocuments();
            additionalDocuments.RemoveAt(1);
            EntityAC entityAC = GetEntityACWithAdditionalDocumentACList();
            entityAC.AdditionalDocuments.First().Id = additionalDocuments.First().Id;

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityAdditionalDocument, bool>>>()))
                .Returns(additionalDocuments.AsQueryable().BuildMock().Object);

            //Act
            await _entityRepository.SaveAdditionalDocumentOfEntityAsync(entityId, entityAC, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityAdditionalDocument>(It.IsAny<List<EntityAdditionalDocument>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityAdditionalDocument>(It.IsAny<List<EntityAdditionalDocument>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.RemoveRange<Document>(It.IsAny<List<Document>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Never);
        }

        #endregion

        #endregion
    }
}
