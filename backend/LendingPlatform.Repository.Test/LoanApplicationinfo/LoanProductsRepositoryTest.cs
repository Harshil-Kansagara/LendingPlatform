using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass.LoanApplicationInfo;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Repository.Repository.LoanApplicationInfo;
using LendingPlatform.Utils.Constants;
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
    public class LoanProductsRepositoryTest : BaseTest
    {
        #region Private Variables
        private readonly ILoanProductsRepository _loanProductsRepository;
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IGlobalRepository> _globalRepository;
        #endregion

        #region Constructor
        public LoanProductsRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _loanProductsRepository = bootstrap.ServiceProvider.GetService<ILoanProductsRepository>();
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _configurationMock = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _globalRepository = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _configurationMock.Reset();
            _dataRepositoryMock.Reset();
        }
        #endregion

        #region Public Methods

        [Fact]
        public async Task GetRecommendedLoanProductsListAsync_LoanApplicationNotExist_ThrowsError()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            var userExpected = GetCurrentUser();
            var loanApplications = new List<LoanApplication>();

            _globalRepository.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanProductsRepository.GetRecommendedLoanProductsListAsync(loanApplicationId, userExpected));
        }

        [Fact]
        public async Task GetRecommendedLoanProductsListAsync_LoanPurposeNotExist_ThrowsError()
        {
            // Arrange
            var commonGuid = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>()
            {
                new LoanApplication()
                {
                    Id = commonGuid
                }
            };
            var emptyloanPurposeProductMappingList = new List<LoanProductPurposeMapping>();
            var dollarSymbol = "$";
            var userExpected = GetCurrentUser();

            _globalRepository.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("Currency").GetSection("Symbol").Value).Returns(dollarSymbol);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanProductPurposeMapping>(It.IsAny<Expression<Func<LoanProductPurposeMapping, bool>>>())).Returns(emptyloanPurposeProductMappingList.AsQueryable().BuildMock().Object);

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanProductsRepository.GetRecommendedLoanProductsListAsync(commonGuid, userExpected));
        }

        [Fact]
        public async Task GetRecommendedLoanProductsListAsync_RecommendedLoanProductNotExist_ThrowsError()
        {
            // Arrange
            var commonGuidValue = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>()
            {
                new LoanApplication()
                {
                    Id = commonGuidValue
                }
            };
            var loanPurposeProductMappingList = new List<LoanProductPurposeMapping>() {
                new LoanProductPurposeMapping
                {
                    Id = commonGuidValue
                }
            };
            var dollarSymbol = "$";
            var emptyLoanProductRangeTypeMappingList = new List<LoanProductRangeTypeMapping>();
            var userExpected = GetCurrentUser();

            _globalRepository.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("Currency").GetSection("Symbol").Value).Returns(dollarSymbol);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanProductPurposeMapping>(It.IsAny<Expression<Func<LoanProductPurposeMapping, bool>>>())).Returns(loanPurposeProductMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<LoanProductRangeTypeMapping>()).Returns(emptyLoanProductRangeTypeMappingList.AsQueryable().BuildMock().Object);

            // Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanProductsRepository.GetRecommendedLoanProductsListAsync(commonGuidValue, userExpected));
        }

        [Fact]
        public async Task GetRecommendedLoanProductsListAsync_RecommendedLoanProduct_VerifyCount()
        {
            // Arrange
            var commonGuidValue = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>()
            {
                new LoanApplication()
                {
                    Id = commonGuidValue,
                    LoanAmount = 200000,
                    LoanPeriod = 48
                }
            };
            var loanPurposeProductMappingList = new List<LoanProductPurposeMapping>() {
                new LoanProductPurposeMapping
                {
                    Id = commonGuidValue
                }
            };
            var dollarSymbol = "$";
            var loanProductRangeTypeMapping = GetLoanProductRangeTypeMappings(commonGuidValue);
            var userExpected = GetCurrentUser();
            var loanProductFeeTypeMapping = GetLoanProductFeeTypeMappings(commonGuidValue);
            var actualLoanProductList = GetLoanProductListAC(commonGuidValue);
            _globalRepository.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _configurationMock.Setup(x => x.GetSection("Currency").GetSection("Symbol").Value).Returns(dollarSymbol);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanProductPurposeMapping>(It.IsAny<Expression<Func<LoanProductPurposeMapping, bool>>>())).Returns(loanPurposeProductMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<LoanProductRangeTypeMapping>()).Returns(loanProductRangeTypeMapping.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanProductFeeTypeMapping>(It.IsAny<Expression<Func<LoanProductFeeTypeMapping, bool>>>())).Returns(loanProductFeeTypeMapping.AsQueryable().BuildMock().Object);

            // Act
            var result = await _loanProductsRepository.GetRecommendedLoanProductsListAsync(commonGuidValue, userExpected);

            // Assert
            Assert.Equal(result.LoanProductList.Count, actualLoanProductList.LoanProductList.Count);
        }

        [Fact]
        public async Task GetLoanProductDataAsync_LoanApplicationProductNotFound_ThrowsError()
        {
            // Arrange
            var loanApplicationId = Guid.NewGuid();
            var dollarSymbol = "$";
            var emptyLoanApplicationProductMapping = new List<LoanApplicationProductMapping>();
            var userExpected = GetCurrentUser();
            _globalRepository.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _configurationMock.Setup(x => x.GetSection("Currency").GetSection("Symbol").Value).Returns(dollarSymbol);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplicationProductMapping>(It.IsAny<Expression<Func<LoanApplicationProductMapping, bool>>>())).Returns(emptyLoanApplicationProductMapping.AsQueryable().BuildMock().Object);

            // Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanProductsRepository.GetLoanProductDataAsync(loanApplicationId, userExpected));
        }

        [Fact]
        public async Task GetLoanProductDataAsync_LoanProductFeeType_VerifyCount()
        {
            var commonGuidValue = Guid.NewGuid();
            var dollarSymbol = "$";
            var loanApplicationProductMapping = new List<LoanApplicationProductMapping>()
            {
                new LoanApplicationProductMapping()
                {
                    Id = commonGuidValue,
                    LoanApplicationId = commonGuidValue,
                    LoanProductId = commonGuidValue,
                    LoanProduct = new LoanProduct()
                    {
                        Id = commonGuidValue,
                        Name = "Xyz",
                        InterestRateType = DomainModel.Enums.LoanProductInterestRateType.FixedRate,
                        FixedRate = (decimal)2.3
                    },
                    LoanApplication = new LoanApplication()
                    {
                        Id = commonGuidValue,
                        LoanAmount = 200000,
                        LoanPeriod = 48
                    }
                }
            };
            var loanProductRangeTypeMapping = GetLoanProductRangeTypeMappings(commonGuidValue);
            var userExpected = GetCurrentUser();
            var loanProductFeeTypeMapping = GetLoanProductFeeTypeMappings(commonGuidValue);
            _globalRepository.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _configurationMock.Setup(x => x.GetSection("Currency").GetSection("Symbol").Value).Returns(dollarSymbol);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplicationProductMapping>(It.IsAny<Expression<Func<LoanApplicationProductMapping, bool>>>())).Returns(loanApplicationProductMapping.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanProductRangeTypeMapping>(It.IsAny<Expression<Func<LoanProductRangeTypeMapping, bool>>>())).Returns(loanProductRangeTypeMapping.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanProductFeeTypeMapping>(It.IsAny<Expression<Func<LoanProductFeeTypeMapping, bool>>>())).Returns(loanProductFeeTypeMapping.AsQueryable().BuildMock().Object);

            // Act
            var result = await _loanProductsRepository.GetLoanProductDataAsync(commonGuidValue, userExpected);

            // Assert
            Assert.Equal(loanProductFeeTypeMapping.Count, result.LoanFeeTypeListAC.LoanFeeTypeDetailACs.Count);
            Assert.Equal(loanApplicationProductMapping[0].Id, result.Id);
        }

        [Fact]
        public async Task AddUpdateLoanProductDataAsync_LoanApplicaionAndLoanProductNotExist_ThrowsError()
        {
            // Arrange
            var userExpected = GetCurrentUser();
            var loanApplicationProductMappingAC = new LoanApplicationProductMappingAC();

            _globalRepository.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()));
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<LoanProduct>(It.IsAny<Expression<Func<LoanProduct, bool>>>()));

            // Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _loanProductsRepository.AddUpdateLoanProductDataAsync(loanApplicationProductMappingAC, userExpected));
        }

        [Fact]
        public async Task AddUpdateLoanProductDataAsync_AddLoanProduct_VerifyCount()
        {
            // Arrange
            var commonGuidValue = Guid.NewGuid();
            var userExpected = GetCurrentUser();
            var loanApplicationProductMappingAC = new LoanApplicationProductMappingAC()
            {
                LoanApplicationId = commonGuidValue,
                LoanProductId = commonGuidValue
            };
            var loanApplication = new LoanApplication()
            {
                Id = commonGuidValue
            };
            var loanProduct = new LoanProduct()
            {
                Id = commonGuidValue
            };

            _globalRepository.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<LoanProduct>(It.IsAny<Expression<Func<LoanProduct, bool>>>())).Returns(Task.FromResult(loanProduct));
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<LoanApplicationProductMapping>(It.IsAny<Expression<Func<LoanApplicationProductMapping, bool>>>()));
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _globalRepository.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()));
            
            // Act
            await _loanProductsRepository.AddUpdateLoanProductDataAsync(loanApplicationProductMappingAC, userExpected);

            // Assert
            _dataRepositoryMock.Verify(x => x.AddAsync<LoanApplicationProductMapping>(It.IsAny<LoanApplicationProductMapping>()),Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        [Fact]
        public async Task AddUpdateLoanProductDataAsync_UpdateLoanProduct_VerifyCount()
        {
            // Arrange
            var commonGuidValue = Guid.NewGuid();
            var userExpected = GetCurrentUser();
            var loanApplicationProductMappingAC = new LoanApplicationProductMappingAC()
            {
                Id = commonGuidValue,
                LoanApplicationId = commonGuidValue,
                LoanProductId = commonGuidValue
            };
            var loanApplicationProductMapping = new LoanApplicationProductMapping()
            {
                Id = commonGuidValue
            };
            var loanApplication = new LoanApplication()
            {
                Id = commonGuidValue
            };
            var loanProduct = new LoanProduct()
            {
                Id = commonGuidValue
            };

            _globalRepository.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<User>(), It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(true);
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<LoanProduct>(It.IsAny<Expression<Func<LoanProduct, bool>>>())).Returns(Task.FromResult(loanProduct));
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<LoanApplicationProductMapping>(It.IsAny<Expression<Func<LoanApplicationProductMapping, bool>>>())).Returns(Task.FromResult(loanApplicationProductMapping));
            _globalRepository.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()));

            // Act
            await _loanProductsRepository.AddUpdateLoanProductDataAsync(loanApplicationProductMappingAC, userExpected);

            // Assert
            _dataRepositoryMock.Verify(x => x.Update<LoanApplicationProductMapping>(It.IsAny<LoanApplicationProductMapping>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

        }

        #endregion

        #region Private Method
        private List<LoanProductRangeTypeMapping> GetLoanProductRangeTypeMappings(Guid commonGuidValue)
        {
            return new List<LoanProductRangeTypeMapping>()
            {
                new LoanProductRangeTypeMapping
                {
                    Id = commonGuidValue,
                    LoanProduct = new LoanProduct()
                    {
                        Id = commonGuidValue,
                        Name = "Xyz",
                        InterestRateType = DomainModel.Enums.LoanProductInterestRateType.FixedRate,
                        ProductStartDate = Convert.ToDateTime("2/1/2020"),
                        ProductEndDate = Convert.ToDateTime("2/1/2022"),
                        FixedRate = (decimal)2.3
                    },
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = commonGuidValue,
                        Name = StringConstant.LoanAmount
                    },
                    Minimum = 100000,
                    Maximum = 500000

                },
                new LoanProductRangeTypeMapping
                {
                    Id = commonGuidValue,
                    LoanProduct = new LoanProduct()
                    {
                        Id = commonGuidValue,
                        Name = "Xyz",
                        InterestRateType = DomainModel.Enums.LoanProductInterestRateType.FixedRate,
                        FixedRate = (decimal)2.3
                    },
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = commonGuidValue,
                        Name = StringConstant.Lifecycle
                    },
                    Minimum = 24,
                    Maximum = 48

                },
                new LoanProductRangeTypeMapping
                {
                    Id = commonGuidValue,
                    LoanProduct = new LoanProduct()
                    {
                        Id = commonGuidValue,
                        Name = "Xyz",
                        InterestRateType = DomainModel.Enums.LoanProductInterestRateType.FixedRate,
                        FixedRate = (decimal)2.3
                    },
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = commonGuidValue,
                        Name = StringConstant.FixedRatePeriod
                    },
                    Minimum = 12,
                    Maximum = 20

                },
                new LoanProductRangeTypeMapping
                {
                    Id = commonGuidValue,
                    LoanProduct = new LoanProduct()
                    {
                        Id = commonGuidValue,
                        Name = "Xyz",
                        InterestRateType = DomainModel.Enums.LoanProductInterestRateType.FixedRate,
                        FixedRate = (decimal)2.3
                    },
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = commonGuidValue,
                        Name = StringConstant.InterestOnlyPeriod
                    },
                    Minimum = 12,
                    Maximum = 20

                }
            };
        }

        private User GetCurrentUser()
        {
            return new User()
            {
                Email = "karan@lendingplatform.com",
                Id = Guid.NewGuid(),
                Name = "Karan Desai",
                Phone = "9999999999",
                SSN = "920-89-1234"
            };
        }

        private List<LoanProductFeeTypeMapping> GetLoanProductFeeTypeMappings(Guid commonGuidValue)
        {
            return new List<LoanProductFeeTypeMapping>()
            {
                new LoanProductFeeTypeMapping
                {
                    Id = commonGuidValue,
                    LoanFeeTypeId = commonGuidValue,
                    LoanProductId = commonGuidValue,
                    LoanFeeType = new LoanFeeType()
                    {
                        Id = commonGuidValue,
                        Name = "Xyz",
                        Amount = 100,
                        Description = "XYZ XYZ",
                        FrequencyType = DomainModel.Enums.LoanFeeFrequencyType.OneTime
                    }
                }
            };
        }

        private LoanProductListAC GetLoanProductListAC(Guid commonGuidValue)
        {
            return new LoanProductListAC()
            {
                LoanProductList = new List<LoanProductAC>()
                {
                    new LoanProductAC
                    {
                        Id = commonGuidValue
                    }
                }
            };
        }

        #endregion

    }
}
