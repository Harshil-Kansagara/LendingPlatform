using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass.EntityInfo;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace LendingPlatform.Repository.Test.EntityInfo
{
    [Collection("Register Dependency")]
    public class CompanyInfoRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly ICompanyInfoRepository _companyInfoRepository;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly Mock<ISmartyStreetsUtility> _smartyStreetUtilityMock;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public CompanyInfoRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _smartyStreetUtilityMock = bootstrap.ServiceProvider.GetService<Mock<ISmartyStreetsUtility>>();
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _companyInfoRepository = bootstrap.ServiceProvider.GetService<ICompanyInfoRepository>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _mapper = bootstrap.ServiceProvider.GetService<IMapper>();
            _globalRepositoryMock.Reset();
            _dataRepositoryMock.Reset();
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Method to get the list of entityLoanApplicationMapping.
        /// </summary>
        private List<EntityLoanApplicationMapping> GetEntityLoanApplicationMappingList()
        {
            return new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    Id=Guid.NewGuid(),
                    Entity=new Entity()
                    {
                        Id=Guid.NewGuid(),
                        Company=new Company()
                        {
                            Id=Guid.NewGuid(),
                            Name="ABC",
                            CompanyType=new CompanyType()
                            {
                                Id=Guid.NewGuid(),
                                Name="ABC"
                            },
                            SICIndustryType=new SICIndustryType()
                            {
                                Id=Guid.NewGuid(),
                                IndustryTitle="ABC",
                                SICCode="ABC"
                            },
                            BusinessAge=new BusinessAge()
                            {
                                Id=Guid.NewGuid(),
                                Age="ABC"
                            },
                            EmployeeStrength=new EmployeeStrength()
                            {
                                Id=Guid.NewGuid(),
                                Strength="ABC"
                            },
                            CIN="ABC",
                            EmployeeStrengthId=Guid.NewGuid(),
                            CompanyTypeId=Guid.NewGuid(),
                            BusinessAgeId=Guid.NewGuid()
                        },
                        Address=new Address()
                        {
                            Id=Guid.NewGuid()
                        }
                    },
                    LoanApplicationId=Guid.NewGuid(),
                    EntityId=Guid.NewGuid()
                },
                new EntityLoanApplicationMapping()
                {
                    Id=Guid.NewGuid(),
                    Entity=new Entity()
                    {
                        Id=Guid.NewGuid(),
                        Company=new Company()
                        {
                            Id=Guid.NewGuid(),
                            Name="ABCSSDS",
                            CompanyType=new CompanyType()
                            {
                                Id=Guid.NewGuid(),
                                Name="ABSSDDC"
                            },
                            SICIndustryType=new SICIndustryType()
                            {
                                Id=Guid.NewGuid(),
                                IndustryTitle="ABCSDDD",
                                SICCode="ABCDDD"
                            },
                            BusinessAge=new BusinessAge()
                            {
                                Id=Guid.NewGuid(),
                                Age="ABC"
                            },
                            EmployeeStrength=new EmployeeStrength()
                            {
                                Id=Guid.NewGuid(),
                                Strength="ABC"
                            },
                            CIN="ABC",
                            EmployeeStrengthId=Guid.NewGuid(),
                            CompanyTypeId=Guid.NewGuid(),
                            BusinessAgeId=Guid.NewGuid()
                        },
                        Address=new Address()
                        {
                            Id=Guid.NewGuid()
                        }
                    },
                    LoanApplicationId=Guid.NewGuid(),
                    EntityId=Guid.NewGuid()
                }
            };
        }

        /// <summary>
        /// Get the object of AddressAC class.
        /// </summary>
        /// <returns>Returns object of AddressAC class</returns>
        private AddressAC GetAddress()
        {
            return new AddressAC
            {
                Id = Guid.NewGuid(),
                City = "Belmont",
                StreetLine = "86 frontage rd",
                StateAbbreviation = "MA",
                ZipCode = "361111"
            };
        }

        /// <summary>
        /// Get the single sole trader as shareholder
        /// </summary>
        /// <returns>Returns List of UserAC class</returns>
        private List<UserAC> GetSingleShareHolders()
        {
            return new List<UserAC>()
            {
                new UserAC
                {
                    Email = "arjun@lendingplatform.com",
                    Id = Guid.NewGuid(),
                    Name = "Arjunsinh Jadeja",
                    Phone = "+919999999999",
                    SSN = "898989898",
                    Relationship = StringConstant.SoleTrader,
                    SharePercentage = null
                }
            };
        }

        /// <summary>
        /// Get CompanyInfoAC object of company type sole trader
        /// </summary>
        /// <returns>Return onject of CompanyInfoAC</returns>
        private CompanyInfoAC GetSoleTraderCompanyInfo()
        {
            return new CompanyInfoAC()
            {
                Address = GetAddress(),
                CIN = "369258178",
                BusinessAgeId = Guid.NewGuid(),
                EmployeeStrengthId = Guid.NewGuid(),
                CompanyTypeId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                SICCode = "2364",
                Name = "google",
                ShareHolderUsers = GetSingleShareHolders()
            };
        }

        /// <summary>
        /// Get CompanyInfoAC object of company type registered entity company type
        /// </summary>
        /// <returns>Return onject of CompanyInfoAC</returns>
        private CompanyInfoAC GetRegisteredEntityCompanyInfo()
        {
            return new CompanyInfoAC()
            {
                Address = GetAddress(),
                CIN = "369258178",
                BusinessAgeId = Guid.NewGuid(),
                EmployeeStrengthId = Guid.NewGuid(),
                CompanyTypeId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                SICCode = "2364",
                Name = "google",
                ShareHolderUsers = GetRegisteredEntityShareHolders()
            };
        }

        /// <summary>
        /// Get CompanyInfoAC object of company type partnership company type
        /// </summary>
        /// <returns>Return onject of CompanyInfoAC</returns>
        private CompanyInfoAC GetPartnershipCompanyInfo()
        {
            return new CompanyInfoAC()
            {
                Address = GetAddress(),
                CIN = "469250099",
                BusinessAgeId = Guid.NewGuid(),
                EmployeeStrengthId = Guid.NewGuid(),
                CompanyTypeId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                SICCode = "2994",
                Name = "microsoft",
                ShareHolderUsers = GetPartnershipShareHolders()
            };
        }

        /// <summary>
        /// Get the shareholders of registered entity company type
        /// </summary>
        /// <returns>Returns List of UserAC class</returns>
        private List<UserAC> GetRegisteredEntityShareHolders()
        {
            return new List<UserAC>()
            {
                new UserAC
                {
                    Email = "arjun@lendingplatform.com",
                    Id = Guid.NewGuid(),
                    Name = "Arjunsinh Jadeja",
                    Phone = "+919999999999",
                    SSN = "898989898",
                    Relationship = StringConstant.RegisteredEntity,
                    SharePercentage = 50
                },
                new UserAC
                {
                    Email = "karan@lendingplatform.com",
                    Name = "Karan Desai",
                    Phone = "+919999999900",
                    SSN = "898989800",
                    Relationship = StringConstant.RegisteredEntity,
                    SharePercentage = 50
                }
            };
        }

        /// <summary>
        /// Get the shareholders of partnership company type
        /// </summary>
        /// <returns>Returns List of UserAC class</returns>
        private List<UserAC> GetPartnershipShareHolders()
        {
            return new List<UserAC>()
            {
                new UserAC
                {
                    Email = "arjun@lendingplatform.com",
                    Id = Guid.NewGuid(),
                    Name = "Arjunsinh Jadeja",
                    Phone = "+919999999999",
                    SSN = "898989898",
                    Relationship = StringConstant.Partner,
                    SharePercentage = 50
                },
                new UserAC
                {
                    Email = "karan@lendingplatform.com",
                    Name = "Karan Desai",
                    Phone = "+919999999900",
                    SSN = "898989800",
                    Relationship = StringConstant.Partner,
                    SharePercentage = 50
                }
            };
        }

        /// <summary>
        /// Get all company type list
        /// </summary>
        /// <returns>Return list of CompanyType class</returns>
        private List<CompanyType> GetCompanyTypes()
        {
            return new List<CompanyType>
            {
                    new CompanyType
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.SoleTraders
                    },
                    new CompanyType
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.RegisteredEntity
                    },
                    new CompanyType
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.Partnership
                    }
            };
        }

        /// <summary>
        /// Get all relationship list
        /// </summary>
        /// <returns>Return list of Relationship class</returns>
        private List<Relationship> GetRelationships()
        {
            return new List<Relationship>
            {
                    new Relationship
                    {
                        Id = Guid.NewGuid(),
                        Relation = StringConstant.SoleTrader
                    },
                    new Relationship
                    {
                        Id = Guid.NewGuid(),
                        Relation = StringConstant.Partner
                    },
                    new Relationship
                    {
                        Id = Guid.NewGuid(),
                        Relation = StringConstant.Shareholder
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
        /// Get Already saved address
        /// </summary>
        /// <returns>Returns object of Address class</returns>
        private Address GetExistingAddress()
        {
            return new Address
            {
                Id = Guid.NewGuid(),
                City = "Belmont",
                StreetLine = "frontage rd",
                StateAbbreviation = "NY",
                PrimaryNumber = "86",
                ZipCode = "361111"
            };
        }


        /// <summary>
        /// Get SICIndustryType object
        /// </summary>
        /// <returns>Returns object of SICIndustryType class</returns>
        private SICIndustryType GetSICIndustryType()
        {
            return new SICIndustryType
            {
                Id = Guid.NewGuid(),
                IndustryTitle = "Wheat farming",
                SICCode = "23133"
            };
        }

        /// <summary>
        /// Get object of Company
        /// </summary>
        /// <returns>Returns object of Company class</returns>
        private Company GetCompany()
        {
            return new Company
            {
                Id = Guid.NewGuid(),
                CIN = "369258178",
                BusinessAgeId = Guid.NewGuid(),
                EmployeeStrengthId = Guid.NewGuid(),
                CompanyTypeId = Guid.NewGuid(),
                SICIndustryTypeId = Guid.NewGuid(),
                Name = "amazon",
            };
        }

        /// <summary>
        /// Get list of EntityRelationshipMapping class
        /// </summary>
        /// <returns>Returns List of EntityRelationshipMapping class</returns>
        public List<EntityRelationshipMapping> GetEntityRelationshipMappingList()
        {
            return new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 50
                },
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelationshipId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    SharePercentage = 50
                }
            };
        }

        /// <summary>
        /// Get list of LoanApplication class
        /// </summary>
        /// <returns>Returns list of LoanApplication class</returns>
        public List<LoanApplication> GetLoanAppicationList()
        {
            return new List<LoanApplication>()
            {
                new LoanApplication
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    LoanAmount = 50,
                    LoanApplicationNumber = "56644020",
                    LoanPeriod = 200000,
                    LoanPurposeId = Guid.NewGuid(),
                    LoanApplicationStatusId = Guid.NewGuid(),
                    LoanApplicationStatus = new LoanApplicationStatus
                    {
                        SectionName = StringConstant.SectionNames.Single(x => x.Key == 2).Value
                    }
                }
            };
        }

        /// <summary>
        /// Get object of EntityLoanApplicationMapping
        /// </summary>
        /// <returns>Returns object of EntityLoanApplicationMapping</returns>
        private EntityLoanApplicationMapping GetEntityLoanApplicationMapping()
        {
            return new EntityLoanApplicationMapping
            {
                EntityId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid()
            };
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Test method to verify count of companies linked with user when linked companies exist.
        /// </summary>
        [Fact]
        public async Task GetAllCompaniesOfUserByUserIdAsync_LinkedCompanyExists_VerifyCountOfCompanies()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    PrimaryEntity = new Entity()
                    {
                        Id = userId,
                        Type = EntityType.Company,
                        Company = new Company()
                        {
                            Name = "Promact"
                        }
                    },
                    RelativeEntityId = userId,
                    RelationshipId = Guid.NewGuid(),
                    SharePercentage = 20
                },
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    PrimaryEntity = new Entity()
                    {
                        Id = userId,
                        Type = EntityType.Company,
                        Company = new Company()
                        {
                            Name = "Zomato"
                        }
                    },
                    RelativeEntityId = userId,
                    RelationshipId = Guid.NewGuid(),
                    SharePercentage = 40
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act
            List<CompanyAC> actualList = await _companyInfoRepository.GetAllCompaniesOfUserByUserIdAsync(userId);

            //Assert
            Assert.Equal(entityRelationshipMappings.Count, actualList.Count);
        }

        /// <summary>
        /// Test method to verify empty list of companies when linked companies don't exist.
        /// </summary>
        [Fact]
        public async Task GetAllCompaniesOfUserByUserIdAsync_LinkedCompanyNotExists_VerifyEmptyListOfCompanies()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var entityRelationshipMappings = new List<EntityRelationshipMapping>() { };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act
            List<CompanyAC> actualList = await _companyInfoRepository.GetAllCompaniesOfUserByUserIdAsync(userId);

            //Assert
            Assert.Empty(actualList);
        }

        /// <summary>
        /// Method to test company details by loan application id.
        /// </summary>
        [Fact]
        public async Task GetCompanyInfoAsync_ApplicationIdExist_ReturnCompanyACObject()
        {
            //Arrange
            var entityLoanApplicationList = GetEntityLoanApplicationMappingList();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
              .Returns(entityLoanApplicationList.AsQueryable().BuildMock().Object.Where(x => x.LoanApplicationId == entityLoanApplicationList[0].LoanApplicationId));

            //Act
            CompanyAC actual = await _companyInfoRepository.GetCompanyInfoAsync(entityLoanApplicationList[0].LoanApplicationId);

            //Assert
            Assert.Equal(entityLoanApplicationList[0].Entity.Company.CIN, actual.CIN);
        }

        /// <summary>
        /// Test case to check if user a try to access loan of another user then it throws ThrowsInvalidResourceAccessException
        /// </summary>
        [Fact]
        public async Task GetCompanyUsersAsync_UnauthorizedUser_AssertThrowsInvalidResourceAccessException()
        {
            //Arange
            Guid entityId = Guid.NewGuid();
            Guid currentUserId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _companyInfoRepository.GetCompanyUsersAsync(entityId, currentUserId));
        }

        /// <summary>
        /// Test case to check if no users are related to supplied enitityId than throws DataNotFoundException
        /// </summary>
        [Fact]
        public async Task GetCompanyUsersAsync_NoEnitityRelationshipMapping_AssertThrowsDataNotFoundException()
        {
            //Arange
            Guid entityId = Guid.NewGuid();
            Guid currentUserId = Guid.NewGuid();

            List<EntityRelationshipMapping> entityRelationshipMapping = new List<EntityRelationshipMapping> { };

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMapping.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(() => _companyInfoRepository.GetCompanyUsersAsync(entityId, currentUserId));
        }

        /// <summary>
        /// Test case to check if method returns users list related to entity
        /// </summary>
        [Fact]
        public async Task GetCompanyUsersAsync_EntityRelationshipExists_VerifyCountOfUsers()
        {
            //Arange
            Guid entityId = Guid.NewGuid();
            Guid currentUserId = Guid.NewGuid();

            User user = _mapper.Map<UserAC, User>(GetSingleShareHolders().Single());
            user.Id = currentUserId;

            List<EntityRelationshipMapping> entityRelationshipMapping = new List<EntityRelationshipMapping> {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityId,
                    RelationshipId= Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid(),
                    RelativeEntity = new Entity() {
                      User = user,
                      AddressId = null,
                      Type = EntityType.User,
                      Id = currentUserId
                    },
                    Relationship = new Relationship()
                    {
                        Id = Guid.NewGuid(),
                        Relation = StringConstant.SoleTrader
                    },
                    SharePercentage = null
                }
            };

            List<UserAC> expectedList = GetSingleShareHolders();
            expectedList.Single().Id = currentUserId;

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMapping.AsQueryable().BuildMock().Object);

            //Act
            List<UserAC> actualList = await _companyInfoRepository.GetCompanyUsersAsync(entityId, currentUserId);

            //Assert
            Assert.Equal(actualList.Count, expectedList.Count);
        }

        /// <summary>
        /// Test case to check if method returns all company info options
        /// </summary>
        [Fact]
        public async Task GetCompanyInfoOptionAsync_GetAllOptions_VerifyAllOptionsCount()
        {
            //Arrange

            List<BusinessAge> businessAgeList = new List<BusinessAge>()
            {
                new BusinessAge
                {
                     Age = StringConstant.SixMonthToOneYear,
                     Id = Guid.NewGuid(),
                     Order = 1
                }
            };

            List<CompanyType> companyTypeList = new List<CompanyType>()
            {
                new CompanyType
                {
                    Id = Guid.NewGuid(),
                    Name = StringConstant.RegisteredEntity,
                    Order = 1
                }
            };

            List<EmployeeStrength> employeeStrengthList = new List<EmployeeStrength>()
            {
                new EmployeeStrength
                {
                    Id = Guid.NewGuid(),
                    Strength = StringConstant.TenToFifty,
                    Order = 1
                }
            };

            CompanyInfoOptionAC expected = new CompanyInfoOptionAC();

            List<BusinessAgeAC> businessAgeAC = _mapper.Map<List<BusinessAge>, List<BusinessAgeAC>>(businessAgeList.ToList());
            expected.BusinessAgeList = businessAgeAC;

            List<CompanyTypeAC> companyTypeAC = _mapper.Map<List<CompanyType>, List<CompanyTypeAC>>(companyTypeList.ToList());
            expected.CompanyTypeList = companyTypeAC;

            List<EmployeeStrengthAC> employeeStrengthAC = _mapper.Map<List<EmployeeStrength>, List<EmployeeStrengthAC>>(employeeStrengthList.ToList());
            expected.EmployeeStrengthList = employeeStrengthAC;


            _dataRepositoryMock.Setup(x => x.GetAll<BusinessAge>())
                .Returns(businessAgeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<EmployeeStrength>())
                .Returns(employeeStrengthList.AsQueryable().BuildMock().Object);
            //Act
            var actual = await _companyInfoRepository.GetCompanyInfoOptionAsync();

            //Assert
            Assert.Equal(expected.BusinessAgeList.Count, actual.BusinessAgeList.Count);
            Assert.Equal(expected.CompanyTypeList.Count, actual.CompanyTypeList.Count);
            Assert.Equal(expected.EmployeeStrengthList.Count, actual.EmployeeStrengthList.Count);
        }

        /// <summary>
        /// Test case to check if user a try to access loan of another user then it throws InvalidResourceAccessException
        /// </summary>
        [Fact]
        public async Task GetAddressAsync_UnauthorizedUser_AssertThrowsInvalidResourceAccessException()
        {
            //Arange
            Guid entityId = Guid.NewGuid();
            Guid currentUserId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _companyInfoRepository.GetAddressAsync(entityId, currentUserId));
        }

        /// <summary>
        /// Test case to check if address not found then it throws DataNotFoundException
        /// </summary>
        [Fact]
        public async Task GetAddressAsync_AddressNotExist_AssertThrowsDataNotFoundException()
        {
            //Arange
            Guid entityId = Guid.NewGuid();

            Guid currentUserId = Guid.NewGuid();

            List<Entity> entities = new List<Entity>();

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _companyInfoRepository.GetAddressAsync(entityId, currentUserId));
        }

        /// <summary>
        /// Test case to check if address found then returns valid address
        /// </summary>
        [Fact]
        public async Task GetAddressAsync_AddressExist_AssertAddessIdAndStreetLine()
        {
            //Arange
            Guid entityId = Guid.NewGuid();

            Guid currentUserId = Guid.NewGuid();

            AddressAC expected = GetAddress();

            List<Entity> entities = new List<Entity>()
            {
                new Entity
                {
                    Id = entityId,
                    Type = EntityType.Company,
                    AddressId = expected.Id,
                    Address = new Address
                    {
                        Id = expected.Id,
                        City = "Belmont",
                        StreetLine = "frontage rd",
                        StateAbbreviation = "NY",
                        PrimaryNumber = "86",
                        ZipCode = "361111"
                    }
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entities.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _companyInfoRepository.GetAddressAsync(entityId, currentUserId);

            //Assert
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.StreetLine, actual.StreetLine);
        }

        /// <summary>
        /// Test case to check if user a try to access loan of another user then it throws InvalidResourceAccessException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_UnauthorizedUser_AssertThrowsInvalidResourceAccessException()
        {
            //Arange
            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();

            User user = _mapper.Map<UserAC, User>(GetSingleShareHolders().Single());

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), false))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));
        }

        /// <summary>
        /// Test case to check if company EIN format is invalid then it throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_InvalidCompanyEIN_AssertThrowsValidationException()
        {
            //Arange
            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            companyInfo.CIN = "1234567890";

            User user = _mapper.Map<UserAC, User>(GetSingleShareHolders().Single());

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));
        }

        /// <summary>
        /// Test  case to check if shareholders of company has duplicate email addresses then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_DuplicateEmail_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            foreach (var shareholder in companyInfo.ShareHolderUsers)
            {
                shareholder.Email = "arjun@lendingplatform.com";
            }

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));
        }

        /// <summary>
        /// Test  case to check if shareholders of company has duplicate phone numbers then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_DuplicatePhoneNumber_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            foreach (var shareholder in companyInfo.ShareHolderUsers)
            {
                shareholder.Phone = "9999999999";
            }

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));
        }

        /// <summary>
        /// Test  case to check if shareholders of company has not unique phone numbers then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_NotUniquePhoneNumber_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()))
                .ThrowsAsync(new ValidationException());

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));
        }

        /// <summary>
        /// Check if company info Add operation is not allowed then throws InvalidResourceAccessException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndAddOperationNotAllowed_AssertThrowsInvalidResourceAccessException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            //Act

            //Assert

            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// Check if company ein is not unique then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndNotUniqueCompanyEIN_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// Check if multiple shareholders are linked with company type sole trader then throws InvalidDataException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndMultipleShareholderWithSoleTraderCompanyType_AssertThrowsInvalidDataException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            //Act

            //Assert

            await Assert.ThrowsAsync<InvalidDataException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// Check if sole trader tin is invalid then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndSoleTraderInvalidTIN_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetSingleShareHolders().Single());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();

            companyInfo.ShareHolderUsers.Single().SSN = "1234567891";

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(false);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// Check if registered company type has shareholder percentage is not in range of 0 to 100 then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndInvalidSharePercentageRange_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();
            companyInfo.ShareHolderUsers.First().SharePercentage = 120;

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// Check if registered company type has shareholder percentage total is not 100 then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndInvalidTotalSharePercentage_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();
            companyInfo.ShareHolderUsers.First().SharePercentage = 20;

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// Check if address is invalid then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndInvalidAddress_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns<SmartyStreets.USStreetApi.Candidate>(null);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// Check if SICCode of industry type not exists then throws DataNotFoundException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndIndustryTypeSICCodeNotExist_AssertThrowsDataNotFoundException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync((SICIndustryType)null);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act

            //Assert

            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);

        }

        /// <summary>
        /// Check if company type is sole trader is not current user then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndSoleTraderNotCurrentUser_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetSingleShareHolders().Single());
            user.Email = "karan@lendingplatform.com";

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            SICIndustryType sicIndustryType = GetSICIndustryType();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);

        }

        /// <summary>
        /// Check if entity loan application mapping not found then throws DataNotFoundException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndEntityLoanMappingNotFound_AssetThrowsDataNotFoundException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetSingleShareHolders().Single());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            SICIndustryType sicIndustryType = GetSICIndustryType();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync((EntityLoanApplicationMapping)null);

            //Act

            //Assert

            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Once);

        }

        /// <summary>
        /// Check if company info of company type sole trader is added.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndSoleTraderCompanyAdded_AssertCompanyName()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetSingleShareHolders().Single());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            SICIndustryType sicIndustryType = GetSICIndustryType();

            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.EntityId = user.Id;
            entityLoanApplicationMapping.LoanApplicationId = companyInfo.LoanApplicationId;

            CompanyInfoAC expected = GetSoleTraderCompanyInfo();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);

            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            CompanyInfoAC actual = await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

            Assert.Equal(expected.Name, actual.Name);

        }

        /// <summary>
        /// Check if registered company type does not has atleast two shareholders then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndOneShareHolderWithRegisteredEntityCompanyType_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            companyInfo.ShareHolderUsers = GetSingleShareHolders();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// Check if company type is not valid then throws InvalidDataException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndInvalidCompanyType_AssertThrowsInvalidDataException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = Guid.NewGuid();

            List<Relationship> relationshipList = GetRelationships();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First())
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            //Act

            //Assert

            await Assert.ThrowsAsync<InvalidDataException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Exactly(2));

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<User>>()), Times.Once);
        }

        /// <summary>
        /// Check if registered entity company type is added
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndRegisteredEntityCompanyTypeAdded_AssertCompanyName()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First())
            };

            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.LoanApplicationId = companyInfo.LoanApplicationId;
            entityLoanApplicationMapping.EntityId = user.Id;

            CompanyInfoAC expected = GetRegisteredEntityCompanyInfo();
            expected.CompanyTypeId = companyInfo.CompanyTypeId;

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);

            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            CompanyInfoAC actual = await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Exactly(2));

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<User>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityRelationshipMapping>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

            Assert.Equal(actual.CompanyTypeId, expected.CompanyTypeId);

            Assert.Equal(actual.Name, expected.Name);
        }

        /// <summary>
        /// Check if partnership company type is added
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNullAndPartnershipCompanyTypeAdded_AssertCompanyName()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetPartnershipShareHolders().First());

            CompanyInfoAC companyInfo = GetPartnershipCompanyInfo();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.Partnership)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First())
            };

            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.LoanApplicationId = companyInfo.LoanApplicationId;
            entityLoanApplicationMapping.EntityId = user.Id;

            CompanyInfoAC expected = GetPartnershipCompanyInfo();
            expected.CompanyTypeId = companyInfo.CompanyTypeId;

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);

            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            CompanyInfoAC actual = await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Exactly(2));

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<User>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityRelationshipMapping>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

            Assert.Equal(actual.CompanyTypeId, expected.CompanyTypeId);

            Assert.Equal(actual.Name, expected.Name);
        }

        /// <summary>
        /// Check if company info Add operation is not allowed then throws InvalidResourceAccessException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndAddOperationNotAllowed_AssertThrowsInvalidResourceAccessException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            //Act

            //Assert

            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// Check if company EIN is not unique then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndCompanyEINNotUnique_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Company company = GetCompany();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync(company);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if registered company type does not has atleast two shareholders then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndOneShareHolderWithRegisteredEntityCompanyType_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();
            companyInfo.ShareHolderUsers = GetSingleShareHolders();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if registered company type has shareholder percentage is not in range of 0 to 100 then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndInvalidSharePercentageRange_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();
            companyInfo.ShareHolderUsers.First().SharePercentage = 120;

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if registered company type has shareholder percentage total is not 100 then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndInvalidTotalSharePercentage_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();
            companyInfo.ShareHolderUsers.First().SharePercentage = 20;

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if multiple shareholders are linked with company type sole trader then throws InvalidDataException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndMultipleShareholderWithSoleTraderCompanyType_AssertThrowsInvalidDataException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            companyInfo.Id = Guid.NewGuid();
            companyInfo.ShareHolderUsers = GetRegisteredEntityShareHolders();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);

            //Act

            //Assert

            await Assert.ThrowsAsync<InvalidDataException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if sole trader tin is invalid then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndSoleTraderInvalidTIN_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            companyInfo.Id = Guid.NewGuid();
            companyInfo.ShareHolderUsers.Single().SSN = "1234567890";

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(false);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if address does not already exist for provided company then throws DataNotFoundException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndAddressNotExist_AssertThrowsDataNotFoundException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync((Address)null);

            //Act

            //Assert

            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if address is invalid then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndInvalidAddress_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns<SmartyStreets.USStreetApi.Candidate>(null);

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if supplied company id is invalid then throws DataNotFoundExpection
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndInvalidCompanyId_AssertThrowsDataNotFoundExpection()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            companyInfo.Id = Guid.NewGuid();
            companyInfo.Address.StateAbbreviation = "MA";

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync((Company)null);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            //Act

            //Assert

            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if SICCode of industry type not exists then throws DataNotFoundException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndIndustryTypeSICCodeNotExist_AssertThrowsDataNotFoundException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            Company company = GetCompany();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync(company);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync((SICIndustryType)null);

            //Act

            //Assert

            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if company type is sole trader is not current then throws ValidationException
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndSoleTraderNotCurrentUser_AssertThrowsValidationException()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());
            user.Email = "karan@lendingplatform.com";

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            Company company = GetCompany();

            SICIndustryType sicIndustryType = GetSICIndustryType();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync(company);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act

            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);
        }

        /// <summary>
        /// For update company info check if company type is sole trader added sucessfully.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndSoleTraderCompanyAdded_AssertCompanyName()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            Company company = GetCompany();

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<EntityRelationshipMapping> entityRelationshipMappingList = GetEntityRelationshipMappingList();

            CompanyInfoAC expected = GetSoleTraderCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.EntityId = companyInfo.Id.Value;

            List<LoanApplication> loanApplication = GetLoanAppicationList();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync(company);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            List<EntityRelationshipMapping> entityRelationshipMappingNull = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.SetupSequence(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappingNull.AsQueryable().BuildMock().Object)
                .Returns(entityRelationshipMappingList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplication.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);
            //Act
            CompanyInfoAC actual = await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<List<EntityRelationshipMapping>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityRelationshipMapping>()), Times.Once);

            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

            Assert.Equal(expected.Name, actual.Name);
        }

        /// <summary>
        /// For update company info check if company type Id is invalid then throws Exception
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndInvalidCompanyTypeId_AssertCompanyName()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = Guid.NewGuid();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            Company company = GetCompany();
            company.Id = companyInfo.Id.Value;

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First())
            };

            companyInfo.Id = Guid.NewGuid();

            List<EntityRelationshipMapping> entityRelationshipMappingList = GetEntityRelationshipMappingList();


            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync(company);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappingList.AsQueryable().BuildMock().Object);

            //Act

            //Assert

            await Assert.ThrowsAsync<InvalidDataException>(async () => await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user));

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<User>>()), Times.Once);

        }

        /// <summary>
        /// For update company info check if registered entity company type is added sucessfully
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndRegisteredEntityCompany_AssertCompanyName()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            Company company = GetCompany();
            company.Id = companyInfo.Id.Value;

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First())
            };

            CompanyInfoAC expected = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.EntityId = companyInfo.Id.Value;

            List<EntityRelationshipMapping> entityRelationshipMappingList = GetEntityRelationshipMappingList();

            List<LoanApplication> loanApplication = GetLoanAppicationList();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync(company);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappingList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplication.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);

            //Act
            CompanyInfoAC actual = await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityRelationshipMapping>(It.IsAny<List<EntityRelationshipMapping>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<User>>()), Times.Once);

            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

            Assert.Equal(actual.Name, expected.Name);
        }

        /// <summary>
        /// For update company info check if registered entity company type is added sucessfully and there was no change in shareholders
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndNoChangeInShareholders_VerifyRemoveRangeEntityRelationshipMapping()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            Company company = GetCompany();
            company.Id = companyInfo.Id.Value;

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First())
            };

            CompanyInfoAC expected = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.EntityId = companyInfo.Id.Value;

            List<EntityRelationshipMapping> entityRelationshipMappingList = new List<EntityRelationshipMapping>();

            List<LoanApplication> loanApplication = GetLoanAppicationList();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync(company);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappingList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplication.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);


            //Act
            CompanyInfoAC actual = await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityRelationshipMapping>(It.IsAny<List<EntityRelationshipMapping>>()), Times.Never);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<User>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

            Assert.Equal(actual.Name, expected.Name);
        }

        /// <summary>
        /// For update company info check if registered entity company type is added sucessfully and if section name is different
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndDifferentSectionName_VerifyUpdateAndFetchCall()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            Company company = GetCompany();
            company.Id = companyInfo.Id.Value;

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First())
            };

            CompanyInfoAC expected = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<EntityRelationshipMapping> entityRelationshipMappingList = new List<EntityRelationshipMapping>();

            List<LoanApplication> loanApplication = GetLoanAppicationList();
            loanApplication.Single().LoanApplicationStatus.SectionName = StringConstant.SectionNames.Single(x => x.Key == 3).Value;

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync(company);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappingList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplication.AsQueryable().BuildMock().Object);


            //Act
            CompanyInfoAC actual = await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityRelationshipMapping>(It.IsAny<List<EntityRelationshipMapping>>()), Times.Never);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<User>>()), Times.Once);

            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);

            _dataRepositoryMock.Verify(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Never);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityLoanApplicationMapping>()), Times.Never);

            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

            Assert.Equal(actual.Name, expected.Name);
        }

        /// <summary>
        /// For update company info check if registered entity company type is added sucessfully and if company Id already link with loanApplicationId in LoamApplicationMapping
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndAlreadyLinkedCompanyIdWithLoan_VerifyUpdateCall()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First());

            CompanyInfoAC companyInfo = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.RegisteredEntity)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            Company company = GetCompany();
            company.Id = companyInfo.Id.Value;

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First())
            };

            CompanyInfoAC expected = GetRegisteredEntityCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<EntityRelationshipMapping> entityRelationshipMappingList = new List<EntityRelationshipMapping>();

            List<LoanApplication> loanApplication = GetLoanAppicationList();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync(company);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappingList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplication.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync((EntityLoanApplicationMapping)null);


            //Act
            CompanyInfoAC actual = await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityRelationshipMapping>(It.IsAny<List<EntityRelationshipMapping>>()), Times.Never);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<User>>()), Times.Once);

            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityLoanApplicationMapping>()), Times.Never);

            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

            Assert.Equal(actual.Name, expected.Name);
        }

        /// <summary>
        /// For update company info check if partnership company type is added sucessfully
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_CompanyIdNotNullAndPartnershipCompany_AssertCompanyName()
        {
            //Arange

            User user = _mapper.Map<UserAC, User>(GetPartnershipShareHolders().First());

            CompanyInfoAC companyInfo = GetPartnershipCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            List<CompanyType> companyTypeList = GetCompanyTypes();

            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.Partnership)).Select(s => s.Id).Single();

            List<Relationship> relationshipList = GetRelationships();

            Address address = GetExistingAddress();

            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();

            Company company = GetCompany();
            company.Id = companyInfo.Id.Value;

            SICIndustryType sicIndustryType = GetSICIndustryType();

            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First())
            };

            CompanyInfoAC expected = GetPartnershipCompanyInfo();
            companyInfo.Id = Guid.NewGuid();

            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.EntityId = companyInfo.Id.Value;

            List<EntityRelationshipMapping> entityRelationshipMappingList = GetEntityRelationshipMappingList();

            List<LoanApplication> loanApplication = GetLoanAppicationList();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);

            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync((Company)null)
                .ReturnsAsync(company);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<Address>(It.IsAny<Expression<Func<Address, bool>>>()))
                .ReturnsAsync(address);

            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);

            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);

            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappingList.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplication.AsQueryable().BuildMock().Object);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);

            //Act
            CompanyInfoAC actual = await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert

            _globalRepositoryMock.Verify(x => x.IsUniquePhoneNumberAsync(It.IsAny<List<string>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Company>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<User>>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityRelationshipMapping>(It.IsAny<List<EntityRelationshipMapping>>()), Times.Once);

            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);

            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

            Assert.Equal(actual.Name, expected.Name);
        }
        
        /// <summary>
        /// Check if shareholder of company type sole trader does not exists then is added.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_ShareHolderConsentNotExists_AssertShareHolderIsAdded()
        {
            //Arange
            User user = _mapper.Map<UserAC, User>(GetSingleShareHolders().Single());
            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            List<CompanyType> companyTypeList = GetCompanyTypes();
            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();
            List<Relationship> relationshipList = GetRelationships();
            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();
            SICIndustryType sicIndustryType = GetSICIndustryType();
            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.EntityId = user.Id;
            entityLoanApplicationMapping.LoanApplicationId = companyInfo.LoanApplicationId;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);
            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);
            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Check if shareholder of company type sole trader exists then doesn't add.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_ShareHolderConsentExists_AssertShareHolderNotAdded()
        {
            //Arange
            User user = _mapper.Map<UserAC, User>(GetSingleShareHolders().Single());
            CompanyInfoAC companyInfo = GetSoleTraderCompanyInfo();
            List<CompanyType> companyTypeList = GetCompanyTypes();
            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.SoleTraders)).Select(s => s.Id).Single();
            List<Relationship> relationshipList = GetRelationships();
            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();
            SICIndustryType sicIndustryType = GetSICIndustryType();
            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.EntityId = user.Id;
            entityLoanApplicationMapping.LoanApplicationId = companyInfo.LoanApplicationId;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    UserId = user.Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);
            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);
            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Check if same shareholders of company exist then doesn't add any shareholder.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_ShareHoldersConsentsExist_AssertShareHoldersNotAdded()
        {
            //Arange
            User user = _mapper.Map<UserAC, User>(GetPartnershipShareHolders().First());
            CompanyInfoAC companyInfo = GetPartnershipCompanyInfo();
            List<CompanyType> companyTypeList = GetCompanyTypes();
            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.Partnership)).Select(s => s.Id).Single();
            List<Relationship> relationshipList = GetRelationships();
            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();
            SICIndustryType sicIndustryType = GetSICIndustryType();
            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First()),
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().Last())
            };
            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.LoanApplicationId = companyInfo.LoanApplicationId;
            entityLoanApplicationMapping.EntityId = user.Id;
            CompanyInfoAC expected = GetPartnershipCompanyInfo();
            expected.CompanyTypeId = companyInfo.CompanyTypeId;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    UserId = registeredUserList.First().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                },
                new EntityLoanApplicationConsent
                {
                    UserId = registeredUserList.Last().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);
            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);
            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Check if some shareholders of company exist then doesn't add all shareholders and remove other existing shareholders.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_SomeShareHoldersConsentsExist_AssertShareHoldersAddRemoveOperations()
        {
            //Arange
            User user = _mapper.Map<UserAC, User>(GetPartnershipShareHolders().First());
            CompanyInfoAC companyInfo = GetPartnershipCompanyInfo();
            List<CompanyType> companyTypeList = GetCompanyTypes();
            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.Partnership)).Select(s => s.Id).Single();
            List<Relationship> relationshipList = GetRelationships();
            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();
            SICIndustryType sicIndustryType = GetSICIndustryType();
            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First()),
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().Last()),
                _mapper.Map<UserAC, User>(GetSingleShareHolders().First()),
            };
            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.LoanApplicationId = companyInfo.LoanApplicationId;
            entityLoanApplicationMapping.EntityId = user.Id;
            CompanyInfoAC expected = GetPartnershipCompanyInfo();
            expected.CompanyTypeId = companyInfo.CompanyTypeId;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    UserId = registeredUserList.First().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                },
                new EntityLoanApplicationConsent
                {
                    UserId = registeredUserList.Last().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                },
                new EntityLoanApplicationConsent
                {
                    UserId = GetSingleShareHolders().First().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);
            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);
            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Check if all shareholders of company exist then doesn't add shareholders and remove other existing shareholders.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_AllShareHoldersConsentsExist_AssertShareHoldersAddRemoveOperations()
        {
            //Arange
            User user = _mapper.Map<UserAC, User>(GetPartnershipShareHolders().First());
            CompanyInfoAC companyInfo = GetPartnershipCompanyInfo();
            List<CompanyType> companyTypeList = GetCompanyTypes();
            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.Partnership)).Select(s => s.Id).Single();
            List<Relationship> relationshipList = GetRelationships();
            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();
            SICIndustryType sicIndustryType = GetSICIndustryType();
            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First()),
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().Last()),
            };
            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.LoanApplicationId = companyInfo.LoanApplicationId;
            entityLoanApplicationMapping.EntityId = user.Id;
            CompanyInfoAC expected = GetPartnershipCompanyInfo();
            expected.CompanyTypeId = companyInfo.CompanyTypeId;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    UserId = registeredUserList.First().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                },
                new EntityLoanApplicationConsent
                {
                    UserId = registeredUserList.Last().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                },
                new EntityLoanApplicationConsent
                {
                    UserId = GetSingleShareHolders().First().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);
            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);
            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Check if some shareholders of company doesn't exist then add only new shareholders.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateAsync_SomeShareHoldersConsentsNotExist_AssertNewShareHoldersAdded()
        {
            //Arange
            User user = _mapper.Map<UserAC, User>(GetPartnershipShareHolders().First());
            CompanyInfoAC companyInfo = GetPartnershipCompanyInfo();
            List<CompanyType> companyTypeList = GetCompanyTypes();
            companyInfo.CompanyTypeId = companyTypeList.Where(c => c.Name.Equals(StringConstant.Partnership)).Select(s => s.Id).Single();
            List<Relationship> relationshipList = GetRelationships();
            SmartyStreets.USStreetApi.Candidate validatedAddress = GetValidatedAdress();
            SICIndustryType sicIndustryType = GetSICIndustryType();
            List<User> registeredUserList = new List<User>
            {
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().First()),
                _mapper.Map<UserAC, User>(GetRegisteredEntityShareHolders().Last()),
                _mapper.Map<UserAC, User>(GetSingleShareHolders().First())
            };
            EntityLoanApplicationMapping entityLoanApplicationMapping = GetEntityLoanApplicationMapping();
            entityLoanApplicationMapping.LoanApplicationId = companyInfo.LoanApplicationId;
            entityLoanApplicationMapping.EntityId = user.Id;
            CompanyInfoAC expected = GetPartnershipCompanyInfo();
            expected.CompanyTypeId = companyInfo.CompanyTypeId;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsent = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    UserId = registeredUserList.First().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                },
                new EntityLoanApplicationConsent
                {
                    UserId = registeredUserList.Last().Id,
                    LoanApplicationId = companyInfo.LoanApplicationId
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true))
                .ReturnsAsync(true);
            _globalRepositoryMock.SetupSequence(x => x.IsValidCIN(It.IsAny<string>()))
                .Returns(true)
                .Returns(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<CompanyType>())
                .Returns(companyTypeList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<Relationship>())
                .Returns(relationshipList.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsUniqueEINAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                .Returns(validatedAddress);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<SICIndustryType>(It.IsAny<Expression<Func<SICIndustryType, bool>>>()))
                .ReturnsAsync(sicIndustryType);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(registeredUserList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsent.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .ReturnsAsync(entityLoanApplicationMapping);
            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            await _companyInfoRepository.AddOrUpdateAsync(companyInfo, user);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.RemoveRange<EntityLoanApplicationConsent>(It.IsAny<List<EntityLoanApplicationConsent>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion
    }
}