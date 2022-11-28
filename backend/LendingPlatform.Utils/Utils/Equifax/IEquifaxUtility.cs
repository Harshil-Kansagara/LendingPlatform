using LendingPlatform.Utils.ApplicationClass;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface IEquifaxUtility
    {
        #region Public Method(s)

        /// <summary>
        /// Method to fetch credit report of people from equifax consumer credit report API.
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="equifaxConfigurationJson">Equifax ConfigurationJson</param>
        /// <returns></returns>
        Task<UserInfoAC> FetchUserCreditScoreEquifaxAsync(UserInfoAC userInfo, string equifaxConfigurationJson);

        #endregion
    }
}
