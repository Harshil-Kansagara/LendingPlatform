using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.PayPal
{
    public class InvoicesResponseAC
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<InvoicesItemsAC> Items { get; set; }
    }
}
