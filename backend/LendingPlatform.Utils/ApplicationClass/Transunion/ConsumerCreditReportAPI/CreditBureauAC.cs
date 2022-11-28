namespace LendingPlatform.Utils.ApplicationClass.Transunion.ConsumerCreditReportAPI
{
    public class CreditBureauAC
    {
        #region Public Properties
        public string Document { get; set; }
        public string Version { get; set; }
        public TransactionControlAC TransactionControl { get; set; }
        public ProductAC Product { get; set; }
        #endregion
    }
}
