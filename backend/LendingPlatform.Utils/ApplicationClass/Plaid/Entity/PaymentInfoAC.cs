namespace LendingPlatform.Utils.ApplicationClass.Plaid.Entity
{

    /// <summary>
    /// Metadata about the customer and merchant.
    /// </summary>
    public class PaymentInfoAC
    {
        /// <summary>
        /// Gets or sets the reference number.
        /// </summary>
        /// <value>The reference number.</value>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the PPD identifier.
        /// </summary>
        /// <value>The PPD identifier.</value>
        public string PpdId { get; set; }

        /// <summary>
        /// Gets or sets the name of the payee.
        /// </summary>
        /// <value>The name of the payee.</value>
        public string Payee { get; set; }

        /// <summary>
        /// Gets or sets the name of the payer.
        /// </summary>
        /// <value>The name of the payer.</value>
        public string Payer { get; set; }

        /// <summary>
        /// Gets or sets the payment processor.
        /// </summary>
        /// <value>The payment processor.</value>
        public string PaymentProcessor { get; set; }

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        /// <value>The reason.</value>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the by order of.
        /// </summary>
        /// <value>The by order of.</value>
        public string ByOrderOf { get; set; }
    }
}
