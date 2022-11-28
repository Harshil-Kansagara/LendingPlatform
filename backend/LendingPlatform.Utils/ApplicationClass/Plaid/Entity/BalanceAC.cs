namespace LendingPlatform.Utils.ApplicationClass.Plaid.Entity
{
    /// <summary>
    /// Represents an account balance.
    /// </summary>
    public class BalanceAC
    {
        /// <summary>
        /// Gets or sets the current balance.
        /// </summary>
        /// <value>The current.</value>
        public decimal Current { get; set; }

        /// <summary>
        /// Gets or sets the available balance.
        /// </summary>
        /// <value>The available.</value>
        public decimal? Available { get; set; }

        /// <summary>
        /// Gets or sets the account limit.
        /// </summary>
        /// <value>The limit.</value>
        public decimal? Limit { get; set; }

        /// <summary>
        /// Gets or sets the iso currency code.
        /// </summary>
        /// <value>currency code.</value>
        public string IsoCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the unofficial currency code.
        /// </summary>
        /// <value>currency code.</value>
        public string UnofficialCurrencyCode { get; set; }

    }
}