namespace LendingPlatform.Utils.ApplicationClass.Plaid.Entity
{
    /// <summary>
    /// Represents a Plaid item. A set of credentials at a financial institution.
    /// </summary>
    public class ItemAC
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string ItemId { get; set; }

        /// <summary>
        /// Gets or sets the available products.
        /// </summary>
        /// <value>The available products.</value>
        public string[] AvailableProducts { get; set; }

        /// <summary>
        /// Gets or sets the billed products.
        /// </summary>
        /// <value>The billed products.</value>
        public string[] BilledProducts { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Entity.Institution"/> identifier.
        /// </summary>
        /// <value>The institution identifier.</value>
        public string InstitutionId { get; set; }

        /// <summary>
        /// Gets or sets the webhook.
        /// </summary>
        /// <value>The webhook.</value>
        public string Webhook { get; set; }
    }
}