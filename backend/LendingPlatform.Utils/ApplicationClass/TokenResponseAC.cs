
namespace LendingPlatform.Utils.ApplicationClass
{
    public class TokenResponseAC
    {
        public string IssuedAt { get; set; }
        public string ExpiresIn { get; set; }
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
        public string Nonce { get; set; }
    }
}
