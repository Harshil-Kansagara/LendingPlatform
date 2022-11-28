using LendingPlatform.Utils.ApplicationClass.Product;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface IRulesUtility
    {
        /// <summary>
        /// Method to execute rule for fetching finance standard account
        /// </summary>
        /// <param name="inputObj"></param>
        /// <returns></returns>
        Task<JToken> ExecuteRuleForFetchingFinanceStandardAccountsAsync(object inputObj);

        /// <summary>
        /// Method to execute rule for evaluating loan
        /// </summary>
        /// <param name="inputObj"></param>
        /// <returns></returns>
        Task<JToken> ExecuteRuleForEvaluatingLoanAsync(object inputObj);

        /// <summary>
        /// Method is to execute product drool rule
        /// </summary>
        /// <param name="productRule">ProductRuleAC object</param>
        /// <returns>List of ProductPercentageSuitabilityAC</returns>
        Task<List<ProductPercentageSuitabilityAC>> ExecuteProductRules(ProductRuleAC productRule);
    }
}
