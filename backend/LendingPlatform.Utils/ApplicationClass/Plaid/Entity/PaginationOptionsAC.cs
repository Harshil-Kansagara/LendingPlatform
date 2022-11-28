namespace LendingPlatform.Utils.ApplicationClass.Plaid.Entity
{

    /// <summary>
    /// Represents pagination options.
    /// </summary>
    public class PaginationOptionsAC
    {
        /// <summary>
        /// Gets or sets the number of transactions to fetch, where 0 &lt; count &lt;= 500.
        /// </summary>
        /// <value>The number of transactions to return.</value>
        public uint Count { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions to skip, where offset &gt;= 0.
        /// </summary>
        /// <value>The offset.</value>
        public uint Offset { get; set; }

        /// <summary>
        /// Gets or sets the list of account ids to retrieve for the <see cref="Entity.ItemAC" />. Note: An error will be returned if a provided account_id is not associated with the <see cref="Entity.ItemAC" />.
        /// </summary>
        /// <value>The account ids.</value>
        public string[] AccountIds { get; set; }
    }
}
