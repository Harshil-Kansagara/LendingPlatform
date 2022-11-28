using LendingPlatform.Utils.ApplicationClass;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface IExperianUtility
    {

        /// <summary>
        /// Fetch credit report of company from experian api
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="experianConfigurationJson">Experian ConfigurationJson</param>
        /// <returns></returns>
        Task<PremierProfilesResponseAC> FetchCompanyCreditScoreExperianAsync(string bin, string experianConfigurationJson);

        /// <summary>
        /// Fetch credit report of user from experian api
        /// </summary>
        /// <param name="user"></param>
        /// <param name="experianConfigurationJson">Experian ConfigurationJson</param>
        /// <returns></returns>
        Task<UserInfoAC> FetchUserCreditScoreExperianAsync(UserInfoAC user, string experianConfigurationJson);
    }
}
