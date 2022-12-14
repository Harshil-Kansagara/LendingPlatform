using LendingPlatform.Utils.ApplicationClass.Plaid.Entity;
using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.Plaid.Transactions
{
    /// <summary>
    /// Represents a response from plaid's '/transactions/get' endpoint. The /transactions/get endpoint allows developers to receive user-authorized transaction data for credit and depository-type Accounts. Transaction data is standardized across financial institutions, and in many cases transactions are linked to a clean name, entity type, location, and category. Similarly, account data is standardized and returned with a clean name, number, balance, and other meta information where available.
    /// </summary>
    /// <remarks>Due to the potentially large number of transactions associated with an <see cref="Entity.ItemAC"/>, results are paginated. Manipulate the count and offset parameters in conjunction with the total_transactions response body field to fetch all available Transactions.</remarks>
    /// <seealso cref="Plaid.ResponseBaseAC" />
    public class GetTransactionsResponseAC : ResponseBaseAC
    {
        /// <summary>
        /// Gets or sets the number of transactions returned.
        /// </summary>
        /// <value>The number of transactions returned.</value>
        public int TotalTransactions { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>The transactions.</value>
        public List<PlaidTransactionAC> Transactions { get; set; }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public ItemAC Item { get; set; }

        /// <summary>
        /// Gets or sets the accounts.
        /// </summary>
        /// <value>The accounts.</value>
        public List<AccountAC> Accounts { get; set; }
    }
}