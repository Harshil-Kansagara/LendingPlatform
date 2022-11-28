using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.ApplicationClass.EntityInfo;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
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
    public class TaxReturnRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly ITaxReturnRepository _taxReturnRepository;
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly Mock<IAmazonS3Utility> _amazonS3Utility;
        #endregion

        #region Constructor
        public TaxReturnRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _taxReturnRepository = bootstrap.ServiceProvider.GetService<ITaxReturnRepository>();
            _amazonS3Utility = bootstrap.ServiceProvider.GetService<Mock<IAmazonS3Utility>>();
            _dataRepositoryMock.Reset();
            _amazonS3Utility.Reset();
        }
        #endregion

        #region Public Method

        /// <summary>
        /// Test method to verify count of entity finance yearly mapping and financial account type list of tax return data when tax return data not exists in entity finance table.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxReturnInfoByLoanIdAsync_VerifyCountOfEntityFinanceYearlyMappingAndFinancialAccountType()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var financialStatementExpected = new FinancialStatement()
            {
                Id = guid,
                Name = StringConstant.TaxReturns
            };
            var accountTypeExpected = new List<FinancialAccountType>()
            {
                new FinancialAccountType()
                {
                    Id = guid,
                    FinancialStatementId = financialStatementExpected.Id,
                    Name = StringConstant.TotalIncome
                }
            };

            var entityFinanceExpectedList = new List<EntityFinance>();

            var expected = new EntityFinanceAC
            {
                FinancialAccountTypeACs = new List<FinancialAccountTypeAC>()
                {
                    new FinancialAccountTypeAC
                    {
                        Id = guid,
                        Name = StringConstant.TotalIncome,
                        FinancialStatementId = guid
                    }
                },

                EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                    {
                        new EntityFinanceYearlyMappingAC
                        {
                            Period = "1-12 2018"
                        },
                        new EntityFinanceYearlyMappingAC
                        {
                            Period = "1-12 2019"
                        },
                        new EntityFinanceYearlyMappingAC
                        {
                            Period = "1-12 2020"
                        }
                    }
            };
            var stringList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };
            var entityLoanApplicationMapping = new EntityLoanApplicationMapping()
            {
                Id = guid,
                EntityId = guid,
                LoanApplicationId = guid
            };
            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(Task.FromResult(entityLoanApplicationMapping));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<FinancialAccountType, bool>>>())).Returns(accountTypeExpected.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(entityFinanceExpectedList.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.GetListOfLastNFinancialYears(false)).Returns(stringList);
            _globalRepositoryMock.Setup(x => x.GetFinancialStatementFromNameAsync(It.IsAny<string>())).Returns(Task.FromResult(financialStatementExpected));


            // Act
            var result = await _taxReturnRepository.GetEntityFinanceTaxReturnInfoByLoanIdAsync(guid, userExpected);

            // Assert
            Assert.Equal(expected.EntityFinanceYearlyMappings.Count, result.EntityFinanceYearlyMappings.Count);
            Assert.Equal(expected.FinancialAccountTypeACs.Count, result.FinancialAccountTypeACs.Count);
        }

        /// <summary>
        /// Test method to verify that add and save operation is being performed on EntityFinanceAC object
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddUpdateTaxReturnInfoAsync_AddAndSaveOperationTest()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var financialStatementExpected = new FinancialStatement()
            {
                Id = guid,
                Name = StringConstant.TaxReturns
            };

            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = guid,
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };

            var entityFinanceDbListExpected = new List<EntityFinance>();

            var entityFinanceAC = new EntityFinanceAC
            {
                LoanApplicationId = guid,
                EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>
                {
                    new EntityFinanceYearlyMappingAC
                    {
                        Period = "Jan - Dec 2018",
                        EntityFinanceStandardAccounts = new List<EntityFinanceStandardAccountsAC>
                        {
                            new EntityFinanceStandardAccountsAC
                            {
                                FinancialAccountTypeId = guid,
                                Amount = 100
                            },
                            new EntityFinanceStandardAccountsAC
                            {
                                FinancialAccountTypeId = guid,
                                Amount = 100
                            }
                        }
                    }
                },
                FinancialStatement = StringConstant.TaxReturns
            };

            _globalRepositoryMock.Setup(x => x.GetFinancialStatementFromNameAsync(It.IsAny<string>())).Returns(Task.FromResult(financialStatementExpected));
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinanceDbListExpected.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));


            //Act
            await _taxReturnRepository.AddUpdateTaxReturnInfoAsync(entityFinanceAC, userExpected);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        ///  Test method is verify that update and save operation is being performed on EntityFinanceAC object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddUpdateTaxReturnInfoAsync_UpdateAndSaveOperationTest()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var financialStatementExpected = new FinancialStatement()
            {
                Id = guid,
                Name = StringConstant.TaxReturns
            };

            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = guid,
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };

            var entityFinanceDbListExpected = new List<EntityFinance>()
            {
                new EntityFinance()
                {
                    Id = guid,
                    EntityId = guid,
                    LoanApplicationId = guid
                }
            };

            var entityFinanceAC = new EntityFinanceAC
            {
                Id = guid,
                EntityId = guid,
                LoanApplicationId = guid,
                EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>
                {
                    new EntityFinanceYearlyMappingAC
                    {
                        Id = guid,
                        Period = "1-12 2017",
                        DocumentName="test.pdf",
                        UploadedDocumentId=guid,
                        UploadedDocumentPath=StringConstant.FileTemp + guid,
                        EntityFinanceStandardAccounts = new List<EntityFinanceStandardAccountsAC>
                        {
                            new EntityFinanceStandardAccountsAC
                            {
                                Id = guid,
                                FinancialAccountTypeId = guid,
                                Amount = 5000
                            }
                        }
                    }
                },
                FinancialStatement = StringConstant.TaxReturns
            };

            var entityFinanceYearlyMappings = new List<EntityFinanceYearlyMapping>
            {
                new EntityFinanceYearlyMapping()
                {
                    Id = guid,
                    UploadedDocumentId = guid,
                    Period = "1-12 2018",
                    EntityFinanceId = guid
                }
            };

            var entityFinanceStandardAccounts = new List<EntityFinanceStandardAccount>
            {
                new EntityFinanceStandardAccount
                {
                    Id = guid,
                    Amount = 0,
                    EntityFinancialYearlyMappingId = guid,
                    FinancialAccountTypeId = guid
                }
            };

            var uploadedDocumentList = new List<UploadedFinancialDocument>
            {
                new UploadedFinancialDocument
                {
                    DocumentName="test.pdf",
                    Id=guid,
                    UploadedDocumentPath=StringConstant.FileTemp + guid
                }
            };

            _globalRepositoryMock.Setup(x => x.GetFinancialStatementFromNameAsync(It.IsAny<string>())).Returns(Task.FromResult(financialStatementExpected));
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinanceDbListExpected.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(Task.FromResult(entityFinanceDbListExpected[0]));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinanceYearlyMapping, bool>>>())).Returns(entityFinanceYearlyMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinanceStandardAccount, bool>>>())).Returns(entityFinanceStandardAccounts.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<UploadedFinancialDocument>()).Returns(uploadedDocumentList.AsQueryable().BuildMock().Object);

            await _taxReturnRepository.AddUpdateTaxReturnInfoAsync(entityFinanceAC, userExpected);

            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<EntityFinance>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<EntityFinanceYearlyMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<EntityFinanceStandardAccount>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<UploadedFinancialDocument>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        [Fact]
        public async Task AddUpdateTaxReturnInfoAsync_ArgumentNullException_ThrowsError()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var financialStatementExpected = new FinancialStatement()
            {
                Id = guid,
                Name = StringConstant.TaxReturns
            };

            
            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = guid,
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };

            var entityFinanceDbListExpected = new List<EntityFinance>();

            var entityFinanceAC = new EntityFinanceAC
            {
                LoanApplicationId = guid,
                EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>
                {
                    new EntityFinanceYearlyMappingAC
                    {
                        Period = "1-12 2017",
                        EntityFinanceStandardAccounts = new List<EntityFinanceStandardAccountsAC>
                        {
                            new EntityFinanceStandardAccountsAC
                            {
                                FinancialAccountTypeId = guid,
                                Amount = null
                            }
                        }
                    }
                },
                FinancialStatement = StringConstant.TaxReturns
            };

            _globalRepositoryMock.Setup(x => x.GetFinancialStatementFromNameAsync(It.IsAny<string>())).Returns(Task.FromResult(financialStatementExpected));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinanceDbListExpected.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));


            //Act

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _taxReturnRepository.AddUpdateTaxReturnInfoAsync(entityFinanceAC, userExpected));
        }
        #endregion
    }
}
