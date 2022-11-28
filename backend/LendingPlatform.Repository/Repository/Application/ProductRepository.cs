using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass.Product;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.Application
{
    public class ProductRepository : IProductRepository
    {
        #region Private Variable
        private readonly IDataRepository _dataRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IGlobalRepository _globalRepository;
        private readonly IRulesUtility _rulesUtility;
        #endregion

        #region Constructor
        public ProductRepository(IDataRepository dataRepository, IGlobalRepository globalRepository,
            IConfiguration configuration, IMapper mapper, IRulesUtility rulesUtility)
        {
            _dataRepository = dataRepository;
            _configuration = configuration;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _rulesUtility = rulesUtility;
        }
        #endregion

        /// <summary>
        /// Method to save the loan product related to loan application
        /// </summary>
        /// <param name="loanApplicationId">Loan application Id</param>
        /// <param name="productId">Product Id</param>
        /// <param name="currentUser"> Current User </param>
        /// <returns></returns>
        public async Task SaveLoanProductAsync(Guid loanApplicationId, Guid productId, CurrentUserAC currentUser)
        {
            if (currentUser != null && loanApplicationId != Guid.Empty && !await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplicationId))
            {
                throw new UnauthorizedAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            if (!await _globalRepository.IsAddOrUpdateAllowedAsync(loanApplicationId))
            {
                throw new InvalidResourceAccessException(StringConstant.UpdateOperationNotAllowed);
            }

            var loanApplication = await _dataRepository.FirstOrDefaultAsync<LoanApplication>(x => x.Id == loanApplicationId);

            var product = await _dataRepository.FirstOrDefaultAsync<Product>(x => x.Id == productId);

            if (loanApplication.Id == Guid.Empty && product.Id == Guid.Empty)
            {
                throw new InvalidParameterException(StringConstant.LoanApplicationOrProductNotFound);
            }

            loanApplication.Product = product;
            loanApplication.UpdatedByUserId = currentUser != null ? currentUser.Id : Guid.Empty;
            loanApplication.UpdatedOn = DateTime.UtcNow;
            using (await _dataRepository.BeginTransactionAsync())
            {
                _dataRepository.Update<LoanApplication>(loanApplication);
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, loanApplication.Id);
                await _dataRepository.SaveChangesAsync(auditLog);
                _dataRepository.CommitTransaction();
            }
        }

        /// <summary>
        /// Method is to get selected product for loan application
        /// </summary>
        /// <param name="loanApplicationId">Loan Application Id</param>
        /// <param name="currentUser">Current User</param>
        /// <param name="recommendedProducts">List of recommended products</param>
        /// <returns>Application detailAC object</returns>
        public async Task<RecommendedProductAC> GetSelectedProductDataAsync(Guid loanApplicationId, CurrentUserAC currentUser, List<RecommendedProductAC> recommendedProducts = null)
        {
            if (currentUser != null && !currentUser.IsBankUser && !await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplicationId))
            {
                throw new UnauthorizedAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            var currentCurrencySymbol = _configuration.GetSection("Currency:Symbol").Value;

            var loanApplication = await _dataRepository.Fetch<LoanApplication>(x => x.Id == loanApplicationId)
                .Include(x => x.Product).Include(i => i.UserLoanSectionMappings).ThenInclude(s => s.Section)
                .Include(x => x.CreatedByUser).SingleOrDefaultAsync();

            if (loanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(loanApplication.CreatedByUserId)).Section == null || string.IsNullOrEmpty(loanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(loanApplication.CreatedByUserId)).Section.Name))
            {
                throw new InvalidParameterException(StringConstant.SectionNameIsNull);
            }

            if (loanApplication.Product == null)
            {
                if (!loanApplication.UserLoanSectionMappings.Single(s => s.UserId.Equals(loanApplication.CreatedByUserId)).Section.Name.Equals(StringConstant.LoanProductSection))
                {
                    throw new InvalidParameterException(StringConstant.LoanApplicationProductNotFound);
                }
                else
                {
                    return new RecommendedProductAC();
                }
            }

            var productRangeTypeMappingList = await _dataRepository.GetAll<ProductRangeTypeMapping>()
                                                    .Include(x => x.LoanRangeType).ToListAsync();

            var loanPurposeRangeTypeMappings = await _dataRepository.Fetch<LoanPurposeRangeTypeMapping>(x => x.LoanPurposeId == loanApplication.LoanPurposeId).Include(x => x.LoanRangeType).ToListAsync();

            // Loan Product
            var recommendedProduct = _mapper.Map<Product, RecommendedProductAC>(loanApplication.Product);
            recommendedProduct.IsProductRecommended = true;

            if (recommendedProducts != null && recommendedProducts.Any() && !recommendedProducts.Any(x => x.Id == loanApplication.ProductId))
            {
                recommendedProduct.IsPreviousProductMatched = false;
            }
            else
            {
                recommendedProduct.IsPreviousProductMatched = true;
            }
            recommendedProduct.ProductDetails = _globalRepository.CalculateLoanProductDetail(productRangeTypeMappingList.Where(x => x.ProductId == loanApplication.ProductId).ToList(), currentCurrencySymbol, loanApplication);
            recommendedProduct.ProductDetails.TenureStepperCount = loanPurposeRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.Lifecycle).StepperAmount;
            recommendedProduct.ProductDetails.AmountStepperCount = loanPurposeRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.LoanAmount).StepperAmount;

            return recommendedProduct;
        }

        /// <summary>
        /// Method is to get the list of recommended loan products.
        /// </summary>
        /// <param name="loanApplicationId">Current loan application id</param>
        /// <param name="currentUser">Current User</param>
        /// <param name="isCallFromSelectedProduct">Is method call from selected product method</param>
        /// <returns>List of recommended Loan Products</returns>
        public async Task<List<RecommendedProductAC>> GetRecommendedProductsListAsync(Guid loanApplicationId, CurrentUserAC currentUser, bool isCallFromSelectedProduct = false)
        {
            List<RecommendedProductAC> recommendedProducts = new List<RecommendedProductAC>();

            if (!currentUser.IsBankUser && !await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplicationId))
            {
                throw new UnauthorizedAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            var loanApplication = await _dataRepository.Fetch<LoanApplication>(x => x.Id == loanApplicationId)
                                                        .Include(x => x.LoanPurpose)
                                                        .Include(x => x.SubLoanPurpose)
                                                        .Include(x => x.CreatedByUser).SingleOrDefaultAsync();

            if (loanApplication.Id == Guid.Empty)
            {
                throw new InvalidParameterException(StringConstant.LoanApplicationNotExistsForGivenId);
            }

            var productRangeTypeMappingList = await _dataRepository.GetAll<ProductRangeTypeMapping>()
                                                    .Include(x => x.Product)
                                                    .ThenInclude(x => x.DescriptionPoints)
                                                    .Include(x => x.LoanRangeType)
                                                    .Where(x => x.Product.IsEnabled).ToListAsync();
            var productPurposeMappingList = await _dataRepository.GetAll<ProductSubPurposeMapping>()
                                                  .Include(x => x.SubLoanPurpose).ThenInclude(x => x.LoanPurpose).ToListAsync();

            var productRule = _mapper.Map<LoanApplication, ProductRuleAC>(loanApplication);
            productRule.LoanPeriod = productRule.LoanPeriod * 12;
            if (productPurposeMappingList.Any())
            {
                productRule.ProductSubPurposeMappings = _mapper.Map<List<ProductSubPurposeMapping>, List<ProductSubPurposeMappingAC>>(productPurposeMappingList);
            }

            if (productRangeTypeMappingList.Any())
            {
                productRule.ProductRangeTypeMappings = _mapper.Map<List<ProductRangeTypeMapping>, List<ProductRangeTypeMappingAC>>(productRangeTypeMappingList);
            }

            var productsPercentageSuitability = await _rulesUtility.ExecuteProductRules(productRule);

            if (!productsPercentageSuitability.Any())
            {
                throw new DataNotFoundException();
            }

            if (isCallFromSelectedProduct)
            {
                foreach (var productPercentageSuitability in productsPercentageSuitability)
                {
                    recommendedProducts.Add(new RecommendedProductAC
                    {
                        Id = productPercentageSuitability.ProductId
                    });
                }
                return recommendedProducts;
            }

            var currentCurrencySymbol = _configuration.GetSection("Currency:Symbol").Value;
            var productsRangeDetailList = productRangeTypeMappingList.Where(x => productsPercentageSuitability.Select(y => y.ProductId).Contains(x.ProductId)).ToList();

            var applicableProductRangeMappingList = productsRangeDetailList.GroupBy(x => x.ProductId);

            var loanPurposeRangeTypeMappings = await _dataRepository.Fetch<LoanPurposeRangeTypeMapping>(x => x.LoanPurposeId == loanApplication.LoanPurposeId).Include(x => x.LoanRangeType).ToListAsync();

            foreach (var applicableProduct in applicableProductRangeMappingList)
            {
                var loanProduct = applicableProduct.Select(x => x.Product).First();

                // Check Date 
                var isProductAdd = DateTime.Compare(DateTime.Now, loanProduct.ProductStartDate) > 0 && DateTime.Compare(loanProduct.ProductStartDate, loanProduct.ProductEndDate) < 0;

                if (isProductAdd)
                {
                    var loanProductAC = _mapper.Map<Product, RecommendedProductAC>(loanProduct);

                    // Loan Business suitability
                    loanProductAC.BusinessPercentageSuitability = productsPercentageSuitability.First(x => x.ProductId == loanProductAC.Id).PercentageSuitability;

                    // Loan product recommended
                    loanProductAC.IsProductRecommended = productsPercentageSuitability.First(x => x.ProductId == loanProductAC.Id).IsRecommended;

                    // Product amount range
                    loanProductAC.ProductAmountRange = String.Format("{0} {1} - {2} {3}", currentCurrencySymbol, ((double)applicableProduct.First(x => x.LoanRangeType.Name == StringConstant.LoanAmount).Minimum).ToString("C", CultureInfo.CreateSpecificCulture("en-US")).Split('$')[1].Split('.')[0], currentCurrencySymbol, ((double)applicableProduct.First(x => x.LoanRangeType.Name == StringConstant.LoanAmount).Maximum).ToString("C", CultureInfo.CreateSpecificCulture("en-US")).Split('$')[1].Split('.')[0]);

                    // Product period range
                    loanProductAC.ProductPeriodRange = String.Format("{0} - {1}", ((double)(applicableProduct.First(x => x.LoanRangeType.Name == StringConstant.Lifecycle).Minimum / 12)).ToString("C", CultureInfo.CreateSpecificCulture("en-US")).Split('$')[1].Split('.')[0], ((double)(applicableProduct.First(x => x.LoanRangeType.Name == StringConstant.Lifecycle).Maximum / 12)).ToString("C", CultureInfo.CreateSpecificCulture("en-US")).Split('$')[1].Split('.')[0]);

                    // Loan Product Detail
                    loanProductAC.ProductDetails = _globalRepository.CalculateLoanProductDetail(applicableProduct.ToList(), currentCurrencySymbol, loanApplication);
                    loanProductAC.ProductDetails.TenureStepperCount = loanPurposeRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.Lifecycle).StepperAmount;
                    loanProductAC.ProductDetails.AmountStepperCount = loanPurposeRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.LoanAmount).StepperAmount;


                    recommendedProducts.Add(loanProductAC);
                }
            }

            if (!recommendedProducts.Any())
            {
                throw new DataNotFoundException();
            }
            else
            {
                return recommendedProducts.OrderByDescending(x => x.BusinessPercentageSuitability).ToList();
            }
        }
    }
}
