using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.Quickbooks
{
    public class QuickbooksAccountAC
    {
        public string Id { get; set; }
        public string Group { get; set; }
        public List<decimal> Amounts { get; set; }
    }
}
