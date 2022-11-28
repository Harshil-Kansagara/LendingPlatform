namespace LendingPlatform.Utils.ApplicationClass
{
    public class FinancialStatementsAC
    {
        /// <summary>
        /// JSON that is received for each report from third party 
        /// </summary>
        public string ReportJson { get; set; }

        /// <summary>
        /// Name of report as named in third party for ex, Xero names income statement as ProfitAndLoss
        /// </summary>
        public string ThirdPartyWiseName { get; set; }

        /// <summary>
        /// Name of report as we save in Jamoon
        /// </summary>
        public string ReportName { get; set; }

        /// <summary>
        /// Name of company picked in Quickbooks or Xero
        /// </summary>
        public string ThirdPartyWiseCompanyName { get; set; }

    }
}
