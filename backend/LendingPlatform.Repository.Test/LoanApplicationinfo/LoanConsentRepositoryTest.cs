using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass.EntityInfo;
using LendingPlatform.Repository.ApplicationClass.LoanApplicationInfo;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Repository.Repository.LoanApplicationInfo;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using LendingPlatform.Utils.Utils.Transunion;
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

namespace LendingPlatform.Repository.Test.LoanApplicationinfo
{
    [Collection("Register Dependency")]
    public class LoanConsentRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly ILoanConsentRepository _loanConsentRepository;
        private readonly Mock<ISimpleEmailServiceUtility> _simpleEmailServiceUtilityMock;
        private readonly Mock<IExperianUtility> _experianUtilityMock;
        private readonly Mock<IEquifaxUtility> _equifaxUtilityMock;
        private readonly Mock<ITransunionUtility> _transunionUtilityMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IRulesRepository> _rulesRepositoryMock;
        #endregion

        #region Constructor
        public LoanConsentRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _loanConsentRepository = bootstrap.ServiceProvider.GetService<ILoanConsentRepository>();
            _simpleEmailServiceUtilityMock = bootstrap.ServiceProvider.GetService<Mock<ISimpleEmailServiceUtility>>();
            _experianUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IExperianUtility>>();
            _equifaxUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IEquifaxUtility>>();
            _transunionUtilityMock = bootstrap.ServiceProvider.GetService<Mock<ITransunionUtility>>();
            _configurationMock = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _rulesRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IRulesRepository>>();
            _dataRepositoryMock.Reset();
            _globalRepositoryMock.Reset();
            _equifaxUtilityMock.Reset();
            _transunionUtilityMock.Reset();
            _simpleEmailServiceUtilityMock.Reset();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get current logged in user details.
        /// </summary>
        /// <returns></returns>
        private User GetCurrentUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Email = "john@doe.com",
                Phone = "9898989898",
                SSN = "123456789"
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
                UserId = Guid.NewGuid(),
                LoanPurposeId = Guid.NewGuid(),
                LoanAmount = 100000,
                LoanApplicationStatusId = Guid.NewGuid(),
                LoanPeriod = 24,
                LoanApplicationNumber = "GHI5678KK"
            };
        }

        /// <summary>
        /// Get an object of EntityLoanApplicationConsentAC class.
        /// </summary>
        /// <returns>Returns object of EntityLoanApplicationConsentAC class</returns>
        private EntityLoanApplicationConsentAC GetEntityLoanApplicationConsentACObject()
        {
            return new EntityLoanApplicationConsentAC
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                User = new UserAC
                {
                    Id = Guid.NewGuid(),
                    Name = "Lorem",
                    Email = "lorem@epsum.com",
                    Phone = "7878787878",
                    SSN = "987654321",
                    SharePercentage = 20,
                    Relationship = "Partner"
                },
                IsConsentGiven = false
            };
        }

        /// <summary>
        /// Get list of EntityLoanApplicationMapping
        /// </summary>
        /// <returns>Returns list of EntityLoanApplicationMapping class</returns>
        private List<EntityLoanApplicationMapping> GetEntityLoanApplicationMappingList()
        {
            return new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = Guid.NewGuid(),
                    Entity = new Entity
                    {
                        Id = Guid.NewGuid(),
                        Type = DomainModel.Enums.EntityType.Company,
                        AddressId = Guid.NewGuid(),
                        Company = new Company
                        {
                            CompanyType = new CompanyType
                            {
                                Id = Guid.NewGuid(),
                                Name = StringConstant.SoleTraders,
                                Order = 1
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Get list of credit report.
        /// </summary>
        /// <returns>Returns list of CreditReport class</returns>
        private List<CreditReport> GetCreditReportList()
        {
            return new List<CreditReport>
            {
                new CreditReport
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    CommercialScore = 50,
                    FsrScore = 50,
                    LoanApplicationId = Guid.NewGuid()
                }
            };
        }

        /// <summary>
        /// Get list of entity.
        /// </summary>
        /// <returns>List of Entity objects</returns>
        private List<Entity> GetEntityList()
        {
            return new List<Entity>
            {
                new Entity
                {
                    Id = Guid.NewGuid(),
                    AddressId = Guid.NewGuid(),
                    Address = new Address
                    {
                        Id = Guid.NewGuid(),
                        City = "belmont",
                        PrimaryNumber = "86",
                        StreetLine ="frontage rd",
                        StateAbbreviation = "NY",
                        ZipCode = "361011"
                    },
                    User = new User
                    {
                        Name = "John k wick",
                        Email = "john@promact.com",
                        DOB = DateTime.Now,
                        Id = Guid.NewGuid(),
                        Phone= "9999999999",
                        SSN = "123456789"
                    },
                    Type = EntityType.User
                }
            };
        }

        /// <summary>
        /// Get Premier Profiles API response
        /// </summary>
        /// <returns>Returns object of PremierProfilesResponseAC</returns>
        private PremierProfilesResponseAC GetPremierProfilesResponse()
        {
            return new PremierProfilesResponseAC
            {
                JsonResponse = @"json response of premier profiles api",
                Results = new ResultsAC
                {
                    ScoreInformation = new ScoreInformationAC
                    {
                        CommercialScore = new CommercialScoreAC
                        {
                            Score = 50
                        },
                        FsrScore = new FsrScoreAC
                        {
                            Score = 50
                        }
                    },
                    ExpandedCreditSummary = new ExpandedCreditSummaryAC
                    {
                        BankruptcyIndicator = false,
                        JudgmentIndicator = false,
                        TaxLienIndicator = false
                    }
                }
            };
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Test method to verify count of Consent list returns when data exists in table.
        /// </summary>
        [Fact]
        public async Task GetConsentsAsync_ConsentsExist_ReturnsConsentList()
        {
            //Arrange
            var consents = new List<Consent>
            {
                new Consent
                {
                    Id = Guid.NewGuid(),
                    ConsentText = "Lorem Epsum 1"
                },
                new Consent
                {
                    Id = Guid.NewGuid(),
                    ConsentText = "Lorem Epsum 2"
                }
            };

            _dataRepositoryMock.Setup(x => x.GetAll<Consent>())
                .Returns(consents.AsQueryable().BuildMock().Object);

            //Act
            List<ConsentAC> actualList = await _loanConsentRepository.GetConsentsAsync();

            //Assert
            Assert.Equal(consents.Count, actualList.Count);
        }

        /// <summary>
        /// Test method to verify that it throws exception when data not exists in Consent table.
        /// </summary>
        [Fact]
        public async Task GetConsentsAsync_ConsentsNotExist_ThrowsError()
        {
            //Arrange
            var consents = new List<Consent>();

            _dataRepositoryMock.Setup(x => x.GetAll<Consent>())
                .Returns(consents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanConsentRepository.GetConsentsAsync());
        }

        /// <summary>
        /// Test method to verify that it throws error for a non-existing loan application.
        /// </summary>
        [Fact]
        public async Task GetUserConsentsByLoanApplicationIdAsync_LoanApplicationNotExist_ThrowsError()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            User currentUser = GetCurrentUser();
            LoanApplication loanApplication = null;

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(Task.FromResult(loanApplication));

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanConsentRepository.GetUserConsentsByLoanApplicationIdAsync(loanApplicationId, currentUser));
        }

        /// <summary>
        /// Test method to verify that it throws error when there are no any consents for a given loan application.
        /// </summary>
        [Fact]
        public async Task GetUserConsentsByLoanApplicationIdAsync_EntityConsentsNotExist_ThrowsError()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            User currentUser = GetCurrentUser();
            LoanApplication loanApplication = GetLoanApplicationObject();
            loanApplication.Id = loanApplicationId;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
               .Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanConsentRepository.GetUserConsentsByLoanApplicationIdAsync(loanApplicationId, currentUser));
        }

        /// <summary>
        /// Test method to verify counts of consents return when consents and loan application exist.
        /// </summary>
        [Fact]
        public async Task GetUserConsentsByLoanApplicationIdAsync_LoanApplicationAndEntityConsentsExist_ReturnsLoanConsentList()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            User currentUser = GetCurrentUser();
            Guid entityId = Guid.NewGuid();
            User shareHolder = new User
            {
                Id = Guid.NewGuid(),
                Name = "Lorem",
                Email = "lorem@epsum.com",
                Phone = "7878787878",
                SSN = "987654321"
            };
            LoanApplication loanApplication = GetLoanApplicationObject();
            loanApplication.Id = loanApplicationId;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = loanApplicationId,
                    UserId = currentUser.Id,
                    User = currentUser,
                    IsConsentGiven = false
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = loanApplicationId,
                    UserId = shareHolder.Id,
                    User = shareHolder,
                    IsConsentGiven = false
                }
            };
            EntityLoanApplicationMapping entityLoanApplicationMapping = new EntityLoanApplicationMapping()
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                LoanApplicationId = loanApplicationId,
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityId,
                    RelativeEntityId = currentUser.Id,
                    RelationshipId = Guid.NewGuid(),
                    SharePercentage = 20
                },
                new EntityRelationshipMapping()
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityId,
                    RelativeEntityId = shareHolder.Id,
                    RelationshipId = Guid.NewGuid(),
                    SharePercentage = 40
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
               .Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
               .Returns(Task.FromResult(entityLoanApplicationMapping));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
               .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act
            List<EntityLoanApplicationConsentAC> actualList = await _loanConsentRepository.GetUserConsentsByLoanApplicationIdAsync(loanApplicationId, currentUser);

            //Assert
            Assert.Equal(entityLoanApplicationConsents.Count, actualList.Count);
        }

        /// <summary>
        /// Test method to verify that it throws error when try to save consent of a user different from current user.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_ConsentOfDifferentUser_ThrowsError()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser));
        }

        /// <summary>
        /// Test method to verify that it throws error when try to save a consent of a user which is not in database.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_ConsentNotExist_ThrowsError()
        {
            //Arrange
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>();
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            consentAC.IsConsentGiven = true;

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
               .Returns(existingConsents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser));
        }

        /// <summary>
        /// Test method to verify that save operation is being performed if consent exists for given id 
        /// in argument and consent is being saved of current user only.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_SaveConsentOfCurrentUserAndConsentExists_SaveConsentOfCurrentUser()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = false,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };

            List<CreditReport> creditReportList = GetCreditReportList();
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();

            List<Entity> entityList = GetEntityList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
               .Returns(existingConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that methods for section name and status update are not being called if all the consents are not given.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_NotAllConsentsAreGiven_NoUpdateOfSectionNameAndStatus()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = false,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();

            List<Entity> entityList = GetEntityList();
            List<CreditReport> creditReportList = GetCreditReportList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
               .Returns(existingConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>()), Times.Never);
        }

        /// <summary>
        /// Test method to verify that methods for section name and status update are being called if all the consents are given.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_AllConsentsAreGiven_CallUpdateSectionNameAndStatusMethod()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = false,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = true
                }
            };

            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();

            List<Entity> entityList = GetEntityList();
            List<CreditReport> creditReportList = GetCreditReportList();
            List<EntityLoanApplicationMapping> entityLoanApplicationMappingList = GetEntityLoanApplicationMappingList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
               .Returns(existingConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object)
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Exactly(2));
        }

        /// <summary>
        /// Test method to verify that if no any consent is pending then don't send an email to shareholders.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ResendEmailsToShareHoldersAsync_LoanApplicationIdNullNoPendingConsent_DontSendEmail()
        {
            //Arrange
            Guid? loanApplicationId = null;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
              .Returns(existingConsents.AsQueryable().BuildMock().Object);

            //Act
            await _loanConsentRepository.SendReminderEmailToShareHoldersAsync(loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Never);
            _simpleEmailServiceUtilityMock.Verify(x => x.SendEmailToShareHoldersAsync(It.IsAny<List<EmailLoanDetailsAC>>()), Times.Never);
        }

        /// <summary>
        /// Test method to verify that if no any consent is pending then don't send an email to shareholders.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_LoanApplicationIdExistsNoPendingConsent_DontSendEmail()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            existingConsents.First().LoanApplication.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false,
                    LoanApplication = existingConsents.First().LoanApplication
                }
            };
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();

            List<Entity> entityList = GetEntityList();
            List<EntityLoanApplicationConsent> entityConsents = new List<EntityLoanApplicationConsent>();
            List<CreditReport> creditReportList = GetCreditReportList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
               .Returns(existingConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object)
               .Returns(entityConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Never);
            _simpleEmailServiceUtilityMock.Verify(x => x.SendEmailToShareHoldersAsync(It.IsAny<List<EmailLoanDetailsAC>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that If no any loan initiator has given consent then don't send
        /// en email to shareholders.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ResendEmailsToShareHoldersAsync_LoanApplicationIdNullNoLoanInitiatorGivenConsent_DontSendEmail()
        {
            //Arrange
            Guid? loanApplicationId = null;
            LoanApplication loanApplication = GetLoanApplicationObject();
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    UserId = loanApplication.UserId,
                    LoanApplicationId = loanApplication.Id,
                    IsConsentGiven = false,
                    LoanApplication = loanApplication
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
              .Returns(existingConsents.AsQueryable().BuildMock().Object);

            //Act
            await _loanConsentRepository.SendReminderEmailToShareHoldersAsync(loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Never);
            _simpleEmailServiceUtilityMock.Verify(x => x.SendEmailToShareHoldersAsync(It.IsAny<List<EmailLoanDetailsAC>>()), Times.Never);
        }

        /// <summary>
        /// Test method to verify that If no entity and loan application mapping exists for 
        /// any loan application id then don't send an email.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ResendEmailsToShareHoldersAsync_LoanApplicationIdNullNoLoanEntityMappingExists_DontSendEmail()
        {
            //Arrange
            Guid? loanApplicationId = null;
            LoanApplication loanApplication = GetLoanApplicationObject();
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    LoanApplicationId = loanApplication.Id,
                    IsConsentGiven = false,
                    LoanApplication = loanApplication
                }
            };
            List<EntityLoanApplicationMapping> entityLoanMappings = new List<EntityLoanApplicationMapping>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
              .Returns(existingConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
              .Returns(entityLoanMappings.AsQueryable().BuildMock().Object);

            //Act
            await _loanConsentRepository.SendReminderEmailToShareHoldersAsync(loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Never);
            _simpleEmailServiceUtilityMock.Verify(x => x.SendEmailToShareHoldersAsync(It.IsAny<List<EmailLoanDetailsAC>>()), Times.Never);
        }

        /// <summary>
        /// Test method to verify that If no entity and loan application mapping exists for 
        /// given loan application id then don't send an email.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_LoanApplicationIdExistsNoEntityMappingExists_DontSendEmail()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            existingConsents.First().LoanApplication.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false,
                    LoanApplication = existingConsents.First().LoanApplication
                }
            };
            List<CreditReport> creditReportList = GetCreditReportList();
            List<EntityLoanApplicationMapping> entityLoanMappings = new List<EntityLoanApplicationMapping>();
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();
            List<Entity> entityList = GetEntityList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
               .Returns(existingConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
              .Returns(entityLoanMappings.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Never);
            _simpleEmailServiceUtilityMock.Verify(x => x.SendEmailToShareHoldersAsync(It.IsAny<List<EmailLoanDetailsAC>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that If no any entity relationship exists for given 
        /// entity ids and users whose consents are pending then don't send an email.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ResendEmailsToShareHoldersAsync_LoanApplicationIdNullNoEntityRelationMappingExists_DontSendEmail()
        {
            //Arrange
            Guid? loanApplicationId = null;
            LoanApplication loanApplication = GetLoanApplicationObject();
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    LoanApplicationId = loanApplication.Id,
                    IsConsentGiven = false,
                    LoanApplication = loanApplication
                }
            };
            List<EntityLoanApplicationMapping> entityLoanMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = loanApplication.Id,
                    LoanApplication = loanApplication,
                    Entity = new Entity()
                    {
                        Company = new Company()
                        {
                            Name = "John Doe Pvt Ltd"
                        }
                    }
                }
            };
            List<EntityRelationshipMapping> entityRelationsMappings = new List<EntityRelationshipMapping>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
              .Returns(existingConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
              .Returns(entityLoanMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
              .Returns(entityRelationsMappings.AsQueryable().BuildMock().Object);

            //Act
            await _loanConsentRepository.SendReminderEmailToShareHoldersAsync(loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Once);
            _simpleEmailServiceUtilityMock.Verify(x => x.SendEmailToShareHoldersAsync(It.IsAny<List<EmailLoanDetailsAC>>()), Times.Never);
        }

        /// <summary>
        /// Test method to verify that If no any entity relationship exists for given 
        /// entity id and users whose consents are pending then don't send an email.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_LoanApplicationIdExistsNoEntityRelationshipMappingExist_DontSendEmail()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            existingConsents.First().LoanApplication.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false,
                    LoanApplication = existingConsents.First().LoanApplication
                }
            };
            List<CreditReport> creditReportList = GetCreditReportList();
            List<EntityLoanApplicationMapping> entityLoanMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplication.Id,
                    LoanApplication = existingConsents.First().LoanApplication,
                    Entity = new Entity()
                    {
                        Company = new Company()
                        {
                            Name = "John Doe Pvt Ltd"
                        }
                    }
                }
            };

            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();

            List<Entity> entityList = GetEntityList();
            List<EntityRelationshipMapping> entityRelationsMappings = new List<EntityRelationshipMapping>();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
               .Returns(existingConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
              .Returns(entityLoanMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
              .Returns(entityRelationsMappings.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Once);
            _simpleEmailServiceUtilityMock.Verify(x => x.SendEmailToShareHoldersAsync(It.IsAny<List<EmailLoanDetailsAC>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that mail is send to all the shareholders of the loan applications
        /// whose consents are given by loan initiators.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ResendEmailsToShareHoldersAsync_LoanApplicationIdNullLoanInitiatorGivenConsent_SendEmailToShareHolders()
        {
            //Arrange
            Guid? loanApplicationId = null;
            LoanApplication loanApplication = GetLoanApplicationObject();
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    LoanApplicationId = loanApplication.Id,
                    IsConsentGiven = false,
                    LoanApplication = loanApplication
                }
            };
            List<EntityLoanApplicationMapping> entityLoanMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = loanApplication.Id,
                    LoanApplication = loanApplication,
                    Entity = new Entity()
                    {
                        Company = new Company()
                        {
                            Name = "John Doe Pvt Ltd"
                        }
                    }
                }
            };
            List<EntityRelationshipMapping> entityRelationsMappings = new List<EntityRelationshipMapping>
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityLoanMappings.First().EntityId,
                    RelativeEntityId = existingConsents.First().UserId,
                    RelativeEntity = new Entity
                    {
                        User = new User
                        {
                            Email = "john@doe.com",
                            Name = "John Doe"
                        }
                    },
                    Relationship = new Relationship
                    {
                        Relation = "Shareholder"
                    }
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
              .Returns(existingConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
              .Returns(entityLoanMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
              .Returns(entityRelationsMappings.AsQueryable().BuildMock().Object);

            //Act
            await _loanConsentRepository.SendReminderEmailToShareHoldersAsync(loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Once);
            _simpleEmailServiceUtilityMock.Verify(x => x.SendEmailToShareHoldersAsync(It.IsAny<List<EmailLoanDetailsAC>>()), Times.Once);
        }

        /// <summary>
        /// Test method to verify that mail is send to all the shareholders once loan initiator gives the consent.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_LoanApplicationIdExistsLoanInitiatorGivenConsent_SendEmailToShareHolders()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            existingConsents.First().LoanApplication.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false,
                    LoanApplication = existingConsents.First().LoanApplication
                }
            };
            List<CreditReport> creditReportList = GetCreditReportList();
            List<EntityLoanApplicationMapping> entityLoanMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplication.Id,
                    LoanApplication = existingConsents.First().LoanApplication,
                    Entity = new Entity()
                    {
                        Company = new Company()
                        {
                            Name = "John Doe Pvt Ltd"
                        }
                    }
                }
            };
            List<EntityRelationshipMapping> entityRelationsMappings = new List<EntityRelationshipMapping>
            {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = entityLoanMappings.First().EntityId,
                    RelativeEntityId = entityLoanApplicationConsents.Last().UserId,
                    RelativeEntity = new Entity
                    {
                        User = new User
                        {
                            Email = "john@doe.com",
                            Name = "John Doe"
                        }
                    },
                    Relationship = new Relationship
                    {
                        Relation = "Shareholder"
                    }
                }
            };
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();

            List<Entity> entityList = GetEntityList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
               .Returns(existingConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object)
               .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
              .Returns(entityLoanMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
              .Returns(entityRelationsMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityRelationshipMapping>(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()), Times.Once);
            _simpleEmailServiceUtilityMock.Verify(x => x.SendEmailToShareHoldersAsync(It.IsAny<List<EmailLoanDetailsAC>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that if user credit report already exist for given entity then don't add new Credit Report.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_CreditAlreadyExists_VerifyAddCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();

            List<Entity> entityList = GetEntityList();
            List<CreditReport> creditReportList = GetCreditReportList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditReport>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that user with first name, last name and middle name credit report save successfully from experian.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_UserWithFirstNameLastNameMiddleName_VerifyAddAndExperianMethodCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            List<CreditReport> creditReportList = new List<CreditReport>();
            List<Entity> entityList = GetEntityList();
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = @"Json Response of automotive credit profile api"
            };

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _experianUtilityMock.Verify(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that user with first name, last name and middle name credit report save successfully from equifax.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_UserWithFirstNameLastNameMiddleName_VerifyAddAndEquifaxMethodCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            List<CreditReport> creditReportList = new List<CreditReport>();
            List<Entity> entityList = GetEntityList();
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = @"Json Response of consumer credit profile api equifax"
            };

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _equifaxUtilityMock.Setup(x => x.FetchUserCreditScoreEquifaxAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Equifax");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Equifax");

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _equifaxUtilityMock.Verify(x => x.FetchUserCreditScoreEquifaxAsync(It.IsAny<UserInfoAC>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that user with first name, last name and middle name credit report save successfully from transunion.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_UserWithFirstNameLastNameMiddleName_VerifyAddAndTransunionMethodCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            List<CreditReport> creditReportList = new List<CreditReport>();
            List<Entity> entityList = GetEntityList();
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = @"Json Response of consumer credit profile api transunion"
            };

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _transunionUtilityMock.Setup(x => x.FetchConsumerCreditReportAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Transunion");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Transunion");

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _transunionUtilityMock.Verify(x => x.FetchConsumerCreditReportAsync(It.IsAny<UserInfoAC>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that user credit report already exist but is older than 1 year than fetch credit report and is save successfully.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_UserCreditReportExistsOlderThanOneYear_VerifyAddAndFetchCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            List<CreditReport> creditReportList = new List<CreditReport>() {
                new CreditReport
                {
                    Id = Guid.NewGuid(),
                    CommercialScore= 5,
                    CreatedOn = new DateTime(2016, 7, 15, 3, 15, 0),
                    FsrScore = 5,
                    EntityId = Guid.NewGuid(),
                    HasPendingJudgment = false,
                    HasPendingLien = false,
                    IsBankrupted = false,
                    LoanApplicationId = Guid.NewGuid(),
                    Response =  @"Json Response of consumer credit profile api equifax",
                    ResponseSource = "Equifax"
                }
            };
            List<Entity> entityList = GetEntityList();
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = @"Json Response of automotive credit profile api"
            };

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _equifaxUtilityMock.Setup(x => x.FetchUserCreditScoreEquifaxAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Equifax");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Equifax");

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _equifaxUtilityMock.Verify(x => x.FetchUserCreditScoreEquifaxAsync(It.IsAny<UserInfoAC>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that user credit report already exist but is not older than 1 year than don't fetch credit report and previous credit report is save successfully.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_UserCreditReportExistsNotOlderThanOneYear_VerifyAddAndEquifaxCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            List<CreditReport> creditReportList = new List<CreditReport>() {
                new CreditReport
                {
                    Id = Guid.NewGuid(),
                    CommercialScore= 5,
                    CreatedOn = new DateTime(2020, 7, 10, 3, 15, 0),
                    FsrScore = 5,
                    EntityId = Guid.NewGuid(),
                    HasPendingJudgment = false,
                    HasPendingLien = false,
                    IsBankrupted = false,
                    LoanApplicationId = Guid.NewGuid(),
                    Response =  @"Json Response of consumer credit profile api equifax",
                    ResponseSource = "Equifax"
                }
            };
            List<Entity> entityList = GetEntityList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Equifax");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Equifax");

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _equifaxUtilityMock.Verify(x => x.FetchUserCreditScoreEquifaxAsync(It.IsAny<UserInfoAC>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that user with first name and last name credit report save successfully from experian.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_UserWithFirstNameLastName_VerifyAddCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            List<CreditReport> creditReportList = new List<CreditReport>();
            List<Entity> entityList = GetEntityList();
            entityList.Single().User.Name = "John Wick";
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = @"Json Response of automotive credit profile api"
            };

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }


        /// <summary>
        /// Test method to verify that user with only first name credit report save successfully from experian.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_UserWithFirstName_VerifyAddCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            List<CreditReport> creditReportList = new List<CreditReport>();
            List<Entity> entityList = GetEntityList();
            entityList.Single().User.Name = "John";
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = @"Json Response of automotive credit profile api"
            };

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that user credit report not save if credit report not found.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_CreditReportNotFound_VerifyAddCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = false
                }
            };
            List<CreditReport> creditReportList = new List<CreditReport>();
            List<Entity> entityList = GetEntityList();
            entityList.Single().User.Name = "John";
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that method don't add credit report of company and call company experian api if company is sole trader.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_SoleTraderCompany_VerifyAddCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = true
                }
            };

            List<CreditReport> creditReportList = new List<CreditReport>();

            List<EntityLoanApplicationMapping> entityLoanApplicationMappingList = GetEntityLoanApplicationMappingList();

            List<Company> companyList = new List<Company>()
            {
                new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "Google",
                    CIN = "123456781",
                    CompanyTypeId = Guid.NewGuid(),
                    CompanyType = new CompanyType
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.SoleTraders,
                        Order = 1
                    }
                }
            };
            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();

            List<Entity> entityList = GetEntityList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(GetCreditReportList().AsQueryable().BuildMock().Object)
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companyList.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _experianUtilityMock.Verify(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Exactly(2));
        }

        /// <summary>
        /// Test method to verify that method add credit report of company successfully if company type is registered entity from experian.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_RegisteredEntityCompany_VerifyCreditReportAddCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = true
                }
            };

            List<CreditReport> creditReportList = new List<CreditReport>();

            List<EntityLoanApplicationMapping> entityLoanApplicationMappingList = GetEntityLoanApplicationMappingList();

            List<Company> companyList = new List<Company>()
            {
                new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "Google",
                    CIN = "123456781",
                    CompanyTypeId = Guid.NewGuid(),
                    CompanyType = new CompanyType
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.RegisteredEntity,
                        Order = 1
                    }
                }
            };

            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();
            List<Entity> entityList = GetEntityList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(GetCreditReportList().AsQueryable().BuildMock().Object)
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companyList.AsQueryable().BuildMock().Object);
            _experianUtilityMock.Setup(x => x.FetchUserCreditScoreExperianAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Experian");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Exactly(2));
        }

        /// <summary>
        /// Test method to verify that method add credit report of company and user successfully if commercial API selection is experian and consumer API selection is transunion.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_ConsumerAPITransunionCommercialAPIExperian_VerifyCreditReportAddCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = true
                }
            };

            List<CreditReport> creditReportList = new List<CreditReport>();

            List<EntityLoanApplicationMapping> entityLoanApplicationMappingList = GetEntityLoanApplicationMappingList();

            List<Company> companyList = new List<Company>()
            {
                new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "Google",
                    CIN = "123456781",
                    CompanyTypeId = Guid.NewGuid(),
                    CompanyType = new CompanyType
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.RegisteredEntity,
                        Order = 1
                    }
                }
            };

            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = @"Json from transunion",
                Bankruptcy = true,
                Score = 542
            };

            PremierProfilesResponseAC premierProfilesResponse = GetPremierProfilesResponse();
            List<Entity> entityList = GetEntityList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(GetCreditReportList().AsQueryable().BuildMock().Object)
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(companyList.AsQueryable().BuildMock().Object);
            _transunionUtilityMock.Setup(x => x.FetchConsumerCreditReportAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _experianUtilityMock.Setup(x => x.FetchCompanyCreditScoreExperianAsync(It.IsAny<string>()))
                .ReturnsAsync(premierProfilesResponse);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Transunion");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Experian");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Exactly(2));
        }

        /// <summary>
        /// Test method to verify that not credit report of company is fetch if bank preferencce for bureau is equifax.
        /// </summary>
        [Fact]
        public async Task SaveLoanConsentOfUserAsync_RegisteredEntityCompanyCreditReportNotFetched_VerifyCreditReportAddCall()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            EntityLoanApplicationConsentAC consentAC = GetEntityLoanApplicationConsentACObject();
            consentAC.UserId = currentUser.Id;
            List<EntityLoanApplicationConsent> existingConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = consentAC.Id,
                    UserId = consentAC.UserId,
                    LoanApplicationId = consentAC.LoanApplicationId,
                    IsConsentGiven = true,
                    LoanApplication = GetLoanApplicationObject()
                }
            };
            List<EntityLoanApplicationConsent> entityLoanApplicationConsents = new List<EntityLoanApplicationConsent>
            {
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = currentUser.Id,
                    IsConsentGiven = true
                },
                new EntityLoanApplicationConsent
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = existingConsents.First().LoanApplicationId,
                    UserId = Guid.NewGuid(),
                    IsConsentGiven = true
                }
            };

            List<CreditReport> creditReportList = new List<CreditReport>();

            UserInfoAC userInfo = new UserInfoAC
            {
                ConsumerCreditReportResponse = null
            };
            List<Entity> entityList = GetEntityList();

            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(existingConsents.AsQueryable().BuildMock().Object)
                .Returns(entityLoanApplicationConsents.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(GetCreditReportList().AsQueryable().BuildMock().Object)
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _equifaxUtilityMock.Setup(x => x.FetchUserCreditScoreEquifaxAsync(It.IsAny<UserInfoAC>()))
                .ReturnsAsync(userInfo);
            _configurationMock.Setup(x => x.GetSection("BankPreference:ConsumerAPIBureau").Value).Returns("Equifax");
            _configurationMock.Setup(x => x.GetSection("BankPreference:CommercialAPIBureau").Value).Returns("Equifax");
            _dataRepositoryMock.Setup(s => s.Fetch<Entity>(It.IsAny<Expression<Func<Entity, bool>>>()))
                .Returns(entityList.AsQueryable().BuildMock().Object);

            //Act
            await _loanConsentRepository.SaveLoanConsentOfUserAsync(consentAC, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityLoanApplicationConsent>(It.IsAny<EntityLoanApplicationConsent>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Fetch<EntityLoanApplicationMapping>(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Fetch<Company>(It.IsAny<Expression<Func<Company, bool>>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _globalRepositoryMock.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _globalRepositoryMock.Verify(x => x.UpdateStatusOfLoanApplicationAsync(It.IsAny<Guid>(), It.IsAny<LoanApplicationStatusType>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<CreditReport>(It.IsAny<CreditReport>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Exactly(2));
        }

        /// <summary>
        /// Test method to verify whether exception is thrown for non approved loan on attempt of add bank details
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateBankDetailsAsync_BankDetailsNotExist_VerifyExceptionThrownForNonApprovedLoan()
        {
            // Arrange
            var entityBankDetailsAC = new LoanEntityBankDetailsAC
            {
                LoanApplicationId = Guid.Parse("cf8c2bf8-567e-4bd6-94e6-b05328126b1f")
            };

            var loanApplication = new LoanApplication
            {
                Id = entityBankDetailsAC.LoanApplicationId,
                LoanApplicationStatus = new LoanApplicationStatus
                {
                    Status = LoanApplicationStatusType.Draft
                }
            };

            var loanApplicationList = new List<LoanApplication>();
            loanApplicationList.Add(loanApplication);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
               .Returns(loanApplicationList.AsQueryable().BuildMock().Object);


            // Act
            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _loanConsentRepository.AddOrUpdateBankDetailsAsync(entityBankDetailsAC, new User()));

        }

        /// <summary>
        /// Test method to verify if exception thrown on invalid loan id passed
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateBankDetailsAsync_LoanIdNotExist_VerifyExceptionThrownForLoanNotFound()
        {
            // Arrange
            var entityBankDetailsAC = new LoanEntityBankDetailsAC
            {
                LoanApplicationId = Guid.Parse("cf8c2bf8-567e-4bd6-94e6-b05328126b1f")
            };

            var loanApplicationList = new List<LoanApplication>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
               .Returns(loanApplicationList.AsQueryable().BuildMock().Object);

            // Act
            // Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanConsentRepository.AddOrUpdateBankDetailsAsync(entityBankDetailsAC, new User()));

        }
        #endregion
    }
}
