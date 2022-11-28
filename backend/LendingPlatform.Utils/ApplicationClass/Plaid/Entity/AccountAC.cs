namespace LendingPlatform.Utils.ApplicationClass.Plaid.Entity
{
    /// <summary>
    /// Represents a bank account.
    /// </summary>
    public class AccountAC
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ItemAC"/> identifier.
        /// </summary>
        /// <value>The item identifier.</value>
        public string ItemId { get; set; }

        /// <summary>
        /// Gets or sets the financial <see cref="Institution"/> identifier associated with the account.
        /// </summary>
        /// <value>The institution identifier.</value>
        public string InstitutionId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the last four digits of the Account's number.
        /// </summary>
        /// <value>The mask.</value>
        public string Mask { get; set; }

        /// <summary>
        /// Gets or sets the official name of the account.
        /// </summary>
        /// <value>The official name of the account.</value>
        public string OfficialName { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the sub.
        /// </summary>
        /// <value>The type of the sub.</value>
        public string Subtype { get; set; }

        /// <summary>
        /// Gets or sets the balance. The current balance is the total amount of funds in the account. The available balance is the current balance less any outstanding holds or debits that have not yet posted to the account.
        /// </summary>
        /// <remarks>Note: Not all institutions calculate the available balance. In the event that available balance is unavailable from the institution, Plaid will return an available balance value of <c>null</c>.</remarks>
        /// <value>The balance.</value>
        public BalanceAC Balances { get; set; }
    }
}