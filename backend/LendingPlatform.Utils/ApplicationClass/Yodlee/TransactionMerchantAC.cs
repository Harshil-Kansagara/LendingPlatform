
using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.Yodlee
{
    public class TransactionMerchantAC
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public List<string> CategoryLabel { get; set; }
        public MerchantAddressAC Address { get; set; }
    }
}
