using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass
{
    public class EmailLoanDetailsAC
    {
        public List<BasicUserDetailsAC> ShareHoldersDetails { get; set; }
        public string CompanyName { get; set; }
        public string LoanApplicationNumber { get; set; }
        public string Subject { get; set; }
        public string TemplateFile { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal? LoanAmountFigure { get; set; }
        public decimal? LoanPeriod { get; set; }
        public decimal? InterestRate { get; set; }
        public string LoanApplicationRedirectUrl { get; set; }
    }
}
