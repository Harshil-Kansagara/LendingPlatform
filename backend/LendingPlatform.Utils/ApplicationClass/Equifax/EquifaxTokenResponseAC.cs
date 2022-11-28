
namespace LendingPlatform.Utils.ApplicationClass.Equifax
{
    public class EquifaxTokenResponseAC
    {
        #region Properties
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string ExpiresIn { get; set; }
        public string Scope { get; set; }
        #endregion
    }
}
