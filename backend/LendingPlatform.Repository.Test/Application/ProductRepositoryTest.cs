using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Products;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.Application;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass.Product;
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

namespace LendingPlatform.Repository.Test.Application
{
    [Collection("Register Dependency")]
    public class ProductRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly IProductRepository _productRepository;
        private readonly CurrentUserAC _currentUserAC;
        private readonly Mock<IRulesUtility> _rulesUtilityMock;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public ProductRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _configurationMock = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _productRepository = _scope.ServiceProvider.GetService<IProductRepository>();
            _rulesUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IRulesUtility>>();
            _mapper = bootstrap.ServiceProvider.GetService<IMapper>();
            _dataRepositoryMock.Reset();
            _rulesUtilityMock.Reset();
            _currentUserAC = FetchLoggedInUserAC();
        }
        #endregion

        #region Private Method
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

        #endregion

        #region Public Method

        /// <summary>
        /// Method to verify that it throws an error if user is unauthorized to access the loan application.
        /// </summary>
        [Fact]
        public async Task SaveLoanProductAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _productRepository.SaveLoanProductAsync(loanApplicationId, productId, _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that it throws error when add or update operation is not allowed.
        /// </summary>
        [Fact]
        public async Task SaveLoanProductAsync_UnauthorizedAddOrUpdateOperation_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            var loanApplication = new LoanApplication()
            {
                Id = Guid.NewGuid()
            };
            var product = new Product()
            {
                Id = Guid.NewGuid()
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).ReturnsAsync(loanApplication);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(product);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _productRepository.SaveLoanProductAsync(loanApplication.Id, product.Id, _currentUserAC));
        }

        /// <summary>
        /// Test method to verify that it throws error when parameter is not found.
        /// </summary>
        [Fact]
        public async Task SaveLoanProductAsync_ThrowsInvalidParameterException()
        {
            //Arrange
            var loanApplication = new LoanApplication();
            var product = new Product();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).ReturnsAsync(loanApplication);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(product);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _productRepository.SaveLoanProductAsync(Guid.NewGuid(), Guid.NewGuid(), _currentUserAC));
        }

        /// <summary>
        /// Test method to save loan product related to loan application.
        /// </summary>
        [Fact]
        public async Task SaveLoanProductAsync_PerformsUpdateOperation()
        {
            //Arrange
            var loanApplication = new LoanApplication()
            {
                Id = Guid.NewGuid()
            };
            var product = new Product()
            {
                Id = Guid.NewGuid()
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).ReturnsAsync(loanApplication);
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(product);
            _globalRepositoryMock.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act
            await _productRepository.SaveLoanProductAsync(loanApplication.Id, product.Id, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<LoanApplication>(It.IsAny<LoanApplication>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method to verify that it throws an error if user is unauthorized to access the loan application.
        /// </summary>
        [Fact]
        public async Task GetSelectedProductDataAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _productRepository.GetSelectedProductDataAsync(loanApplicationId, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that it throws an error if section is not found.
        /// </summary>
        [Fact]
        public async Task GetSelectedProductDataAsync_SectionNotFound_ThrowsInvalidParameterException()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>()
            {
                new LoanApplication(){
                    Id = Guid.NewGuid(),
                    Product = null,
                    CreatedByUserId = _currentUserAC.Id,
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                    {
                        new UserLoanSectionMapping
                        {
                            Id = Guid.NewGuid(),
                            UserId = _currentUserAC.Id
                        }
                    }
                }
            };
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("$");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _productRepository.GetSelectedProductDataAsync(loanApplicationId, _currentUserAC));
        }
        /// <summary>
        /// Method to verify that it throws an error if product is linked with loan application but section is not equal to loan product
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSelectedProductDataAsync_ProductFound_SectionIsNotEqualToLoanProduct_ThrowsInvalidParameterException()
        {
            //Arrange
            var loanApplications = new List<LoanApplication>() { new LoanApplication(){ Id = Guid.NewGuid(), Product = null, CreatedByUserId = _currentUserAC.Id, UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                    {
                        new UserLoanSectionMapping
                        {
                            Id= Guid.NewGuid(),
                            LoanApplicationId = Guid.NewGuid(),
                            UserId = _currentUserAC.Id,
                            SectionId = Guid.NewGuid(),
                            Section = new Section() { Name = StringConstant.FinancesSection }
                        }
                    } } };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("$");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _productRepository.GetSelectedProductDataAsync(loanApplications.First().Id, _currentUserAC));
        }

        /// <summary>
        /// Method to verify that product is linked with loan application and section is equal to loan product and we will get new product
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSelectedProductDataAsync_ProductFound_SectionIsEqualToLoanProduct_AssertNotNull()
        {
            //Arrange
            var loanApplications = new List<LoanApplication>() { new LoanApplication() { Id = Guid.NewGuid(), Product = null, CreatedByUserId = _currentUserAC.Id, UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                    {
                        new UserLoanSectionMapping
                        {
                            Id= Guid.NewGuid(),
                            LoanApplicationId = Guid.NewGuid(),
                            UserId = _currentUserAC.Id,
                            SectionId = Guid.NewGuid(),
                            Section = new Section
                            {
                                Name = StringConstant.LoanProductSection
                            }
                        }
                    } } };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("$");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);

            //Act
            var result = await _productRepository.GetSelectedProductDataAsync(loanApplications.First().Id, _currentUserAC);

            //Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Method to verify that it provides proper calculated value of product detail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSelectedProductDataAsync_AssertEqual()
        {
            //Arrange
            var purposeId = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>()
            {
                new LoanApplication(){
                    Id = Guid.NewGuid(),
                    Product = new Product()
                    {
                        Id = Guid.NewGuid()
                    },
                    LoanPeriod = (decimal)3.5,
                    LoanAmount = 100000,
                    LoanPurposeId = purposeId,
                    Status = DomainModel.Enums.LoanApplicationStatusType.Draft,
                    CreatedByUserId = _currentUserAC.Id,
                    CreatedByUser = new User()
                    {
                        SelfDeclaredCreditScore = "650-700"
                    },
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
                                Name = StringConstant.LoanProductSection
                            }
                        }
                    }
                }
            };

            var productRangeTypeMappings = new List<ProductRangeTypeMapping>()
            {
                new ProductRangeTypeMapping()
                {
                    Maximum = 60,
                    Minimum = 12,
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.Lifecycle
                    },
                    ProductId = loanApplications.First().Product.Id
                },
                new ProductRangeTypeMapping()
                {
                    Maximum = 100000,
                    Minimum = 5000,
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.LoanAmount
                    },
                    ProductId = loanApplications.First().Product.Id
                }
            };

            var loanPurposeRangeTypeMappings = new List<LoanPurposeRangeTypeMapping>(){
                new LoanPurposeRangeTypeMapping()
                {
                    LoanPurposeId = purposeId,
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
                    LoanPurposeId = purposeId,
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

            var productDetails = FetchProductDetails();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("$");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductRangeTypeMapping>()).Returns(productRangeTypeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanPurposeRangeTypeMapping, bool>>>())).Returns(loanPurposeRangeTypeMappings.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CalculateLoanProductDetail(It.IsAny<List<ProductRangeTypeMapping>>(), It.IsAny<string>(), It.IsAny<LoanApplication>())).Returns(productDetails);
            _mapper.Map<RecommendedProductAC>(loanApplications.First().Product);

            //Act
            var result = await _productRepository.GetSelectedProductDataAsync(loanApplications.First().Id, _currentUserAC);

            //Assert
            Assert.Equal((decimal)2648.83, result.ProductDetails.MaxMonthlyPayment);
            Assert.Equal((decimal)463.15, result.ProductDetails.MinMonthlyPayment);
            Assert.Equal((decimal)3329.21, result.ProductDetails.MonthlyPayment);
            Assert.Equal(5, result.ProductDetails.MaxProductTenure);
            Assert.Equal(1, result.ProductDetails.MinProductTenure);
            Assert.Equal("$ 39,826.82", result.ProductDetails.TotalInterest);
            Assert.Equal("$ 139,826.82", result.ProductDetails.TotalPayment);
        }

        /// <summary>
        /// Method to verify that it provides proper calculated value of product detail and checked is previous product matched is coming false
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSelectedProductDataAsync_RecommendedProductListNotNull_AssertEqual()
        {
            //Arrange
            var purposeId = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>() { new LoanApplication(){ Id = Guid.NewGuid(), CreatedByUserId = _currentUserAC.Id, Product = new Product() { Id = Guid.NewGuid() }, LoanPeriod = (decimal)3.5, LoanAmount = 100000, LoanPurposeId = purposeId, Status = DomainModel.Enums.LoanApplicationStatusType.Draft, CreatedByUser = new User() { SelfDeclaredCreditScore = "650-700" },UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                    {
                        new UserLoanSectionMapping
                        {
                            Id= Guid.NewGuid(),
                            LoanApplicationId = Guid.NewGuid(),
                            UserId = _currentUserAC.Id,
                            SectionId = Guid.NewGuid(),
                            Section = new Section
                            {
                                Name = StringConstant.LoanProductSection
                            }
                        }
                    } } };
            var productRangeTypeMappings = new List<ProductRangeTypeMapping>() { new ProductRangeTypeMapping() { Maximum = 60, Minimum = 12, LoanRangeType = new LoanRangeType() { Id = Guid.NewGuid(), Name = StringConstant.Lifecycle }, ProductId = loanApplications.First().Product.Id }, new ProductRangeTypeMapping() { Maximum = 100000, Minimum = 5000, LoanRangeType = new LoanRangeType() { Id = Guid.NewGuid(), Name = StringConstant.LoanAmount }, ProductId = loanApplications.First().Product.Id } };

            var loanPurposeRangeTypeMappings = new List<LoanPurposeRangeTypeMapping>() { new LoanPurposeRangeTypeMapping() { LoanPurposeId = purposeId, LoanRangeType = new LoanRangeType() { Id = Guid.NewGuid(), Name = StringConstant.Lifecycle }, Maximum = 5, Minimum = 1, StepperAmount = (decimal)0.5 }, new LoanPurposeRangeTypeMapping() { LoanPurposeId = purposeId, LoanRangeType = new LoanRangeType() { Id = Guid.NewGuid(), Name = StringConstant.LoanAmount }, Maximum = 100000, Minimum = 5000, StepperAmount = (decimal)1000 } };

            var productDetails = FetchProductDetails();
            var recommendedProducts = new List<RecommendedProductAC>() { new RecommendedProductAC { Id = Guid.NewGuid() } };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns("$");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductRangeTypeMapping>()).Returns(productRangeTypeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanPurposeRangeTypeMapping, bool>>>())).Returns(loanPurposeRangeTypeMappings.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CalculateLoanProductDetail(It.IsAny<List<ProductRangeTypeMapping>>(), It.IsAny<string>(), It.IsAny<LoanApplication>())).Returns(productDetails);
            _mapper.Map<RecommendedProductAC>(loanApplications.First().Product);

            //Act
            var result = await _productRepository.GetSelectedProductDataAsync(loanApplications.First().Id, _currentUserAC, recommendedProducts);

            //Assert
            Assert.False(result.IsPreviousProductMatched);
            Assert.Equal((decimal)2648.83, result.ProductDetails.MaxMonthlyPayment);
            Assert.Equal((decimal)463.15, result.ProductDetails.MinMonthlyPayment);
            Assert.Equal((decimal)3329.21, result.ProductDetails.MonthlyPayment);
            Assert.Equal(5, result.ProductDetails.MaxProductTenure);
            Assert.Equal(1, result.ProductDetails.MinProductTenure);
            Assert.Equal("$ 39,826.82", result.ProductDetails.TotalInterest);
            Assert.Equal("$ 139,826.82", result.ProductDetails.TotalPayment);
        }

        /// <summary>
        /// Method to verify that it throws an error if user is unauthorized to access the loan application.
        /// </summary>
        [Fact]
        public async Task GetRecommendedProductsListAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _productRepository.GetRecommendedProductsListAsync(loanApplicationId, _currentUserAC));
        }

        /// <summary>
        /// Method is to verify that it throws an error if loan application value is empty
        /// </summary>
        [Fact]
        public async Task GetRecommendedProductsListAsync_ExecuteProductRules_LoanApplicationNull_ThrowsInvalidParameterException()
        {
            //Arrange
            var loanApplicationId = Guid.NewGuid();
            User user = new User()
            {
                ResidencyStatus = DomainModel.Enums.ResidencyStatus.USCitizen,
                SelfDeclaredCreditScore = "650-700"
            };
            var entityLoanApplicationMappingList = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    Entity = new DomainModel.Models.EntityInfo.Entity()
                    {
                        Company = new Company()
                        {
                            NAICSIndustryType = new NAICSIndustryType()
                            {
                                IndustryCode = "7134"
                            }
                        },
                        Type = DomainModel.Enums.EntityType.Company
                    }
                }

            };
            var loanApplications = new List<LoanApplication>() {
                new LoanApplication()
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _configurationMock.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value).Returns("650").Returns("7132");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _productRepository.GetRecommendedProductsListAsync(loanApplicationId, _currentUserAC));
        }

        /// <summary>
        /// Method is to verify that it throws an error if productPercentageSuitability list is empty
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetRecommendedProductsListAsync_ProductPercentageSuitabilityListEmpty_ThrowDataNotFoundException()
        {
            //Arrange
            var loanId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            User user = new User()
            {
                ResidencyStatus = DomainModel.Enums.ResidencyStatus.USCitizen,
                SelfDeclaredCreditScore = "650-700"
            };
            var entityLoanApplicationMappingList = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    Entity = new DomainModel.Models.EntityInfo.Entity()
                    {
                        Company = new Company()
                        {
                            NAICSIndustryType = new NAICSIndustryType()
                            {
                                IndustryCode = "7134"
                            }
                        },
                        Type = DomainModel.Enums.EntityType.Company
                    }
                }

            };
            var loanApplications = new List<LoanApplication>()
            {
                new LoanApplication()
                {
                    Id = loanId,
                    LoanAmount = 200000,
                    LoanPeriod = 2,
                    LoanPurpose = new LoanPurpose()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Asset Purchase"
                    }
                }
            };
            var productRangeTypeMappings = new List<ProductRangeTypeMapping>()
            {
                new ProductRangeTypeMapping()
                {
                    Maximum = 400000,
                    Minimum = 100000,
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.Lifecycle
                    },
                    ProductId = productId,
                    Product = new Product()
                    {
                        Id = productId,
                        IsEnabled = true
                    }
                },
                                new ProductRangeTypeMapping()
                {
                    Maximum = 400000,
                    Minimum = 100000,
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.LoanAmount
                    },
                    ProductId = productId,
                    Product = new Product()
                    {
                        Id = productId,
                        IsEnabled = true
                    }
                }
            };

            var productPurposeMappings = new List<ProductSubPurposeMapping>()
            {
                new ProductSubPurposeMapping()
                {
                    SubLoanPurpose = new SubLoanPurpose()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Asset Purchase"
                    },
                    ProductId = productId
                }
            };

            var productPercentageSuitabilityList = new List<ProductPercentageSuitabilityAC>();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _configurationMock.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                                .Returns("650")
                                .Returns("7132")
                                .Returns("https://kiegroup.org/dmn/_203CB433-411C-4A59-AA20-BF4F41E59552")
                                .Returns("demo_dmn")
                                .Returns("http://localhost:8080/kie-server/services/rest/server/containers/Demo/dmn")
                                .Returns("XYZ")
                                .Returns("XYZ");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductRangeTypeMapping>()).Returns(productRangeTypeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductSubPurposeMapping>()).Returns(productPurposeMappings.AsQueryable().BuildMock().Object);
            _rulesUtilityMock.Setup(x => x.ExecuteProductRules(It.IsAny<ProductRuleAC>())).ReturnsAsync(productPercentageSuitabilityList);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _productRepository.GetRecommendedProductsListAsync(loanId, _currentUserAC));
        }

        /// <summary>
        /// Method is to verify that the calculated product detail value which is equal to the expected product detail value
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetRecommendedProductsListAsync_ProductPercentageSuitabilityNotEmpty_IsCallFromSelectedProductTrue_AssertEqualCount()
        {
            //Arrange
            var loanId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var purposeId = Guid.NewGuid();
            User user = new User() { ResidencyStatus = DomainModel.Enums.ResidencyStatus.USCitizen, SelfDeclaredCreditScore = "650-700" };
            var entityLoanApplicationMappingList = new List<EntityLoanApplicationMapping>() { new EntityLoanApplicationMapping() { Entity = new DomainModel.Models.EntityInfo.Entity() { Company = new Company() { NAICSIndustryType = new NAICSIndustryType() { IndustryCode = "7134" } }, Type = DomainModel.Enums.EntityType.Company } } };
            var loanApplications = new List<LoanApplication>() { new LoanApplication() { Id = loanId, LoanAmount = 100000, LoanPeriod = (decimal)3.5, LoanPurpose = new LoanPurpose() { Id = purposeId, Name = "Asset Purchase" }, CreatedByUser = new User() { SelfDeclaredCreditScore = "650-700" } } };
            var productRangeTypeMappings = new List<ProductRangeTypeMapping>() { new ProductRangeTypeMapping() { Maximum = 60, Minimum = 12, LoanRangeType = new LoanRangeType() { Id = Guid.NewGuid(), Name = StringConstant.Lifecycle }, ProductId = productId, Product = new Product() { Id = productId, ProductStartDate = Convert.ToDateTime("2020-03-01 00:00:00"), ProductEndDate = Convert.ToDateTime("2022-03-01 00:00:00"), IsEnabled = true } }, new ProductRangeTypeMapping() { Maximum = 100000, Minimum = 5000, LoanRangeType = new LoanRangeType() { Id = Guid.NewGuid(), Name = StringConstant.LoanAmount }, ProductId = productId, Product = new Product() { Id = productId, ProductStartDate = Convert.ToDateTime("2020-03-01 00:00:00"), ProductEndDate = Convert.ToDateTime("2022-03-01 00:00:00"), IsEnabled = true } } };
            var productPurposeMappings = new List<ProductSubPurposeMapping>() { new ProductSubPurposeMapping() { SubLoanPurpose = new SubLoanPurpose() { Id = purposeId, Name = "Asset Purchase" }, ProductId = productId } };

            var productPercentageSuitabilityList = new List<ProductPercentageSuitabilityAC>() { new ProductPercentageSuitabilityAC() { IsRecommended = true, PercentageSuitability = 100, ProductId = productId } };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _configurationMock.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                                .Returns("$");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductRangeTypeMapping>()).Returns(productRangeTypeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductSubPurposeMapping>()).Returns(productPurposeMappings.AsQueryable().BuildMock().Object);
            _rulesUtilityMock.Setup(x => x.ExecuteProductRules(It.IsAny<ProductRuleAC>())).ReturnsAsync(productPercentageSuitabilityList);
            _mapper.Map<LoanApplication, ProductRuleAC>(loanApplications.First());
            _mapper.Map<List<ProductSubPurposeMapping>, List<ProductSubPurposeMappingAC>>(productPurposeMappings);
            _mapper.Map<List<ProductRangeTypeMapping>, List<ProductRangeTypeMappingAC>>(productRangeTypeMappings);
            _mapper.Map<RecommendedProductAC>(loanApplications.First().Product);

            //Act
            var result = await _productRepository.GetRecommendedProductsListAsync(loanId, _currentUserAC, true);

            //Assert
            Assert.Equal(result.Count, productPercentageSuitabilityList.Count);
        }

        /// <summary>
        /// Method is to verify that the calculated product detail value which is equal to the expected product detail value
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetRecommendedProductsListAsync_ProductPercentageSuitabilityNotEmpty_AssertEqual()
        {
            //Arrange
            var loanId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var purposeId = Guid.NewGuid();
            User user = new User()
            {
                ResidencyStatus = DomainModel.Enums.ResidencyStatus.USCitizen,
                SelfDeclaredCreditScore = "650-700"
            };
            var entityLoanApplicationMappingList = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    Entity = new DomainModel.Models.EntityInfo.Entity()
                    {
                        Company = new Company()
                        {
                            NAICSIndustryType = new NAICSIndustryType()
                            {
                                IndustryCode = "7134"
                            }
                        },
                        Type = DomainModel.Enums.EntityType.Company
                    }
                }

            };
            var loanApplications = new List<LoanApplication>()
            {
                new LoanApplication()
                {
                    Id = loanId,
                    LoanAmount = 100000,
                    LoanPeriod = (decimal)3.5,
                    LoanPurpose = new LoanPurpose()
                    {
                        Id = purposeId,
                        Name = "Asset Purchase"
                    },
                    CreatedByUser = new User()
                    {
                        SelfDeclaredCreditScore = "650-700"
                    }
                }
            };
            var productRangeTypeMappings = new List<ProductRangeTypeMapping>()
            {
                new ProductRangeTypeMapping()
                {
                    Maximum = 60,
                    Minimum = 12,
                    LoanRangeType = new LoanRangeType()
                    {
                        Id = Guid.NewGuid(),
                        Name = StringConstant.Lifecycle
                    },
                    ProductId = productId,
                    Product = new Product()
                    {
                        Id = productId,
                        ProductStartDate = Convert.ToDateTime("2020-03-01 00:00:00"),
                        ProductEndDate = Convert.ToDateTime("2022-03-01 00:00:00"),
                        IsEnabled = true
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
                    },
                    ProductId = productId,
                    Product = new Product()
                    {
                        Id = productId,
                        ProductStartDate = Convert.ToDateTime("2020-03-01 00:00:00"),
                        ProductEndDate = Convert.ToDateTime("2022-03-01 00:00:00"),
                        IsEnabled = true
                    }
                }
            };

            var productPurposeMappings = new List<ProductSubPurposeMapping>()
            {
                new ProductSubPurposeMapping()
                {
                    SubLoanPurpose = new SubLoanPurpose()
                    {
                        Id = purposeId,
                        Name = "Asset Purchase"
                    },
                    ProductId = productId
                }
            };

            var productPercentageSuitabilityList = new List<ProductPercentageSuitabilityAC>() {
                new ProductPercentageSuitabilityAC()
                {
                    IsRecommended = true,
                    PercentageSuitability = 100,
                    ProductId = productId
                }
            };

            var loanPurposeRangeTypeMappings = new List<LoanPurposeRangeTypeMapping>(){
                new LoanPurposeRangeTypeMapping()
                {
                    LoanPurposeId = purposeId,
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
                    LoanPurposeId = purposeId,
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

            var productDetails = FetchProductDetails();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _configurationMock.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                                .Returns("$");
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>())).Returns(entityLoanApplicationMappingList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductRangeTypeMapping>()).Returns(productRangeTypeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<ProductSubPurposeMapping>()).Returns(productPurposeMappings.AsQueryable().BuildMock().Object);
            _rulesUtilityMock.Setup(x => x.ExecuteProductRules(It.IsAny<ProductRuleAC>())).ReturnsAsync(productPercentageSuitabilityList);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanPurposeRangeTypeMapping, bool>>>())).Returns(loanPurposeRangeTypeMappings.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.CalculateLoanProductDetail(It.IsAny<List<ProductRangeTypeMapping>>(), It.IsAny<string>(), It.IsAny<LoanApplication>())).Returns(productDetails);
            _mapper.Map<LoanApplication, ProductRuleAC>(loanApplications.First());
            _mapper.Map<List<ProductSubPurposeMapping>, List<ProductSubPurposeMappingAC>>(productPurposeMappings);
            _mapper.Map<List<ProductRangeTypeMapping>, List<ProductRangeTypeMappingAC>>(productRangeTypeMappings);
            _mapper.Map<RecommendedProductAC>(loanApplications.First().Product);

            //Act
            var result = await _productRepository.GetRecommendedProductsListAsync(loanId, _currentUserAC);

            //Assert
            Assert.Equal((decimal)2648.83, result.First().ProductDetails.MaxMonthlyPayment);
            Assert.Equal((decimal)463.15, result.First().ProductDetails.MinMonthlyPayment);
            Assert.Equal((decimal)3329.21, result.First().ProductDetails.MonthlyPayment);
            Assert.Equal(5, result.First().ProductDetails.MaxProductTenure);
            Assert.Equal(1, result.First().ProductDetails.MinProductTenure);
            Assert.Equal("$ 39,826.82", result.First().ProductDetails.TotalInterest);
            Assert.Equal("$ 139,826.82", result.First().ProductDetails.TotalPayment);
        }

        #endregion
    }
}
