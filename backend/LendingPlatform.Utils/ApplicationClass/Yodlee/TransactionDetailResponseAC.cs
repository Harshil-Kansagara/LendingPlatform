
namespace LendingPlatform.Utils.ApplicationClass.Yodlee
{
    public class TransactionDetailResponseAC
    {
        public string Container { get; set; }
        public long Id { get; set; }
        public TransactionAmountAC Amount { get; set; }
        public string BaseType { get; set; }
        public string CategoryType { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int DetailCategoryId { get; set; }
        public string CategorySource { get; set; }
        public long HighLevelCategoryId { get; set; }
        public string CreatedDate { get; set; }
        public string LastUpdated { get; set; }
        public TransactionDescriptionAC Description { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public bool IsManual { get; set; }
        public string SourceType { get; set; }
        public string Date { get; set; }
        public string TransactionDate { get; set; }
        public string PostDate { get; set; }
        public string Status { get; set; }
        public long AccountId { get; set; }
        public TransactionMerchantAC Merchant { get; set; }
    }
}
