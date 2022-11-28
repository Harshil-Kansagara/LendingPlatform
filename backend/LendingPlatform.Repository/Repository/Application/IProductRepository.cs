using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.Application
{
    public interface IProductRepository
    {


        /// <summary>
        /// Method to save the loan product related to loan application
        /// </summary>
        /// <param name="loanApplicationId">Loan application id </param>
        /// <param name="productId">Product Id</param>
        /// <param name="currentUser"> Current User </param>
        /// <returns></returns>
        Task SaveLoanProductAsync(Guid loanApplicationId, Guid productId, CurrentUserAC currentUser);

        /// <summary>
        /// Method is to get selected product for loan application
        /// </summary>
        /// <param name="loanApplicationId">Loan Application Id</param>
        /// <param name="currentUser">Current User</param>
        /// <param name="recommendedProducts">List of recommended products</param>
        /// <returns>Application detailAC object</returns>
        Task<RecommendedProductAC> GetSelectedProductDataAsync(Guid loanApplicationId, CurrentUserAC currentUser, List<RecommendedProductAC> recommendedProducts = null);

        /// <summary>
        /// Method is to get the list of recommended loan products.
        /// </summary>
        /// <param name="loanApplicationId">Current loan application id</param>
        /// <param name="currentUser">Current User</param>
        /// <param name="isCallFromSelectedProduct">Is method call from selected product method</param>
        /// <returns>List of recommended Loan Products</returns>
        Task<List<RecommendedProductAC>> GetRecommendedProductsListAsync(Guid loanApplicationId, CurrentUserAC currentUser, bool isCallFromSelectedProduct = false);
    }
}
