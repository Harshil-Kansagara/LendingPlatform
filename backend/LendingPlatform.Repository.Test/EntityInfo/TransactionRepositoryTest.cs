using Microsoft.Extensions.DependencyInjection;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Utils.Utils;
using Moq;
using Xunit;
using System.Threading.Tasks;
using LendingPlatform.Utils.ApplicationClass.Yodlee;
using System;
using LendingPlatform.DomainModel.Models.EntityInfo;
using System.Linq.Expressions;
using LendingPlatform.Repository.CustomException;
using System.Collections.Generic;
using System.Linq;
using MockQueryable.Moq;
using AutoMapper;
using LendingPlatform.Repository.ApplicationClass.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using plaidEntity = LendingPlatform.Utils.ApplicationClass.Plaid.Entity;
using LendingPlatform.Utils.ApplicationClass.Plaid.Transactions;
using LendingPlatform.Utils.ApplicationClass.Plaid.Institution;

namespace LendingPlatform.Repository.Test.EntityInfo
{
    [Collection("Register Dependency")]
    public class TransactionRepositoryTest : BaseTest
    {
        #region Private Variables
        private readonly ITransactionRepository _transactionRepository;
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IYodleeUtility> _yodleeUtilityMock;
        private readonly IMapper _mapper;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IPlaidUtility> _plaidUtilityMock;
        #endregion

        #region Constructor
        public TransactionRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _transactionRepository = bootstrap.ServiceProvider.GetService<ITransactionRepository>();
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _yodleeUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IYodleeUtility>>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _mapper = bootstrap.ServiceProvider.GetService<IMapper>();
            _configurationMock = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _plaidUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IPlaidUtility>>();
            _configurationMock.Reset();
            _yodleeUtilityMock.Reset();
            _dataRepositoryMock.Reset();
            _plaidUtilityMock.Reset();
        }
        #endregion

        #region Public Method

        /// <summary>
        /// Check  fast link url matches with expected fast link url
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetYodleeFastLinkACAsync_FastLinkUrl_AssertCount()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            YodleeFastLinkAC expectedYodleeFastLinkAC = new YodleeFastLinkAC()
            {
                FastLinkUrl = "https://node.sandbox.yodlee.com/authenticate/restserver",
                AccessToken = "GxLPKJXi2LsGG2Ct0iBVvvujN84F"
            };

            _yodleeUtilityMock.Setup(x => x.GetFastLinkUrlAsync(It.IsAny<Guid>())).Returns(Task.FromResult(expectedYodleeFastLinkAC));

            // Act
            var result = await _transactionRepository.GetYodleeFastLinkACAsync(currentUserId);

            // Assert
            Assert.Equal(expectedYodleeFastLinkAC.FastLinkUrl, result.FastLinkUrl);
        }

        /// <summary>
        /// Verify the count of provider bank list when loan application already exists.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProviderBankListAsync_ProviderBankListExists_EntityNotExists_AssertError()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();

            var yearList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };

            var providerBanks = new List<ProviderBank>()
            {
                new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "XYZ"
                },
                new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "XYZ"
                }
            };

            var providerBankACs = _mapper.Map<List<ProviderBank>, List<ProviderBankAC>>(providerBanks);

            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(yearList);

            _dataRepositoryMock.Setup(x => x.Fetch<ProviderBank>(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(providerBanks.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("BankPreference:Transaction").Value).Returns("Yodlee");
            _configurationMock.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("2");

            // Act
            var result = await _transactionRepository.GetProviderBankListAsync(commonGuid, userExpected);

            // Assert
            Assert.Equal(providerBankACs.Count, result.ProviderBanks.Count);
        }

        /// <summary>
        /// Check when there is no bank provider for current loan application and if no entity relationship exist between loan and entity than throw error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProviderBankListAsync_NoProviderBankListExists_NoEntityExists_AssertThrowError()
        {
            // Arrange
            var loanApplicationId = Guid.NewGuid();

            var yearList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };

            var providerBanks = new List<ProviderBank>();

            var providerBankACs = _mapper.Map<List<ProviderBank>, List<ProviderBankAC>>(providerBanks);

            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };
            EntityLoanApplicationMapping expectedEntityLoanApplicationMapping = null;

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(yearList);
            _dataRepositoryMock.Setup(x => x.Fetch<ProviderBank>(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(providerBanks.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(Task.FromResult(expectedEntityLoanApplicationMapping));

            // Act

            // Assert 
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _transactionRepository.GetProviderBankListAsync(loanApplicationId, userExpected));
        }

        /// <summary>
        /// Check when there is no bank provider for current loan application and if no provider bank exist for previosuly filled loan having same entity
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProviderBankListAsync_NoProviderBankListExists_NoProviderBankListExistsForPreviousEntity_AssertEmpty()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var expectedEntityLoanApplicationMapping = new EntityLoanApplicationMapping()
            {
                Id = commonGuid,
                EntityId = commonGuid,
                LoanApplicationId = commonGuid
            };

            var yearList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };

            var providerBanks = new List<ProviderBank>();

            var providerBankACs = _mapper.Map<List<ProviderBank>, List<ProviderBankAC>>(providerBanks);

            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(yearList);
            _dataRepositoryMock.Setup(x => x.Fetch<ProviderBank>(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(providerBanks.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(Task.FromResult(expectedEntityLoanApplicationMapping));
            _dataRepositoryMock.Setup(x => x.Fetch<ProviderBank>(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(providerBanks.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("BankPreference:Transaction").Value).Returns("Yodlee");
            _configurationMock.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("2");

            // Act
            var result = await _transactionRepository.GetProviderBankListAsync(commonGuid, userExpected);

            // Assert 
            Assert.Empty(result.ProviderBanks);
        }

        /// <summary>
        /// Check when there is no bank provider for current loan application and if provider bank exist for previosuly filled loan having same entity than first save for current loan application and return provider bank list.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProviderBankListAsync_NoProviderBankListExists_ProviderBankListExistsForPreviousEntity_AssertEqual_VerifyCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var expectedEntityLoanApplicationMapping = new EntityLoanApplicationMapping()
            {
                Id = commonGuid,
                EntityId = commonGuid,
                LoanApplicationId = commonGuid
            };

            var yearList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };

            var emptyProviderBanks = new List<ProviderBank>();

            var providerBanks = new List<ProviderBank>() {

                new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "XYZ",
                    LoanApplicationId = commonGuid
                },
                new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "XYZ",
                    LoanApplicationId = commonGuid
                }
            };

            var bankAccountTransactions = new List<BankAccountTransaction>()
            {
                 new BankAccountTransaction()
                {
                    AccountId = "123456",
                    AccountName = "XYZ",
                    CurrentBalance = 1234567,
                    AccountType = "ABC"
                }
            };

            var providerBankACs = _mapper.Map<List<ProviderBank>, List<ProviderBankAC>>(providerBanks);

            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(yearList);
            _dataRepositoryMock.SetupSequence(x => x.Fetch<ProviderBank>(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(emptyProviderBanks.AsQueryable().BuildMock().Object).Returns(providerBanks.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(Task.FromResult(expectedEntityLoanApplicationMapping));
            _dataRepositoryMock.Setup(x => x.Fetch<BankAccountTransaction>(It.IsAny<Expression<Func<BankAccountTransaction, bool>>>())).Returns(bankAccountTransactions.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()));
            _configurationMock.Setup(x => x.GetSection("BankPreference:Transaction").Value).Returns("Yodlee");
            _configurationMock.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("2");

            // Act
            var result = await _transactionRepository.GetProviderBankListAsync(commonGuid, userExpected);

            // Assert 
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<ProviderBank>(It.IsAny<List<ProviderBank>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<BankAccountTransaction>(It.IsAny<List<BankAccountTransaction>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.Equal(providerBankACs.Count, result.ProviderBanks.Count);
        }

        /// <summary>
        /// Check if account not exists in bankAccountTransaction than throw error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAccountTransactionListAsync_AccountNotExists_AssertThrowError()
        {
            // Arrange
            var bankProviderId = Guid.NewGuid();
            var providerBankList = new List<ProviderBank>()
            {
                new ProviderBank()
                {
                    Id = bankProviderId,
                    BankAccountTransactions = new List<BankAccountTransaction>()
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch<ProviderBank>(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(providerBankList.AsQueryable().BuildMock().Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _transactionRepository.GetAccountTransactionListAsync(bankProviderId));
        }

        /// <summary>
        /// Check the count of expected accounts and result accounts list
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAccountTransactionListAsync_AccountExists_AssertEqualCount()
        {
            // Arrange
            var bankProviderId = Guid.NewGuid();
            var bankAccountTransactions = new List<BankAccountTransaction>()
            {
                new BankAccountTransaction()
                {
                    AccountId = "123456",
                    AccountName = "XYZ",
                    AccountType = "Saving",
                    CurrentBalance= 32000,
                    ProviderBankId = bankProviderId,
                    TransactionInformationJson = "[{\"CONTAINER\": \"bank\",\"id\": 20147310, \"baseType\": \"DEBIT\"}]"
                }
            };
            var providerBankList = new List<ProviderBank>()
            {
                new ProviderBank()
                {
                    Id = bankProviderId,
                    BankAccountTransactions = bankAccountTransactions
                }
            };
            var bankAccountTransactionACs = _mapper.Map<List<BankAccountTransaction>, List<BankAccountTransactionAC>>(bankAccountTransactions);
            var transactionAC = JsonConvert.DeserializeObject<List<TransactionAC>>(bankAccountTransactions.First().TransactionInformationJson);
            bankAccountTransactionACs.First().TransactionACs = transactionAC;

            _dataRepositoryMock.Setup(x => x.Fetch<ProviderBank>(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(providerBankList.AsQueryable().BuildMock().Object);

            // Act
            var result = await _transactionRepository.GetAccountTransactionListAsync(bankProviderId);

            // Assert
            Assert.Equal(result.Count, bankAccountTransactionACs.Count);
        }

        /// <summary>
        /// Check how many times add method call for ProviderBank and bankAccountTransaction when there is no provider bank exists.
        /// Check the count of new Provider bank list.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateTransactionsAsync_PerformSaveOperation_NoProviderBankListExists_NoProviderBankExists_VerifyOperationCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };
            var year = "2";
            var month = "January";
            /*var transactionJsonAC = new TransactionJsonAC()
            {
                TransactionJson = @"{'transaction':[{'id':'98394753','date':'2020-03-12','type': 'PURCHASE', 'amount': {'amount':'360', 'currency':'inr'}, 'status': 'PENDING', 'subType': 'PURCHASE', 'baseType': 'DEBIT', 'category': 'Services/Supplies', 'isManual': 'false', 'description':{'simple': 'XYZ'}}]}"
            };*/
            var transactionDetailResponseAC = new List<TransactionDetailResponseAC>()
            {
                new TransactionDetailResponseAC()
                {
                    AccountId = 10193787,
                    Amount = new Utils.ApplicationClass.Yodlee.TransactionAmountAC()
                    {
                        Amount = 2000,
                        Currency = "USD"
                    },
                    Id = 1234567,
                    TransactionDate = "2020-01-01"
                }
            };

            var accountResponseAC = new AccountResponseAC()
            {
                Account = new List<AccountDataResponseAC>()
                {
                    new AccountDataResponseAC()
                    {
                        Id = 10193787,
                        AccountName = "xyz",
                        Balance = new AccountBalanceResponseAC()
                        {
                            Amount = 37458,
                            Currency = "inr"
                        },
                        AccountType = "ABCD",
                        AccountInformationJson = @"{'account':[{'CONTAINER': 'bank', 'providerAccountId': 10050198, 'accountName': 'XYZ', 'accountStatus': 'XYZ','isAsset': true, 'id': 10195527,'lastUpdated': '2020 - 08 - 10T04:42:45Z','providerId': 9310,'providerName': 'XYZ Bank','accountType': 'SAVINGS'}]}",
                        ProviderId = 10193785
                    }
                }
            };
            List<ProviderBank> existingProviderBankList = new List<ProviderBank>();

            var providerBanks = new List<ProviderBank>()
            {
                new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "XYZ"
                },
                new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "XYZ"
                }
            };
            var loanApplications = GetLoanApplicationObjects();
            var providerBankACs = _mapper.Map<List<ProviderBank>, List<ProviderBankAC>>(providerBanks);
            var providerBankResponseAC = new ProviderBankResponseAC()
            {
                Id = 10193785,
                Name = "XYZ"
            };
            var yearList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };
            var entityBankAccountsMappingAC = new EntityBankAccountsMappingAC()
            {
                EntityId = commonGuid,
                LoanApplicationId = commonGuid,
                ProviderAccountIds = new List<long>
                {
                    10193785
                },
                IsCleared = false
            };

            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _configurationMock.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value).Returns(year).Returns(month);
            _yodleeUtilityMock.Setup(x => x.GetTransactionJsonAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(transactionDetailResponseAC));
            _yodleeUtilityMock.Setup(x => x.GetAccountResponseAsync(It.IsAny<Guid>(), It.IsAny<long>())).Returns(Task.FromResult(accountResponseAC));
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(existingProviderBankList.AsQueryable().BuildMock().Object).Returns(providerBanks.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()));
            _yodleeUtilityMock.Setup(x => x.GetProviderBankResponseAsync(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(providerBankResponseAC));
            _globalRepositoryMock.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(yearList);
            _configurationMock.Setup(x => x.GetSection("BankPreference:Transaction").Value).Returns("Yodlee");

            // Act
            var result = await _transactionRepository.AddOrUpdateTransactionsAsync(userExpected, entityBankAccountsMappingAC);

            // Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<ProviderBank>(It.IsAny<List<ProviderBank>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<BankAccountTransaction>(It.IsAny<List<BankAccountTransaction>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
            Assert.Equal(result.ProviderBanks.Count, providerBankACs.Count);
        }

        /// <summary>
        /// Check how many times add method call for ProviderBank and bankAccountTransaction when there is provider bank list but no bank account exists.
        /// Check the count of new Provider bank list.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateTransactionAsync_PerformSaveOperation_ProviderBankListExists_NoBankAccountTransactionExists_VerifyOperationCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };
            var year = "2";
            var month = "January";
            var entityBankAccountsMappingAC = new EntityBankAccountsMappingAC()
            {
                EntityId = commonGuid,
                LoanApplicationId = commonGuid,
                ProviderAccountIds = new List<long>
                {
                    10193785
                },
                IsCleared = false
            };
            /*var transactionJsonAC = new TransactionJsonAC()
            {
                TransactionJson = @"{'transaction':[{'id':'98394753','date':'2020-03-12','type': 'PURCHASE', 'amount': {'amount':'360', 'currency':'inr'}, 'status': 'PENDING', 'subType': 'PURCHASE', 'baseType': 'DEBIT', 'category': 'Services/Supplies', 'isManual': 'false', 'description':{'simple': 'XYZ'}}]}"
            };*/
            var transactionDetailResponseAC = new List<TransactionDetailResponseAC>()
            {
                new TransactionDetailResponseAC()
                {
                    AccountId = 10193785,
                    Amount = new Utils.ApplicationClass.Yodlee.TransactionAmountAC()
                    {
                        Amount = 2000,
                        Currency = "USD"
                    },
                    Id = 1234567,
                    TransactionDate = "2020-01-01"
                }
            };

            var accountResponseAC = new AccountResponseAC()
            {
                Account = new List<AccountDataResponseAC>()
                {
                    new AccountDataResponseAC()
                    {
                        Id = 10193785,
                        AccountName = "xyz",
                        Balance = new AccountBalanceResponseAC()
                        {
                            Amount = 37458,
                            Currency = "inr"
                        },
                        AccountType = "ABCD",
                        ProviderId = 10193785,
                        AccountInformationJson = @"{'account':[{'CONTAINER': 'bank', 'providerAccountId': 10050198, 'accountName': 'XYZ', 'accountStatus': 'XYZ','isAsset': true, 'id': 10195527,'lastUpdated': '2020 - 08 - 10T04:42:45Z','providerId': 9310,'providerName': 'XYZ Bank','accountType': 'SAVINGS'}]}"
                    }
                }
            };
            var existingProviderBankList = new List<ProviderBank>()
            {
                 new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "XYZ"
                },
                new ProviderBank
                {
                    BankId = "10193786",
                    BankName = "XYZ"
                }
            };
            var yearList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };
            var existingBankAccountTransaction = new List<BankAccountTransaction>();
            var providerBankACs = _mapper.Map<List<ProviderBank>, List<ProviderBankAC>>(existingProviderBankList);

            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _configurationMock.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value).Returns(year).Returns(month);
            _yodleeUtilityMock.Setup(x => x.GetTransactionJsonAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(transactionDetailResponseAC));
            _yodleeUtilityMock.Setup(x => x.GetAccountResponseAsync(It.IsAny<Guid>(), It.IsAny<long>())).Returns(Task.FromResult(accountResponseAC));
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(existingProviderBankList.AsQueryable().BuildMock().Object).Returns(existingProviderBankList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<BankAccountTransaction, bool>>>())).Returns(existingBankAccountTransaction.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(yearList);
            _configurationMock.Setup(x => x.GetSection("BankPreference:Transaction").Value).Returns("Yodlee");

            // Act
            var result = await _transactionRepository.AddOrUpdateTransactionsAsync(userExpected, entityBankAccountsMappingAC);

            // Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<BankAccountTransaction>(It.IsAny<List<BankAccountTransaction>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
            Assert.Equal(result.ProviderBanks.Count, providerBankACs.Count);
        }

        /// <summary>
        /// Check how many times update method call for ProviderBank and bankAccountTransaction when there is provider bank and bank account transaction exists.
        /// Check the count of new Provider bank list.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateTransactionAsync_PerformUpdateOperation_ProviderBankExists_BankAccountTransactionExists_VerifyOperationCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };
            var year = "2";
            var month = "January";
            var yearList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };
            /*var transactionJsonAC = new TransactionJsonAC()
            {
                TransactionJson = @"{'transaction':[{'id':'98394753','date':'2020-03-12','type': 'PURCHASE', 'amount': {'amount':'360', 'currency':'inr'}, 'status': 'PENDING', 'subType': 'PURCHASE', 'baseType': 'DEBIT', 'category': 'Services/Supplies', 'isManual': 'false', 'description':{'simple': 'XYZ'}}]}"
            };*/
            var transactionDetailResponseAC = new List<TransactionDetailResponseAC>()
            {
                new TransactionDetailResponseAC()
                {
                    AccountId = 10193785,
                    Amount = new Utils.ApplicationClass.Yodlee.TransactionAmountAC()
                    {
                        Amount = 2000,
                        Currency = "USD"
                    },
                    Id = 1234567,
                    TransactionDate = "2020-01-01"
                }
            };

            var accountResponseAC = new AccountResponseAC()
            {
                Account = new List<AccountDataResponseAC>()
                {
                    new AccountDataResponseAC()
                    {
                        Id = 10193785,
                        AccountName = "xyz",
                        Balance = new AccountBalanceResponseAC()
                        {
                            Amount = 37458,
                            Currency = "inr"
                        },
                        AccountType = "ABCD",
                        ProviderId = 10193787,
                        AccountInformationJson = @"{'account':[{'CONTAINER': 'bank', 'providerAccountId': 10050198, 'accountName': 'XYZ', 'accountStatus': 'XYZ','isAsset': true, 'id': 10195527,'lastUpdated': '2020 - 08 - 10T04:42:45Z','providerId': 9310,'providerName': 'XYZ Bank','accountType': 'SAVINGS'}]}"
                    }
                }
            };
            var entityBankAccountsMappingAC = new EntityBankAccountsMappingAC()
            {
                EntityId = commonGuid,
                LoanApplicationId = commonGuid,
                ProviderAccountIds = new List<long>
                {
                    10193785
                },
                IsCleared = false
            };
            var existingProviderBankList = new List<ProviderBank>()
            {
                 new ProviderBank
                {
                    BankId = "10193787",
                    BankName = "XYZ"
                },
                new ProviderBank
                {
                    BankId = "10193786",
                    BankName = "XYZ"
                }
            };
            var existingBankAccountTransaction = new List<BankAccountTransaction>()
            {
                new BankAccountTransaction()
                {
                    AccountId = "10193785",
                    AccountName = "XYZ",
                    CurrentBalance = 1234567,
                    AccountType = "ABC",
                    AccountInformationJson = accountResponseAC.Account[0].AccountInformationJson
                }
            };
            var providerBankACs = _mapper.Map<List<ProviderBank>, List<ProviderBankAC>>(existingProviderBankList);

            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _configurationMock.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value).Returns(year).Returns(month);
            _yodleeUtilityMock.Setup(x => x.GetTransactionJsonAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(transactionDetailResponseAC));
            _yodleeUtilityMock.Setup(x => x.GetAccountResponseAsync(It.IsAny<Guid>(), It.IsAny<long>())).Returns(Task.FromResult(accountResponseAC));
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(existingProviderBankList.AsQueryable().BuildMock().Object).Returns(existingProviderBankList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<BankAccountTransaction, bool>>>())).Returns(existingBankAccountTransaction.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(yearList);
            _configurationMock.Setup(x => x.GetSection("BankPreference:Transaction").Value).Returns("Yodlee");

            // Act
            var result = await _transactionRepository.AddOrUpdateTransactionsAsync(userExpected, entityBankAccountsMappingAC);

            // Assert
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<BankAccountTransaction>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
            Assert.Equal(result.ProviderBanks.Count, providerBankACs.Count);
        }

        /// <summary>
        /// Check how many times add method call for ProviderBank and bankAccountTransaction when there is provider bank list but no bank account exists and when user want to clear previous filled data.Check the count of new Provider bank list.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateTransactionAsync_PerformSaveOperation_ProviderBankListExists_NoBankAccountTransactionExists_IsClearedTrue_VerifyOperationCount()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var userExpected = new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };
            var year = "2";
            var month = "January";
            /*var transactionJsonAC = new TransactionJsonAC()
            {
                TransactionJson = @"{'transaction':[{'id':'98394753','date':'2020-03-12','type': 'PURCHASE', 'amount': {'amount':'360', 'currency':'inr'}, 'status': 'PENDING', 'subType': 'PURCHASE', 'baseType': 'DEBIT', 'category': 'Services/Supplies', 'isManual': 'false', 'description':{'simple': 'XYZ'}}]}"
            };*/
            var transactionDetailResponseAC = new List<TransactionDetailResponseAC>()
            {
                new TransactionDetailResponseAC()
                {
                    AccountId = 10193787,
                    Amount = new Utils.ApplicationClass.Yodlee.TransactionAmountAC()
                    {
                        Amount = 2000,
                        Currency = "USD"
                    },
                    Id = 1234567,
                    TransactionDate = "2020-01-01"
                }
            };

            var accountResponseAC = new AccountResponseAC()
            {
                Account = new List<AccountDataResponseAC>
                {
                    new AccountDataResponseAC()
                    {
                        Id = 10193787,
                        AccountName = "xyz",
                        Balance = new AccountBalanceResponseAC()
                        {
                            Amount = 37458,
                            Currency = "inr"
                        },
                        AccountType = "ABCD",
                        AccountInformationJson = @"{'account':[{'CONTAINER': 'bank', 'providerAccountId': 10050198, 'accountName': 'XYZ', 'accountStatus': 'XYZ','isAsset': true, 'id': 10195527,'lastUpdated': '2020 - 08 - 10T04:42:45Z','providerId': 9310,'providerName': 'XYZ Bank','accountType': 'SAVINGS'}]}",
                        ProviderId = 10193785
                    }
                }
            };

            var existingProviderBankList = new List<ProviderBank>()
            {
                new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "XYZ"
                },
                new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "XYZ"
                }
            };
            var loanApplications = GetLoanApplicationObjects();
            var providerBankACs = _mapper.Map<List<ProviderBank>, List<ProviderBankAC>>(existingProviderBankList);
            var providerBankResponseAC = new ProviderBankResponseAC()
            {
                Id = 10193785,
                Name = "XYZ"
            };
            var yearList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };
            var entityBankAccountsMappingAC = new EntityBankAccountsMappingAC()
            {
                EntityId = commonGuid,
                LoanApplicationId = commonGuid,
                ProviderAccountIds = new List<long>
                {
                    10193785
                },
                IsCleared = true
            };

            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _configurationMock.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value).Returns(year).Returns(month);
            _yodleeUtilityMock.Setup(x => x.GetTransactionJsonAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(transactionDetailResponseAC));
            _yodleeUtilityMock.Setup(x => x.GetAccountResponseAsync(It.IsAny<Guid>(), It.IsAny<long>())).Returns(Task.FromResult(accountResponseAC));
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(existingProviderBankList.AsQueryable().BuildMock().Object).Returns(existingProviderBankList.AsQueryable().BuildMock().Object).Returns(existingProviderBankList.AsQueryable().BuildMock().Object);
            _yodleeUtilityMock.Setup(x => x.GetProviderBankResponseAsync(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(providerBankResponseAC));
            _globalRepositoryMock.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(yearList);
            _configurationMock.Setup(x => x.GetSection("BankPreference:Transaction").Value).Returns("Yodlee");

            // Act
            var result = await _transactionRepository.AddOrUpdateTransactionsAsync(userExpected, entityBankAccountsMappingAC);

            // Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<ProviderBank>(It.IsAny<List<ProviderBank>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<BankAccountTransaction>(It.IsAny<List<BankAccountTransaction>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(3));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
            Assert.Equal(result.ProviderBanks.Count, providerBankACs.Count);
        }

        #endregion

        #region GetPlaidBankTransactionsAsync
        /// <summary>
        /// Check valid plaid bank transaction details.
        /// Case : Fetch the user bank transaction from plaid and add new entry in the database. 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPlaidBankTransactionsAsync_FetchTransacionFromPlaid_AssertCheckToAddDBCall()
        {
            // Arrange
            var providerBanks = new List<ProviderBank>();
            Guid entityId = Guid.NewGuid();
            Guid loanApplicationId = Guid.NewGuid();
            string publicToken = "public-toknen-1233";

            GetTransactionsResponseAC getTransactionsResponse = new GetTransactionsResponseAC()
            {
                Transactions = new List<plaidEntity.PlaidTransactionAC>{
                    new plaidEntity.PlaidTransactionAC(){ AccountId = "1", Name = "Saving" }
                },
                Accounts = new List<plaidEntity.AccountAC> { new plaidEntity.AccountAC() { AccountId = "1", Balances = new plaidEntity.BalanceAC() } },
                Item = new plaidEntity.ItemAC() { InstitutionId = "ins0001" }
            };
            InstitutionAC institution = new InstitutionAC();
            _plaidUtilityMock.Setup(x => x.GetTransactionInfoAsync(It.IsAny<string>())).ReturnsAsync(getTransactionsResponse);
            _plaidUtilityMock.Setup(x => x.GetBankDetailsAsync(It.IsAny<string>())).ReturnsAsync(institution);
            _dataRepositoryMock.Setup(x => x.Fetch<ProviderBank>(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(providerBanks.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("BankPreference:Transaction").Value).Returns("Plaid");
            _configurationMock.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("2");

            ProviderBank providerBank = new ProviderBank()
            {
                EntityId = entityId,
                LoanApplicationId = loanApplicationId,
                TransactionInformationFrom = TransactionInformationFrom.Plaid,
                CreatedByUserId = Guid.NewGuid()
            };
            List<ProviderBank> providerBanksList = new List<ProviderBank>() { providerBank };
            var providerBankACs = _mapper.Map<List<ProviderBankAC>>(providerBanksList);

            // Act
            var result = await _transactionRepository.GetPlaidBankTransactionsAsync(providerBank, publicToken, false);

            // Assert        
            _dataRepositoryMock.Verify(x => x.AddAsync<ProviderBank>(It.IsAny<ProviderBank>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.AddRangeAsync<BankAccountTransaction>(It.IsAny<List<BankAccountTransaction>>()), Times.Exactly(1));
            Assert.Single(result.ProviderBanks);

        }
        /// <summary>
        /// Check valid plaid bank transaction details.
        /// Case : Fetch the user bank transaction from plaid and update bank details is the existing one in the database. 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPlaidBankTransactionsAsync_FetchTransacionFromPlaid_AssertToUpdateDBCall()
        {
            // Arrange
            var providerBanks = GetProviderBanks();
            Guid entityId = providerBanks[0].EntityId;
            Guid loanApplicationId = providerBanks[0].LoanApplicationId;
            string publicToken = "public-toknen-1233";

            GetTransactionsResponseAC getTransactionsResponse = new GetTransactionsResponseAC()
            {
                Transactions = new List<plaidEntity.PlaidTransactionAC>{
                    new plaidEntity.PlaidTransactionAC(){ AccountId = "1", Name = "Saving" }
                },
                Accounts = new List<plaidEntity.AccountAC> { new plaidEntity.AccountAC() { AccountId = providerBanks[0].BankAccountTransactions[0].AccountId, Balances = new plaidEntity.BalanceAC() } },
                Item = new plaidEntity.ItemAC() { InstitutionId = "ins0001" }
            };
            InstitutionAC institution = new InstitutionAC() { InstitutionId = providerBanks[0].BankId };
            _plaidUtilityMock.Setup(x => x.GetTransactionInfoAsync(It.IsAny<string>())).ReturnsAsync(getTransactionsResponse);
            _plaidUtilityMock.Setup(x => x.GetBankDetailsAsync(It.IsAny<string>())).ReturnsAsync(institution);
            _dataRepositoryMock.Setup(x => x.Fetch<ProviderBank>(It.IsAny<Expression<Func<ProviderBank, bool>>>())).Returns(providerBanks.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("BankPreference:Transaction").Value).Returns("Plaid");
            _configurationMock.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("2");

            ProviderBank providerBank = new ProviderBank()
            {
                EntityId = entityId,
                LoanApplicationId = loanApplicationId,
                TransactionInformationFrom = TransactionInformationFrom.Plaid,
                CreatedByUserId = Guid.NewGuid()
            };
            List<ProviderBank> providerBanksList = new List<ProviderBank>() { providerBank };
            var providerBankACs = _mapper.Map<List<ProviderBankAC>>(providerBanksList);

            // Act
            var result = await _transactionRepository.GetPlaidBankTransactionsAsync(providerBank, publicToken, false);

            // Assert        
            _dataRepositoryMock.Verify(x => x.Update<ProviderBank>(It.IsAny<ProviderBank>()), Times.Exactly(1));
            _dataRepositoryMock.Verify(x => x.UpdateRange<BankAccountTransaction>(It.IsAny<List<BankAccountTransaction>>()), Times.Exactly(1));
            Assert.Equal(result.ProviderBanks.Count, providerBanks.Count);
        }
        #endregion
        #region Private

        /// <summary>
        /// Get list of provider banks.
        /// </summary>
        /// <returns>List<ProviderBank></returns>
        private List<ProviderBank> GetProviderBanks()
        {
            return new List<ProviderBank>() {

                new ProviderBank
                {
                    BankId = "10193785",
                    BankName = "SBI",
                    LoanApplicationId = Guid.NewGuid(),
                    EntityId =Guid.NewGuid(),
                    BankAccountTransactions = new List<BankAccountTransaction>(){
                        new BankAccountTransaction()
                        {
                            Id = Guid.NewGuid(),
                            AccountId = "145632987"
                        }
                    }
                },
                new ProviderBank
                {
                    BankId = "15632412",
                    BankName = "BOB",
                    LoanApplicationId = Guid.NewGuid(),
                    EntityId =Guid.NewGuid(),
                    BankAccountTransactions = new List<BankAccountTransaction>(){
                        new BankAccountTransaction()
                        {
                            Id = Guid.NewGuid()
                        }
                    }
                }
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
                SectionName = "Transactions",
                Status = LoanApplicationStatusType.Draft
            };
        }

        #endregion
    }
}
