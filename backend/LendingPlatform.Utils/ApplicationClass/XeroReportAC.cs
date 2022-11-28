using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace LendingPlatform.Utils.ApplicationClass
{
    public class XeroReportAC
    {
        public List<ReportWithRow> BalanceSheet { get; set; }
        public List<ReportWithRow> ProfitAndLoss { get; set; }
        public List<XeroAccountAC> Accounts { get; set; }
    }
}
