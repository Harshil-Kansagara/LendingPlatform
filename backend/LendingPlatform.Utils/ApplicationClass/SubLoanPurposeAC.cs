namespace LendingPlatform.Utils.ApplicationClass
{
    public class SubLoanPurposeAC
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public int LoanPurposeId { get; set; }
        public bool IsEnabled { get; set; }
    }
}
