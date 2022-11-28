using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass.LoanApplicationInfo;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Repository.Repository.LoanApplicationInfo;
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

namespace LendingPlatform.Repository.Test.LoanAppicationInfo
{
    [Collection("Register Dependency")]
    public class LoanNeedsRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly ILoanNeedsRepository _loanNeedsRepository;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public LoanNeedsRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _configurationMock = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _loanNeedsRepository = _scope.ServiceProvider.GetService<ILoanNeedsRepository>();
            _mapper = bootstrap.ServiceProvider.GetService<IMapper>();
            _dataRepositoryMock.Reset();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get an object of LoanApplicationAC class.
        /// </summary>
        /// <returns>Returns object of LoanApplicationAC class</returns>
        private LoanApplicationAC GetLoanApplicationACObject()
        {
            return new LoanApplicationAC()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                LoanPurposeId = Guid.NewGuid(),
                LoanPeriod = 24,
                LoanApplicationStatusId = Guid.NewGuid(),
                LoanAmount = 100000,
                LoanApplicationNumber = "GHI5678KK",
                LoanApplicationStatusAC = GetLoanApplicationStatusACObject()
            };
        }

        /// <summary>
        /// Get an list of LoanApplication class.
        /// </summary>
        /// <returns>Returns list of LoanApplication class</returns>
        private List<LoanApplication> GetLoanApplicationObjects()
        {
            return new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    LoanPurposeId = Guid.NewGuid(),
                    LoanPeriod = 24,
                    LoanApplicationStatusId = Guid.NewGuid(),
                    LoanAmount = 100000,
                    LoanApplicationNumber = "GHI5678KK",
                    LoanApplicationStatus = GetLoanApplicationStatusObject()
                }
            };
        }

        /// <summary>
        /// Get an object of LoanApplicationStatus class.
        /// </summary>
        /// <returns>Returns object of LoanApplicationStatus class</returns>
        private LoanApplicationStatus GetLoanApplicationStatusObject()
        {
            return new LoanApplicationStatus()
            {
                Id = Guid.NewGuid(),
                SectionName = "LoanNeeds",
                Status = LoanApplicationStatusType.Draft
            };
        }

        /// <summary>
        /// Get an object of LoanApplicationStatus class having section name Loan Consent
        /// </summary>
        /// <returns>Returns Object of LoanApplicationStatus class</returns>
        private LoanApplicationStatus GetLoanApplicationStatus_LoanConsentSectionNameObject()
        {
            return new LoanApplicationStatus()
            {
                Id = Guid.NewGuid(),
                SectionName = "Loan Consent",
                Status = LoanApplicationStatusType.Draft
            };
        }

        /// <summary>
        /// Get an object of LoanApplicationStatusAC class.
        /// </summary>
        /// <returns>Returns object of LoanApplicationStatusAC class</returns>
        private LoanApplicationStatusAC GetLoanApplicationStatusACObject()
        {
            return new LoanApplicationStatusAC()
            {
                Id = Guid.NewGuid(),
                SectionName = "LoanNeeds",
                Status = LoanApplicationStatusType.Draft
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
                LoanPurpose = new LoanPurpose() { Id = Guid.NewGuid(), Name = "Asset Purchase",LoanTypeId=Guid.NewGuid() },
                LoanApplicationStatus = new LoanApplicationStatus() { Id = Guid.NewGuid(), SectionName = "Loan Product", Status = LoanApplicationStatusType.Approved },
                EntityFinances = new List<EntityFinance>()
                { new EntityFinance(){
                    Id = Guid.NewGuid(),
                    FinancialStatement = new FinancialStatement() { Id = Guid.NewGuid(), Name = "Income Statement" },
                    EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMapping>() {new EntityFinanceYearlyMapping()
                    {Period="Jan-Dec2048",Id=Guid.NewGuid(),EntityFinanceId=Guid.NewGuid() } }
                }
                },
                LoanPeriod = 24,
                LoanApplicationNumber = "GHI5678KK"
            };
        }

        /// <summary>
        /// Get list of loan purposes.
        /// </summary>
        /// <returns>Returns list of LoanPurpose</returns>
        private List<LoanPurpose> GetLoanPurposeList()
        {
            return new List<LoanPurpose>()
            {
                new LoanPurpose() { Id = Guid.NewGuid(), Name = "Cash Flow", Order = 1 },
                new LoanPurpose() { Id = Guid.NewGuid(), Name = "Asset Purchase", Order = 2 },
                new LoanPurpose() { Id = Guid.NewGuid(), Name = "Property Purchase", Order = 3 },
                new LoanPurpose() { Id = Guid.NewGuid(), Name = "Other Long Term Need", Order = 4 },
            };
        }

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
        /// Method to get the list of entityLoanApplicationMapping.
        /// </summary>
        private List<EntityLoanApplicationMapping> GetEntityLoanApplicationMappingList()
        {
            return new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping(){Entity=new Entity(){ Id=Guid.NewGuid(),Company=new Company(){Id=Guid.NewGuid(),Name="ABC"} },
                LoanApplication=new LoanApplication(){  CreatedOn=new DateTime(), Id=Guid.NewGuid(),
                    LoanApplicationNumber="LP12344568784",LoanAmount=100,
                    LoanApplicationStatus=new LoanApplicationStatus(){Id=Guid.NewGuid(),Status=LoanApplicationStatusType.Draft},
                    LoanApplicationStatusId=Guid.NewGuid(),UserId=Guid.NewGuid(),
                    LoanPurpose=new LoanPurpose(){Id=Guid.NewGuid(),Name="Asset Purchase",Order=1},
                    User=new Entity()
                    { Id=Guid.NewGuid(),Company=new Company(){Id=Guid.NewGuid(),Name="ABC"} }
                } },
                new EntityLoanApplicationMapping(){Entity=new Entity() { Id=Guid.NewGuid(),Company=new Company(){Id=Guid.NewGuid(),Name="Anc"}},
                LoanApplication=new LoanApplication(){ CreatedOn=new DateTime(), Id=Guid.NewGuid(),
                    LoanApplicationNumber="LP-1234565",
                    LoanApplicationStatus=new LoanApplicationStatus(){Id=Guid.NewGuid(),Status=LoanApplicationStatusType.Draft},
                    LoanApplicationStatusId=Guid.NewGuid(),UserId=Guid.NewGuid(),
                    LoanPurpose=new LoanPurpose(){Id=Guid.NewGuid(),Name="Asset Purchase",Order=1},
                    User=new Entity()
                    { Id=Guid.NewGuid(),Company=new Company(){Id=Guid.NewGuid(),Name="Anc"} }
                } },
                new EntityLoanApplicationMapping(){Entity=new Entity(){Id=Guid.NewGuid(),Company=new Company(){Id=Guid.NewGuid(),Name="Anc"}},
                LoanApplication=new LoanApplication(){ CreatedOn=new DateTime(), Id=Guid.NewGuid(),
                    LoanApplicationNumber="LP-1234565",
                    LoanApplicationStatus=new LoanApplicationStatus(){Id=Guid.NewGuid(),Status=LoanApplicationStatusType.Draft},
                    LoanApplicationStatusId=Guid.NewGuid(),UserId=Guid.NewGuid(),
                    LoanPurpose=new LoanPurpose(){Id=Guid.NewGuid(),Name="Asset Purchase",Order=1},
                    User=new Entity()
                    { Id=Guid.NewGuid(),Company=new Company(){Id=Guid.NewGuid(),Name="Anc"} }
                } }
            };
        }

        /// <summary>
        /// Method to get the list of loan applications.
        /// </summary>
        /// <returns></returns>
        private List<LoanApplication> GetLoanApplicationList()
        {
            return new List<LoanApplication>()
            {
                new LoanApplication(){ CreatedOn=new DateTime(), Id=Guid.NewGuid(),
                    LoanApplicationNumber="LP-1234565",
                    LoanApplicationStatus=new LoanApplicationStatus(){Id=Guid.NewGuid(),Status=LoanApplicationStatusType.Draft},
                    LoanApplicationStatusId=Guid.NewGuid(),UserId=Guid.NewGuid(),
                    LoanPurpose=new LoanPurpose(){Id=Guid.NewGuid(),Name="Asset Purchase",Order=1},
                    User=new Entity()
                    { Id=Guid.NewGuid(),Company=new Company(){Id=Guid.NewGuid(),Name="Anc"} }
                },
                new LoanApplication(){ CreatedOn=new DateTime(), Id=Guid.NewGuid(),
                    LoanApplicationNumber="LP-1234565",
                    LoanApplicationStatus=new LoanApplicationStatus(){Id=Guid.NewGuid(),Status=LoanApplicationStatusType.Draft},
                    LoanApplicationStatusId=Guid.NewGuid(),UserId=Guid.NewGuid(),
                    LoanPurpose=new LoanPurpose(){Id=Guid.NewGuid(),Name="Asset Purchase",Order=1},
                    User=new Entity()
                    { Id=Guid.NewGuid(),Company=new Company(){Id=Guid.NewGuid(),Name="Anc"} }
                }
            };
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Test method to verify count of loan applications of user when loan applications exist.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationsWithStatusByEntityIdAsync_LoanApplicationExists_VerifyCountOfLoanApplications()
        {
            //Arrange
            var currentUser = GetCurrentUser();
            var entityId = Guid.NewGuid();
            var loanApplicationId1 = Guid.NewGuid();
            var loanApplicationId2 = Guid.NewGuid();
            var loanApplicationStatusAC1 = GetLoanApplicationStatusACObject();
            var loanApplicationStatusAC2 = GetLoanApplicationStatusACObject();
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
                        LoanApplicationStatus = GetLoanApplicationStatusObject()
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
                        LoanApplicationStatus = GetLoanApplicationStatusObject()
                    }
                }
            };
            var loanApplicationListAC = new List<LoanApplicationListAC>()
            {
                new LoanApplicationListAC
                {
                    LoanApplicationId = loanApplicationId1,
                    LoanApplicationNumber = "LP2805202006352244",
                    LoanApplicationStatusAC = loanApplicationStatusAC1
                },
                new LoanApplicationListAC
                {
                    LoanApplicationId = loanApplicationId2,
                    LoanApplicationNumber = "LP2805202006386688",
                    LoanApplicationStatusAC = loanApplicationStatusAC2
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(currentUser, It.IsAny<List<LoanApplicationListAC>>(), true)).ReturnsAsync(loanApplicationListAC);

            //Act
            List<LoanApplicationListAC> actualList = await _loanNeedsRepository.GetLoanApplicationsWithStatusByEntityIdAsync(entityId, currentUser);

            //Assert
            Assert.Equal(entityLoanApplicationMappings.ToList().Count, actualList.Count);
        }

        /// <summary>
        /// Test method to verify empty list of loan applications when loan applications does't exist.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationsWithStatusByEntityIdAsync_LoanAplicationNotExists_VerifyEmptyListOfLoanApplications()
        {
            //Arrange
            var entityId = Guid.NewGuid();
            var currentUser = GetCurrentUser();
            var entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>() { };
            var loanApplicationListAC = new List<LoanApplicationListAC>();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                .Returns(entityLoanApplicationMappings.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(currentUser, It.IsAny<List<LoanApplicationListAC>>(), true)).ReturnsAsync(loanApplicationListAC);

            //Act
            List<LoanApplicationListAC> actualList = await _loanNeedsRepository.GetLoanApplicationsWithStatusByEntityIdAsync(entityId, currentUser);

            //Assert
            Assert.Empty(actualList);
        }

        /// <summary>
        /// Test method to verify count of LoanPurpose list returns when data exists in table.
        /// </summary>
        [Fact]
        public async Task GetLoanPurposeListAsync_LoanPurposeExists_ReturnsListOfLoanPurpose()
        {
            //Arrange
            var expectedPurposeList = GetLoanPurposeList();

            _dataRepositoryMock.Setup(x => x.GetAll<LoanPurpose>())
               .Returns(expectedPurposeList.AsQueryable().BuildMock().Object);

            //Act
            LoanPurposeListAC actual = await _loanNeedsRepository.GetLoanPurposeListAsync();

            //Assert
            Assert.Equal(expectedPurposeList.ToList().Count, actual.LoanPurposes.Count);
        }

        /// <summary>
        /// Test method to verify that it throws exception when data not exists in LoanPurpose table.
        /// </summary>
        [Fact]
        public async Task GetLoanPurposeListAsync_LoanPurposeNotExist_ThrowsError()
        {
            //Arrange
            var emptyPurposeList = new List<LoanPurpose>() { };

            _dataRepositoryMock.Setup(x => x.GetAll<LoanPurpose>())
               .Returns(emptyPurposeList.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanNeedsRepository.GetLoanPurposeListAsync());
        }

        /// <summary>
        /// Test method to verify that add and save operation is being performed if loan application 
        /// id is null in argument and section name is also assigned.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_NullLoanApplicationId_PerformsAddAndSaveOperation()
        {
            //Arrange
            var loanApplicationAC = GetLoanApplicationACObject();
            var currenUser = GetCurrentUser();

            _configurationMock.Setup(s => s.GetSection("Codes").GetSection("BankCode").Value).Returns("LP");
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act
            await _loanNeedsRepository.SaveLoanApplicationAsync(loanApplicationAC, currenUser, null);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LoanApplicationStatus>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LoanApplication>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that it throws an error if loan application id is null in argument 
        /// but section name is not assigned.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_SectionNameNotAssigned_ThrowsErrorNoAddOperation()
        {
            //Arrange
            var loanApplicationAC = GetLoanApplicationACObject();
            var currentUser = GetCurrentUser();
            loanApplicationAC.LoanApplicationStatusAC.SectionName = null;

            //Act

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _loanNeedsRepository.SaveLoanApplicationAsync(loanApplicationAC, currentUser, null));
        }

        /// <summary>
        /// Test method to verify that private method is being called and it returns valid loan application number
        /// before add and save operation is being performed 
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_GenerateLoanApplicationNumber_PrivateMethodReturnsValidNumber()
        {
            //Arrange
            var loanApplicationAC = GetLoanApplicationACObject();
            loanApplicationAC.LoanApplicationNumber = null;
            var currentUser = GetCurrentUser();

            _configurationMock.Setup(s => s.GetSection("Codes").GetSection("BankCode").Value).Returns("LP");
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);

            //Act
            await _loanNeedsRepository.SaveLoanApplicationAsync(loanApplicationAC, currentUser, null);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LoanApplicationStatus>()), Times.Once);
            Assert.Matches("^[A-Z]{2}[0-9]{1,16}$", loanApplicationAC.LoanApplicationNumber);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LoanApplication>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityLoanApplicationMapping>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Test method to verify that update and save operation is being performed if loan application 
        /// exists for given id in argument and section name is also assigned.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_LoanApplicationExist_PerformsUpdateAndSaveOperation()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            var loanApplications = GetLoanApplicationObjects();
            var loanApplicationAC = GetLoanApplicationACObject();
            var loanApplication = GetLoanApplicationObject();
            var currentUser = GetCurrentUser();
            loanApplication.Id = loanApplicationId;
            loanApplicationAC.Id = loanApplicationId;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            //Act
            await _loanNeedsRepository.SaveLoanApplicationAsync(loanApplicationAC, currentUser, loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<LoanApplication>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(1));
        }

        /// <summary>
        /// Test method to verify to remove loan product from LoanApplicationProductMapping when section name is loan consent and user updates the loan application data from loan needs
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveLoanApplicationAsync_LoanApplicationExist_VerifyRemoveLoanProductAndUpdateSectionNameOperation()
        {
            var loanApplicationId = Guid.NewGuid();
            var loanApplications = GetLoanApplicationObjects();
            foreach (var data in loanApplications)
            {
                data.LoanApplicationStatus = GetLoanApplicationStatus_LoanConsentSectionNameObject();
            } 
            var loanApplicationAC = GetLoanApplicationACObject();
            var loanApplication = GetLoanApplicationObject();
            var currentUser = GetCurrentUser();
            var loanApplicationProductMapping = new LoanApplicationProductMapping()
            {
                LoanApplicationId = loanApplicationId
            };
            loanApplication.Id = loanApplicationId;
            loanApplicationAC.Id = loanApplicationId;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplicationProductMapping, bool>>>())).Returns(Task.FromResult(loanApplicationProductMapping));
            _dataRepositoryMock.Setup(x => x.Remove(It.IsAny<Expression<Func<LoanApplicationProductMapping, bool>>>())).Verifiable();
            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            await _loanNeedsRepository.SaveLoanApplicationAsync(loanApplicationAC, currentUser, loanApplicationId);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<LoanApplication>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Remove(It.IsAny<LoanApplicationProductMapping>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            
        }

        /// <summary>
        /// Test method to verify that it throws an error if loan application doesn't exists for given id in argument.
        /// </summary>
        [Fact]
        public async Task SaveLoanApplicationAsync_LoanApplicationNotExists_ThrowsErrorNoUpdateOperation()
        {
            //Arrange
            var loanApplications = new List<LoanApplication>();
            var loanApplicationAC = GetLoanApplicationACObject();
            var currentUser = GetCurrentUser();
            var loanApplicationId = Guid.NewGuid();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanNeedsRepository.SaveLoanApplicationAsync(loanApplicationAC, currentUser, loanApplicationId));
        }

        /// <summary>
        /// Test method to verify that object is being returned for existing LoanApplicationId.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationByIdAsync_ApplicationIdExists_ReturnsObject()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            var loanApplication = GetLoanApplicationObject();
            
            var loanApplicationStatus = GetLoanApplicationStatusObject();
            var currentUser = GetCurrentUser();
            var loanApplicationAC = _mapper.Map<LoanApplication, LoanApplicationAC>(loanApplication);
            loanApplicationAC.LoanApplicationStatusAC = _mapper.Map<LoanApplicationStatus, LoanApplicationStatusAC>(loanApplicationStatus);

            var loanApplications = new List<LoanApplication>();
            loanApplications.Add(loanApplication);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
              .Returns(loanApplications.AsQueryable().BuildMock().Object);

            
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            //Act
            LoanApplicationAC actual = await _loanNeedsRepository.GetLoanApplicationByIdAsync(loanApplicationId, currentUser);

            //Assert
            
            Assert.Equal(loanApplicationAC.LoanApplicationNumber, actual.LoanApplicationNumber);
        }

        /// <summary>
        /// Test method to verify that it throws error for non-existing LoanApplicationId.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationByIdAsync_ApplicationIdNotExists_ThrowsError()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            var currentUser = GetCurrentUser();
            LoanApplication loanApplication = null;
            var loanApplications = new List<LoanApplication>();
            loanApplications.Add(loanApplication);

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
              .Returns(loanApplications.AsQueryable().BuildMock().Object);

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanNeedsRepository.GetLoanApplicationByIdAsync(loanApplicationId, currentUser));
        }

        #region Bank interface
        /// <summary>
        /// Test method to verify the PagedLoanApplicationAC object.
        /// </summary>
        /// <returns>PagedLoanApplicationAC Object.</returns>
        [Fact]
        public async Task GetAllLoanAsync_ReturnPagedLoanApplicationObject()
        {
            // Arrange            
            LoanApplicationFilterAC loanApplicationFilterAC = new LoanApplicationFilterAC()
            {
                Field = "MoneyNeeds",
                FilterCompanyName = "ABC",
                FilterDate = new DateTime(),
                FilterLoanPurpose = "Asset Purchase",
                FilterLoanStatus = LoanApplicationStatusType.Draft,
                FilterMoneyNeeds = 100,
                Order = "asc",
                Page = 1,
                PageCount = 10,
                SearchLoanNumber = "LP12344568784"
            };
            int loanCount = 1;

            var loanApplications = GetEntityLoanApplicationMappingList();

            _dataRepositoryMock.Setup(x => x.GetAll<EntityLoanApplicationMapping>()).Returns(loanApplications.AsQueryable().BuildMock().Object);

            // Act
            PagedLoanApplicationAC actual = await _loanNeedsRepository.GetAllLoanAsync(loanApplicationFilterAC);

            //Assert
            Assert.Equal(loanCount, actual.LoanCount);
        }

        /// <summary>
        /// Test method to verify non-existing loan application list.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationAsync_PagedLoanApplicationNotExists_ReturnZeroLoanAppliaction()
        {
            // Arrange            
            LoanApplicationFilterAC loanApplicationFilterAC = new LoanApplicationFilterAC()
            {
                Field = "MoneyNeeds",
                FilterCompanyName = "ABC",
                FilterDate = new DateTime(),
                FilterLoanPurpose = "Asset Purchase",
                FilterLoanStatus = LoanApplicationStatusType.Draft,
                FilterMoneyNeeds = 120,
                Order = "asc",
                Page = 1,
                PageCount = 10,
                SearchLoanNumber = "LP12344568784"
            };

            var loanApplications = GetEntityLoanApplicationMappingList();

            _dataRepositoryMock.Setup(x => x.GetAll<EntityLoanApplicationMapping>()).Returns(loanApplications.AsQueryable().BuildMock().Object);

            // Act
            PagedLoanApplicationAC actual = await _loanNeedsRepository.GetAllLoanAsync(loanApplicationFilterAC);

            //Assert
            Assert.Equal(0, actual.LoanCount);

        }

        /// <summary>
        /// Method to get the loan application.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationAsync_ApplicationIdExists_ReturnsLoanApplicationObject()
        {
            //Arrange
            var loanApplications = GetLoanApplicationList();

            var loanApplicationAC = _mapper.Map<LoanApplication, LoanApplicationAC>(loanApplications[0]);
            loanApplicationAC.LoanApplicationStatusAC = _mapper.Map<LoanApplicationStatus, LoanApplicationStatusAC>(loanApplications[0].LoanApplicationStatus);

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
              .Returns(loanApplications.AsQueryable().BuildMock().Object.Where(x=>x.Id == loanApplicationAC.Id));

            //Act
            LoanApplicationAC actual = await _loanNeedsRepository.GetLoanApplicationAsync(loanApplications[0].Id);

            //Assert
            Assert.Equal(loanApplicationAC.Id, actual.Id);
        }

        /// <summary>
        /// Test method to verify that it throws error for non-existing LoanApplicationId.
        /// </summary>
        [Fact]
        public async Task GetLoanApplicationAsync_ApplicationNotExists_ThrowsError()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            List<LoanApplication> loanApplications = GetLoanApplicationList();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
             .Returns(loanApplications.AsQueryable().BuildMock().Object.Where(x => x.Id == loanApplicationId));

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanNeedsRepository.GetLoanApplicationAsync(loanApplicationId));
        }
        #endregion

        #endregion
    }
}
