using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
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

namespace LendingPlatform.Repository.Test.Entity
{
    [Collection("Register Dependency")]
    public class EntityTaxReturnRepositoryTest : BaseTest
    {

        #region Private Variables
        private readonly Mock<IAmazonServicesUtility> _amazonServiceUtility;
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IConfiguration> _configuration;
        private readonly IEntityTaxReturnRepository _entityTaxReturnRepository;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly CurrentUserAC _currentUserAC, _currentBankUserAC;
        #endregion

        #region Constructor
        public EntityTaxReturnRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _amazonServiceUtility = bootstrap.ServiceProvider.GetService<Mock<IAmazonServicesUtility>>();
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _entityTaxReturnRepository = bootstrap.ServiceProvider.GetService<IEntityTaxReturnRepository>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _configuration = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _amazonServiceUtility.Reset();
            _dataRepositoryMock.Reset();
            _globalRepositoryMock.Reset();
            _currentUserAC = FetchLoggedInUserAC();
            _currentBankUserAC = FetchLoggedInBankUserAC();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of TaxFormCompanyStructureMappings
        /// </summary>
        /// <returns></returns>
        private List<TaxFormCompanyStructureMapping> GetTaxFormCompanyStructureMappings()
        {
            return new List<TaxFormCompanyStructureMapping>()
            {
                new TaxFormCompanyStructureMapping()
                {
                    TaxFormId = Guid.NewGuid(),
                    CompanyStructureId = Guid.NewGuid(),
                    IsSoleProprietors = false,
                    TaxForm = new TaxForm()
                    {
                        Id = Guid.NewGuid(),
                        Name = "1040"
                    }
                }
            };
        }

        /// <summary>
        /// Method to verify that it throws an error if user is unauthorized to access the loan application.
        /// </summary>
        [Fact]
        public async Task GetTaxReturnInfoAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            //Arrange
            var entityId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _entityTaxReturnRepository.GetTaxReturnInfoAsync(entityId, ResourceType.Company, _currentUserAC));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxReturnInfoAsync_EntityExists_EntityTaxFormsExists_NoEntityTaxYearlyMappingExists_VerifyThrowsInvalidDataExceptionError()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var companies = new List<Company>() {
                new Company
                {
                    Id = entityId,
                    CompanyStructure = new CompanyStructure()
                    {
                        Structure = StringConstant.Proprietorship
                    }
                }
            };
            var relativeEntities = new List<EntityRelationshipMapping>();
            var entityTaxForms = new List<EntityTaxForm>() {
                new EntityTaxForm()
                {
                    EntityId = commonGuid,
                    TaxFormId = commonGuid
                }
            };
            var taxFormCompanyStructureMapping = GetTaxFormCompanyStructureMappings();
            var entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>();

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("2");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>())).Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>())).Returns(relativeEntities.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<TaxFormCompanyStructureMapping, bool>>>())).Returns(taxFormCompanyStructureMapping.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxForms.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityTaxReturnRepository.GetTaxReturnInfoAsync(entityId, ResourceType.Company, _currentUserAC));
        }

        /// <summary>
        /// Check successfully able to get taxes
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxReturnInfoAsync_EntityExists_EntityTaxFormsExists_EntityTaxYearlyMappingExists_TaxesCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var companies = new List<Company>() {
                new Company
                {
                    Id = entityId,
                    CompanyStructure = new CompanyStructure()
                    {
                        Structure = StringConstant.Proprietorship
                    }
                }
            };
            var relativeEntities = new List<EntityRelationshipMapping>();
            var entityTaxForms = new List<EntityTaxForm>() {
                new EntityTaxForm()
                {
                    Id = commonGuid,
                    EntityId = commonGuid,
                    TaxFormId = commonGuid
                }
            };
            var taxFormCompanyStructureMapping = GetTaxFormCompanyStructureMappings();
            var entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() {
                new EntityTaxYearlyMapping()
                {
                    EntityTaxFormId = commonGuid,
                    DocumentId = commonGuid,
                    Period = "2019",
                    UploadedDocument = new Document()
                    {
                        Name = "XYZ",
                        Path = "ABC"
                    }
                }
            };
            var actualTaxList = new List<TaxAC>()
            {
                new TaxAC()
                {
                    Id = commonGuid
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("2");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>())).Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>())).Returns(relativeEntities.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<TaxFormCompanyStructureMapping, bool>>>())).Returns(taxFormCompanyStructureMapping.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxForms.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);

            // Act
            var result = await _entityTaxReturnRepository.GetTaxReturnInfoAsync(entityId, ResourceType.Company, _currentUserAC);

            // Assert
            Assert.Equal(result.Taxes.Count, actualTaxList.Count);
        }

        /// <summary>
        /// Check if another user who is not linked with loan try to access the version then throw UnauthorizedAccess Exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxReturnInfoAsync_EntityExists_LoanVersion_UserNotLinkedToLoan_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var applicationId = commonGuid;
            var companies = new List<Company>() {
                new Company
                {
                    Id = commonGuid,
                    CompanyStructure = new CompanyStructure()
                    {
                        Structure = StringConstant.Proprietorship
                    }
                }
            };
            var relativeEntities = new List<EntityRelationshipMapping>();
            var entityTaxForms = new List<EntityTaxForm>() {
                new EntityTaxForm()
                {
                    Id = commonGuid,
                    EntityId = commonGuid,
                    TaxFormId = commonGuid
                }
            };
            var taxFormCompanyStructureMapping = GetTaxFormCompanyStructureMappings();
            var entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() {
                new EntityTaxYearlyMapping()
                {
                    EntityTaxFormId = commonGuid,
                    DocumentId = commonGuid,
                    Period = "2019",
                    UploadedDocument = new Document()
                    {
                        Name = "XYZ",
                        Path = "ABC"
                    }
                }
            };
            var actualTaxList = new List<TaxAC>()
            {
                new TaxAC()
                {
                    Id = commonGuid
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("2");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>())).Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>())).Returns(relativeEntities.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<TaxFormCompanyStructureMapping, bool>>>())).Returns(taxFormCompanyStructureMapping.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxForms.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);
            // Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _entityTaxReturnRepository.GetTaxReturnInfoAsync(applicationId, ResourceType.Loan, _currentUserAC));
        }

        /// <summary>
        /// Check successfully able to get taxes for particular loan version.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxReturnInfoAsync_EntityExists_EntityTaxFormsExists_LoanVersion_TaxesCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var applicationId = commonGuid;
            var companies = new List<Company>() {
                new Company
                {
                    Id = commonGuid,
                    CompanyStructure = new CompanyStructure()
                    {
                        Structure = StringConstant.Proprietorship
                    }
                }
            };
            var relativeEntities = new List<EntityRelationshipMapping>();
            var entityTaxForms = new List<EntityTaxForm>() {
                new EntityTaxForm()
                {
                    Id = commonGuid,
                    EntityId = commonGuid,
                    TaxFormId = commonGuid,
                    SurrogateId = 1,
                    Version = Guid.NewGuid(),
                    TaxForm = new TaxForm
                    {
                        Id = Guid.NewGuid()
                    }
                }
            };
            var taxFormCompanyStructureMapping = GetTaxFormCompanyStructureMappings();
            var entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() {
                new EntityTaxYearlyMapping()
                {
                    EntityTaxFormId = commonGuid,
                    DocumentId = commonGuid,
                    Period = "2019",
                    UploadedDocument = new Document()
                    {
                        Name = "XYZ",
                        Path = "ABC"
                    }
                }
            };
            var actualTaxList = new List<TaxAC>()
            {
                new TaxAC()
                {
                    Id = commonGuid
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("2");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>())).Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>())).Returns(relativeEntities.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<TaxFormCompanyStructureMapping, bool>>>())).Returns(taxFormCompanyStructureMapping.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxForms.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);

            // Act
            var result = await _entityTaxReturnRepository.GetTaxReturnInfoAsync(applicationId, ResourceType.Loan, _currentUserAC);

            // Assert
            Assert.Equal(result.Taxes.Count, actualTaxList.Count);
        }

        /// <summary>
        /// Check successfully able to create create version and get taxes
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxReturnInfoAsync_EntityExists_EntityTaxFormsExists_CreateVersion_TaxesCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var companies = new List<Company>() {
                new Company
                {
                    Id = entityId,
                    CompanyStructure = new CompanyStructure()
                    {
                        Structure = StringConstant.Proprietorship
                    }
                }
            };
            var relativeEntities = new List<EntityRelationshipMapping>();

            var taxFormCompanyStructureMapping = GetTaxFormCompanyStructureMappings();
            var entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() {
                new EntityTaxYearlyMapping()
                {
                    EntityTaxFormId = commonGuid,
                    DocumentId = commonGuid,
                    Period = "2019",
                    UploadedDocument = new Document()
                    {
                        Name = "XYZ",
                        Path = "ABC"
                    },
                    TaxFormValueLabelMappings = new List<TaxFormValueLabelMapping>()
                    {
                        new TaxFormValueLabelMapping
                        {
                            Id = Guid.NewGuid()
                        }
                    }
                }
            };

            var entityTaxForms = new List<EntityTaxForm>() {
                new EntityTaxForm()
                {
                    Id = commonGuid,
                    EntityId = commonGuid,
                    TaxFormId = commonGuid,
                    LoanApplicationId = Guid.NewGuid(),
                    SurrogateId = 1,
                    Version = Guid.NewGuid(),
                    EntityTaxYearlyMappings = entityTaxYearlyMappings
                }
            };

            var actualTaxList = new List<TaxAC>()
            {
                new TaxAC()
                {
                    Id = commonGuid
                }
            };
            List<Document> documents = new List<Document>()
            {
                new Document
                {
                    Id = Guid.NewGuid()
                }
            };
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("2");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Company, bool>>>())).Returns(companies.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>())).Returns(relativeEntities.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<TaxFormCompanyStructureMapping, bool>>>())).Returns(taxFormCompanyStructureMapping.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxForms.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<EntityTaxForm>>(), true))
                .Returns(entityTaxForms);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<EntityTaxYearlyMapping>>(), true))
                .Returns(entityTaxYearlyMappings);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<TaxFormValueLabelMapping>>(), true))
                .Returns(entityTaxYearlyMappings.First().TaxFormValueLabelMappings);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<Document>>(), true))
               .Returns(documents);
            // Act
            var result = await _entityTaxReturnRepository.GetTaxReturnInfoAsync(entityId, ResourceType.Company, _currentUserAC);

            // Assert
            Assert.Equal(result.Taxes.Count, actualTaxList.Count);
        }

        /// <summary>
        /// Method to verify that it throws an error if user is unauthorized to access the loan application.
        /// </summary>
        [Fact]
        public async Task SaveTaxReturnInfoAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            //Arrange
            var entityId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);
            var entityAC = new EntityAC()
            {
                Taxes = new List<TaxAC>()
                {
                    new TaxAC()
                    {
                        Id = Guid.NewGuid(),

                        EntityTaxAccount = new EntityTaxAccountAC()
                        {
                            Period = "2019"
                        }
                    }
                },
                Periods = new List<string>
                {
                    "2019",
                    "2018"
                }
            };

            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(entityId, entityAC, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that it throws an error if all the tax return files are not added
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveTaxReturnInfoAsync_AddAllTaxReturns_VerifyThrowsValidationException()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var entityAC = new EntityAC()
            {
                Taxes = new List<TaxAC>()
                {
                    new TaxAC()
                    {
                        Id = Guid.NewGuid(),

                        EntityTaxAccount = new EntityTaxAccountAC()
                        {
                            Period = "2019"
                        }
                    }
                },
                Periods = new List<string>
                {
                    "2019",
                    "2018"
                }
            };
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("All");

            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(entityId, entityAC, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that it throws an error if no tax return files are added
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveTaxReturnInfoAsync_NoTaxReturnAdded_VerifyThrowsValidationException()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var entityAC = new EntityAC()
            {
                Taxes = new List<TaxAC>(),
                Periods = new List<string>
                {
                    "2019"
                }
            };
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("AnyOne");

            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(entityId, entityAC, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that it throws an error if tax year is null in the uploaded document
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveTaxReturnInfoAsync_TaxReturnWithoutDocumentPeriodAdded_VerifyThrowsValidationException()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var entityAC = new EntityAC()
            {
                Taxes = new List<TaxAC>() {
                    new TaxAC()
                    {
                        Id = commonGuid,
                        EntityTaxAccount = new EntityTaxAccountAC()
                        {
                            Period = null
                        }
                    }
                },
                Periods = new List<string>
                {
                    "2019"
                }
            };
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("AnyOne");

            // Act

            // Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(entityId, entityAC, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that it throws an error if upload tax form journey is not allowed and then also trying to upload the document
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveTaxReturnInfoAsync_TaxFormJourneyNotAllowed_VerifyThrowsInvalidResourceAccessException()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var entityAC = new EntityAC()
            {
                Taxes = new List<TaxAC>() {
                    new TaxAC()
                    {
                        Id = commonGuid,
                        EntityTaxAccount = new EntityTaxAccountAC()
                        {
                            Period = null
                        }
                    }
                },
                Periods = new List<string>
                {
                    "2019"
                }
            };
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("false");

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(entityId, entityAC, _currentUserAC));
        }


        [Fact]
        public async Task SaveTaxReturnInfoAsync_AddEntityTaxReturns_VerifyCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var entityAC = new EntityAC()
            {
                Taxes = new List<TaxAC>()
                {
                    new TaxAC()
                    {
                        EntityTaxAccount = new EntityTaxAccountAC()
                        {
                            Period = "2019",
                            Document = new DocumentAC()
                            {
                                Name = "ABC",
                                Path = "XYZ"
                            }
                        }
                    }
                },
                Periods = new List<string>
                {
                    "2019"
                }
            };

            var taxForms = new List<TaxForm>()
            {
                new TaxForm()
                {
                    Name = StringConstant.TaxReturns
                }
            };

            var entityTaxFormDbData = new List<EntityTaxForm>();

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("AnyOne");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxFormDbData.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<TaxForm>()).Returns(taxForms.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            // Act
            await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(entityId, entityAC, _currentUserAC);

            // Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityTaxForm>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Document>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityTaxYearlyMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method is to verify the count of dataRepository call when need to remove each and every document stored in the database
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveTaxReturnInfoAsync_RemoveAllPreviousEntityTaxReturn_AddNewEntityTaxReturn_VerifyCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var entityAC = new EntityAC()
            {
                Taxes = new List<TaxAC>()
                {
                    new TaxAC()
                    {
                        EntityTaxAccount = new EntityTaxAccountAC()
                        {
                            Period = "2019",
                            Document = new DocumentAC()
                            {
                                Name = "ABC",
                                Path = "temp_/XYZ"
                            }
                        }
                    }
                },
                Periods = new List<string>
                {
                    "2019"
                }
            };
            var entityTaxFormDbData = new List<EntityTaxForm>() { new EntityTaxForm() { Id = commonGuid, EntityId = commonGuid, TaxFormId = commonGuid, EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() { new EntityTaxYearlyMapping() { DocumentId = commonGuid, EntityTaxFormId = commonGuid, Period = "2019" } } } };
            var document = new List<Document>()
            {
                new Document()
                {
                    Id = commonGuid,
                    Name = "XYZ",
                    Path = "ABC"
                }
            };

            var taxForms = new List<TaxForm>()
            {
                new TaxForm()
                {
                    Name = StringConstant.TaxReturns
                }
            };
            List<string> documentPaths = new List<string>
            {
                "XYZ"
            };

            var entityLoanApplicationMappingList = new List<EntityLoanApplicationMapping>();
            string path = "permanent path";

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("AnyOne");
            _dataRepositoryMock.Setup(x => x.GetAll<TaxForm>()).Returns(taxForms.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxFormDbData.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.GetPathForKeyNameBucket(It.IsAny<Guid>(), It.IsAny<EntityTaxAccountAC>())).Returns(path);
            _amazonServiceUtility.Setup(x => x.CopyObject(It.IsAny<string>(), It.IsAny<string>()));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Document, bool>>>())).Returns(document.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _amazonServiceUtility.Setup(x => x.DeleteObjectsAsync(documentPaths)).Verifiable();
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            // Act
            await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(entityId, entityAC, _currentUserAC);

            // Assert
            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<List<Document>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Document>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityTaxYearlyMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method is to verify the count of dataRepository call when need to remove one document stored in the database
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveTaxReturnInfoAsync_RemoveOnePreviousEntityTaxReturn_AddNewEntityTaxReturn_VerifyCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var entityTaxFormDbData = new List<EntityTaxForm>() {
                new EntityTaxForm()
                {
                    Id = Guid.NewGuid(),
                    EntityId = commonGuid,
                    TaxFormId = commonGuid,
                    EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>()
                    {
                        new EntityTaxYearlyMapping()
                        {
                            DocumentId = Guid.NewGuid(),
                            EntityTaxFormId = commonGuid,
                            Period = "2018"
                        }
                    }
                },
                new EntityTaxForm()
                {
                    Id = commonGuid,
                    EntityId = commonGuid,
                    TaxFormId = commonGuid,
                    EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>()
                    {
                        new EntityTaxYearlyMapping()
                        {
                            Id = commonGuid,
                            DocumentId = commonGuid,
                            EntityTaxFormId = commonGuid,
                            Period = "2019"
                        }
                    }
                }
            };
            var entityAC = new EntityAC()
            {
                Taxes = new List<TaxAC>()
                {
                    new TaxAC()
                    {
                        Id = commonGuid,
                        EntityTaxAccount = new EntityTaxAccountAC()
                        {
                            Id = commonGuid,
                            Period = "2019",
                            Document = new DocumentAC()
                            {
                                Id = commonGuid,
                                Name = "ABC",
                                Path = "XYZ"
                            }
                        }
                    },
                    new TaxAC()
                    {
                        Id = Guid.Empty,
                        EntityTaxAccount = new EntityTaxAccountAC()
                        {
                            Id = Guid.Empty,
                            Period = "2018",
                            Document = new DocumentAC()
                            {
                                Name = "HK",
                                Path = "temp_HK"
                            }
                        }
                    }
                },
                Periods = new List<string>
                {
                    "2019",
                    "2018"
                }
            };

            var document = new List<Document>()
            {
                new Document()
                {
                    Id = entityTaxFormDbData.First().EntityTaxYearlyMappings.First().DocumentId,
                    Name = "XYZ",
                    Path = "ABC"
                }
            };

            var taxForms = new List<TaxForm>()
            {
                new TaxForm()
                {
                    Name = StringConstant.TaxReturns
                }
            };
            List<string> documentPaths = new List<string>
            {
                "XYZ"
            };

            var entityLoanApplicationMappingList = new List<EntityLoanApplicationMapping>();

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("AnyOne");
            _dataRepositoryMock.Setup(x => x.GetAll<TaxForm>()).Returns(taxForms.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxFormDbData.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Document, bool>>>())).Returns(document.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _amazonServiceUtility.Setup(x => x.DeleteObjectsAsync(documentPaths)).Verifiable();
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            // Act
            await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(entityId, entityAC, _currentUserAC);

            // Assert
            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<List<Document>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Document>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityTaxYearlyMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method to verify the count of dataRepository call when need to remove one document stored in the database and remove file from S3 bucket
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveTaxReturnInfoAsync_RemovePreviousEntityTaxReturnFromS3Bucket_AddNewEntityTaxReturn_VerifyCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var entityId = commonGuid;
            var entityTaxFormDbData = new List<EntityTaxForm>() { new EntityTaxForm() { Id = Guid.NewGuid(), EntityId = commonGuid, TaxFormId = commonGuid, EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() { new EntityTaxYearlyMapping() { DocumentId = Guid.NewGuid(), EntityTaxFormId = commonGuid, Period = "2018" } } }, new EntityTaxForm() { Id = commonGuid, EntityId = commonGuid, TaxFormId = commonGuid, EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() { new EntityTaxYearlyMapping() { Id = commonGuid, DocumentId = commonGuid, EntityTaxFormId = commonGuid, Period = "2019" } } } };
            var entityAC = new EntityAC() { Taxes = new List<TaxAC>() { new TaxAC() { Id = commonGuid, EntityTaxAccount = new EntityTaxAccountAC() { Id = commonGuid, Period = "2019", Document = new DocumentAC() { Name = "ABC", Path = "XYZ" } } }, new TaxAC() { Id = Guid.Empty, EntityTaxAccount = new EntityTaxAccountAC() { Id = Guid.Empty, Period = "2018", Document = new DocumentAC() { Name = "HK", Path = "temp_HK" } } } }, Periods = new List<string> { "2019", "2018" } };
            var document = new List<Document>() { new Document() { Id = entityTaxFormDbData.First().EntityTaxYearlyMappings.First().DocumentId, Name = "Hello", Path = "ABC" } };
            var taxForms = new List<TaxForm>() { new TaxForm() { Name = StringConstant.TaxReturns } };
            List<string> documentPaths = new List<string> { "ABC" };
            var entityLoanApplicationMappingList = new List<EntityLoanApplicationMapping>() { new EntityLoanApplicationMapping() { Id = commonGuid, LoanApplication = new DomainModel.Models.LoanApplicationInfo.LoanApplication() { Status = DomainModel.Enums.LoanApplicationStatusType.Approved,
                UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = _currentUserAC.Id,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Name = StringConstant.LoanConsentSection }
                            }
                        } } } };
            var loanApplicationSnapshots = new List<LoanApplicationSnapshot>() {
                new LoanApplicationSnapshot
                {
                    Id = Guid.NewGuid(),
                    ApplicationDetailsJson = "{\"BasicDetails\":{\"Id\":\"1dab3e0e-6b61-4426-b966-0eed64dd3489\",\"EntityId\":\"1dab3e0e-6b61-4426-b966-0eed64d22222\",\"LoanApplicationNumber\":\"LP2012202013526332\"},\"BorrowingEntities\":[{\"Id\":\"1dab3e0e-6b61-4426-b966-0eed64d22222\", \"Taxes\":[{\"Id\":\"eb889074-061c-4547-9488-395957ee8568\",\"EntityTaxAccount\":{\"Id\":\"199905b0-1cbe-4daa-8387-6d2060c2d068\", \"Period\":\"2019\", \"Document\":{\"Id\":\"7bbdaa31-45b0-4cd6-a0e8-efeae9a1213e\",\"Name\":\"XYZ\"}}}]}]}",
                    LoanApplicationId = commonGuid
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("AnyOne");
            _dataRepositoryMock.Setup(x => x.GetAll<TaxForm>()).Returns(taxForms.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxFormDbData.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<Document, bool>>>())).Returns(document.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplicationSnapshot, bool>>>())).Returns(loanApplicationSnapshots.AsQueryable().BuildMock().Object);
            _amazonServiceUtility.Setup(x => x.DeleteObjectsAsync(documentPaths)).Verifiable();
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            // Act
            await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(entityId, entityAC, _currentUserAC);

            // Assert
            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<List<Document>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Document>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityTaxYearlyMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method to verify that it throws an error if user is unauthorized to access the loan application.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetExtractedValuesOfDocumentAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _entityTaxReturnRepository.GetExtractedValuesOfDocumentAsync(loanApplicationId, documentId, _currentUserAC));
        }

        /// <summary>
        /// Mehtod to verify that it will throws an error if loan application status is in draft state
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetExtractedValuesOfDocumentAsync_LoanApplicationStatusDraft_ThrowsDataNotException()
        {
            // Arrange
            var loanApplication = new LoanApplication { Id = Guid.NewGuid(), Status = DomainModel.Enums.LoanApplicationStatusType.Draft };

            var documentId = Guid.NewGuid();
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));


            // Act

            // Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _entityTaxReturnRepository.GetExtractedValuesOfDocumentAsync(loanApplication.Id, documentId, _currentBankUserAC));
        }

        /// <summary>
        /// Method to verify that it will throw an error if no document snapshot found and loan application status is in locked state
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetExtractedValuesOfDocumentAsync_LoanApplicationStatusLocked_ThrowsInvalidParameterException()
        {
            // Arrange
            var loanApplication = new LoanApplication { Id = Guid.NewGuid(), Status = DomainModel.Enums.LoanApplicationStatusType.Locked };
            var documentId = Guid.NewGuid();
            var loanApplicationSnapshot = new LoanApplicationSnapshot
            {
                Id = Guid.NewGuid(),
                ApplicationDetailsJson = "{\"BasicDetails\":{\"Id\":\"1dab3e0e-6b61-4426-b966-0eed64dd3489\",\"EntityId\":\"1dab3e0e-6b61-4426-b966-0eed64d22222\",\"LoanApplicationNumber\":\"LP2012202013526332\"},\"BorrowingEntities\":[{\"Id\":\"1dab3e0e-6b61-4426-b966-0eed64d22222\", \"Taxes\":[{\"Id\":\"eb889074-061c-4547-9488-395957ee8568\",\"EntityTaxAccount\":{\"Id\":\"199905b0-1cbe-4daa-8387-6d2060c2d068\", \"Period\":\"2019\", \"Document\":{\"Id\":\"7bbdaa31-45b0-4cd6-a0e8-efeae9a1213e\",\"Name\":\"XYZ\"}}}]}]}",
                LoanApplicationId = loanApplication.Id
            };
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanApplicationSnapshot, bool>>>())).Returns(Task.FromResult(loanApplicationSnapshot));

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityTaxReturnRepository.GetExtractedValuesOfDocumentAsync(loanApplication.Id, documentId, _currentBankUserAC));
        }

        /// <summary>
        /// Method is to verify the single extracted value from the PDF
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetExtractedValuesOfDocumentAsync_LoanApplicationStatusLocked_VerifySingle()
        {
            // Arrange
            var loanApplication = new LoanApplication { Id = Guid.NewGuid(), Status = DomainModel.Enums.LoanApplicationStatusType.Locked };
            var documentId = Guid.NewGuid();
            var taxFormValueLabelMappings = new List<TaxFormValueLabelMapping>()
            {
                new TaxFormValueLabelMapping
                {
                    Id = Guid.NewGuid(),
                    TaxformLabelNameMappingId = Guid.NewGuid(),
                    TaxformLabelNameMapping = new TaxFormLabelNameMapping
                    {
                        Id  = Guid.NewGuid(),
                        Order = 1
                    }
                }
            };
            var entityTaxYearlyMapping = new EntityTaxYearlyMapping
            {
                Id = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
                EntityTaxForm = new EntityTaxForm
                {
                    Id = Guid.NewGuid()
                },
                EntityTaxFormId = Guid.NewGuid(),
                TaxFormValueLabelMappings = taxFormValueLabelMappings,
                UploadedDocument = new Document
                {
                    Id = Guid.NewGuid()
                },
                Period = "2019"
            };
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityTaxYearlyMapping, bool>>>())).Returns(Task.FromResult(entityTaxYearlyMapping));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<TaxFormValueLabelMapping, bool>>>())).Returns(taxFormValueLabelMappings.AsQueryable().BuildMock().Object);

            // Act
            var result = await _entityTaxReturnRepository.GetExtractedValuesOfDocumentAsync(loanApplication.Id, documentId, _currentBankUserAC);

            // Assert
            Assert.Single(result);
        }

        /// <summary>
        /// Method to verify that it will throw an error if no entity tax yearly data found and loan application status is in unlocked state
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetExtractedValuesOfDocumentAsync_LoanApplicationStatusUnlocked_EntityTaxYearlyMappingNotExists_ThrowsInvalidParameterException()
        {
            // Arrange
            var loanApplication = new LoanApplication { Id = Guid.NewGuid(), Status = DomainModel.Enums.LoanApplicationStatusType.Unlocked };
            var documentId = Guid.NewGuid();
            EntityTaxYearlyMapping entityTaxYearlyMapping = null;
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityTaxYearlyMapping, bool>>>())).Returns(Task.FromResult(entityTaxYearlyMapping));

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityTaxReturnRepository.GetExtractedValuesOfDocumentAsync(loanApplication.Id, documentId, _currentBankUserAC));
        }

        /// <summary>
        /// Method to verify count of taxformvaluelabelmapping list
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetExtractedValuesOfDocumentAsync_LoanApplicationStatusUnlocked_EntityTaxYearlyMappingExists_VerifyCount()
        {
            // Arrange
            var loanApplication = new LoanApplication { Id = Guid.NewGuid(), Status = DomainModel.Enums.LoanApplicationStatusType.Unlocked };
            var documentId = Guid.NewGuid();
            var entityTaxYearlyMapping = new EntityTaxYearlyMapping() { DocumentId = documentId, Id = Guid.NewGuid(), EntityTaxFormId = Guid.NewGuid(), Period = "2019" };
            var taxFormValueLabelMappings = new List<TaxFormValueLabelMapping>() { new TaxFormValueLabelMapping() { EntityTaxYearlyMappingId = entityTaxYearlyMapping.Id, Value = "XYZ", Confidence = 1, TaxformLabelNameMapping = new TaxFormLabelNameMapping() { LabelFieldName = "ABC" } } };
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityTaxYearlyMapping, bool>>>())).Returns(Task.FromResult(entityTaxYearlyMapping));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<TaxFormValueLabelMapping, bool>>>())).Returns(taxFormValueLabelMappings.AsQueryable().BuildMock().Object);


            // Act
            var result = await _entityTaxReturnRepository.GetExtractedValuesOfDocumentAsync(loanApplication.Id, documentId, _currentBankUserAC);

            // Assert
            Assert.Equal(result.Count, taxFormValueLabelMappings.Count);
        }

        /// <summary>
        /// Method to verify that it throws an error if user is unauthorized to access the loan application.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateTaxFormValueAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            //Arrange
            var taxFormValueLabelMappings = new List<TaxFormValueLabelMappingAC>();
            var documentId = Guid.NewGuid();
            var entityId = Guid.NewGuid();
            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _entityTaxReturnRepository.UpdateTaxFormValueAsync(entityId, documentId, taxFormValueLabelMappings, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that it throws an error if taxformvaluelabelmapping not exists.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateTaxFormValueAsync_TaxFormValueLabelMappingNotExists_ThrowsInvalidParameterException()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var taxFormValueLabelMappingACs = new List<TaxFormValueLabelMappingAC>();
            var documentId = Guid.NewGuid();
            List<EntityTaxYearlyMapping> entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch<EntityTaxYearlyMapping>(It.IsAny<Expression<Func<EntityTaxYearlyMapping, bool>>>())).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);
            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityTaxReturnRepository.UpdateTaxFormValueAsync(entityId, documentId, taxFormValueLabelMappingACs, _currentBankUserAC));

        }

        /// <summary>
        /// Method is to verify total how many time UpdateRange method call while updating the value 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateTaxFormValueAsync_UpdateTaxForm_Verify()
        {
            // Arrange
            var entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>()
            {
                new EntityTaxYearlyMapping
                {
                    Id = Guid.NewGuid(),
                    DocumentId = Guid.NewGuid(),
                    EntityTaxForm = new EntityTaxForm
                    {
                        Id = Guid.NewGuid(),
                        SurrogateId = 1
                    },
                    EntityTaxFormId = Guid.NewGuid(),
                    UploadedDocument = new Document
                    {
                        Id = Guid.NewGuid()
                    },
                    Period = "2019",
                    TaxFormValueLabelMappings = new List<TaxFormValueLabelMapping>()
                    {
                        new TaxFormValueLabelMapping
                        {
                            Id= Guid.NewGuid(),
                            TaxformLabelNameMapping = new TaxFormLabelNameMapping
                            {
                                Id = Guid.NewGuid(),
                                Order = 1
                            }
                        }
                    }
                }
            };


            List<EntityTaxForm> entityTaxForms = new List<EntityTaxForm>
            {
                new EntityTaxForm
                {
                    Id = Guid.NewGuid(),
                    EntityTaxYearlyMappings = new List<EntityTaxYearlyMapping>()
                    {
                        new EntityTaxYearlyMapping
                        {
                            Id = Guid.NewGuid(),
                            UploadedDocument = new Document
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    },
                    SurrogateId = 1,
                    Version = Guid.NewGuid(),
                    LoanApplicationId = Guid.NewGuid()
                }
            };
            var documents = new List<Document>()
            {
                entityTaxYearlyMappings.First().UploadedDocument
            };
            var entityId = Guid.NewGuid();
            var taxFormValueLabelMappingACs = new List<TaxFormValueLabelMappingAC>() { new TaxFormValueLabelMappingAC { Id = Guid.NewGuid(), Value = "100", CorrectedValue = "1000", Confidence = 1, Label = "XYZ" } };
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.Fetch<EntityTaxYearlyMapping>(It.IsAny<Expression<Func<EntityTaxYearlyMapping, bool>>>())).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<EntityTaxForm>>(), true))
                .Returns(entityTaxForms);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<EntityTaxYearlyMapping>>(), true))
                .Returns(entityTaxYearlyMappings);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<TaxFormValueLabelMapping>>(), true))
                .Returns(entityTaxYearlyMappings.First().TaxFormValueLabelMappings);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<TaxFormValueLabelMapping>>(), false))
                .Returns(entityTaxYearlyMappings.First().TaxFormValueLabelMappings);
            _dataRepositoryMock.Setup(x => x.DetachEntities(It.IsAny<IQueryable<Document>>(), true))
                .Returns(documents);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityTaxForm>(It.IsAny<Expression<Func<EntityTaxForm, bool>>>())).Returns(entityTaxForms.AsQueryable().BuildMock().Object);
            // Act
            await _entityTaxReturnRepository.UpdateTaxFormValueAsync(entityId, Guid.NewGuid(), taxFormValueLabelMappingACs, _currentBankUserAC);

            // Assert
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method to verify that it will throw an error when there is no entityTaxYearlyMappign list exists
        /// </summary>
        [Fact]
        public void GetTaxes_EntityTaxYearlyMappingNotExists_ThrowsInvalidParameterException()
        {
            // Arrange
            var entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>();
            var taxes = new List<TaxAC>();
            var entityTaxForms = new List<EntityTaxForm>();
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);

            // Act

            // Assert
            Assert.Throws<InvalidParameterException>(() => _entityTaxReturnRepository.GetTaxes(taxes, entityTaxForms));
        }

        /// <summary>
        /// Method is to verify that we are not getting empty taxes list
        /// </summary>
        [Fact]
        public void GetTaxes_EntityTaxYearlyMappingExists_VerifyNotEmpty()
        {
            // Arrange
            var temp = Guid.NewGuid();
            var taxes = new List<TaxAC>();
            var entityTaxForms = new List<EntityTaxForm>() { new EntityTaxForm { EntityId = Guid.NewGuid(), TaxFormId = Guid.NewGuid(), Id = Guid.NewGuid() } };
            var entityTaxYearlyMappings = new List<EntityTaxYearlyMapping>() { new EntityTaxYearlyMapping { Id = temp, EntityTaxFormId = entityTaxForms.First().Id, Period = "2019", UploadedDocument = new Document { Name = "XYZ", Path = "XYZ" }, TaxFormValueLabelMappings = new List<TaxFormValueLabelMapping>() { new TaxFormValueLabelMapping { Confidence = 1, Value = "100", TaxformLabelNameMappingId = Guid.NewGuid(), EntityTaxYearlyMappingId = temp } } } };
            _dataRepositoryMock.Setup(x => x.GetAll<EntityTaxYearlyMapping>()).Returns(entityTaxYearlyMappings.AsQueryable().BuildMock().Object);

            // Act
            var result = _entityTaxReturnRepository.GetTaxes(taxes, entityTaxForms);

            // Arrange
            Assert.NotEmpty(result);

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Fetch the logged in user details (CurrentUserAC).
        /// </summary>
        /// <returns></returns>
        private CurrentUserAC FetchLoggedInUserAC()
        {
            return new CurrentUserAC() { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@doe.com", IsBankUser = false };
        }

        /// <summary>
        /// Fetch the logged in bank user details (CurrentUserAC)
        /// </summary>
        /// <returns></returns>
        private CurrentUserAC FetchLoggedInBankUserAC()
        {
            return new CurrentUserAC() { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@doe.com", IsBankUser = true };
        }
        #endregion
    }
}
