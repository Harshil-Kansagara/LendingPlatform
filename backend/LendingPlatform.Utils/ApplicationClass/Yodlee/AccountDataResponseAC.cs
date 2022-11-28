
namespace LendingPlatform.Utils.ApplicationClass.Yodlee
{
    public class AccountDataResponseAC
    {
        public long Id { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public long ProviderId { get; set; }
        public AccountBalanceResponseAC Balance { get; set; }
        public string AccountInformationJson { get; set; }
    }
}
