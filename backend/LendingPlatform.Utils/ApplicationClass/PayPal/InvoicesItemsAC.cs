using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.PayPal
{
    public class InvoicesItemsAC
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public InvoiceDetailAC Detail { get; set; }
        public InvoiceInvoicerAC Invoicer { get; set; }
        public List<InvoicePrimaryRecipientsAC> PrimaryRecipients { get; set; }
        public InvoiceAmountAC Amount { get; set; }
    }
}
