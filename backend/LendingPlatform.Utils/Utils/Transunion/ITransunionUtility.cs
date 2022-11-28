using LendingPlatform.Utils.ApplicationClass;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils.Transunion
{
    public interface ITransunionUtility
    {
        /// <summary>
        /// Method to fetch consumer credit report from transunion API
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="transunionConfigurationJson">Transunion ConfigurationJson</param>
        /// <returns></returns>
        Task<UserInfoAC> FetchConsumerCreditReportAsync(UserInfoAC userInfo, string transunionConfigurationJson);
    }
}
