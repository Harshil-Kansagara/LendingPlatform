using LendingPlatform.Utils.Enums.Plaid;
using System;

namespace LendingPlatform.Utils.ApplicationClass.Plaid.Entity
{
    /// <summary>
    /// Represents a banking transaction.
    /// </summary>
    /// <remarks>
    /// Transaction data is standardized across financial institutions, and in many cases transactions are linked to a clean name, entity type, location, and category. Similarly, account data is standardized and returned with a clean name, number, balance, and other meta information where available.
    /// </remarks>
    public class PlaidTransactionAC
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique id of the transaction.
        /// </summary>
        /// <value>The transaction identifier.</value>
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the id of the account in which this transaction occurred.
        /// </summary>
        /// <value>The account identifier.</value>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the hierarchical array of the categories to which this transaction.
        /// </summary>
        /// <value>The categories.</value>
        public string[] Category { get; set; }

        /// <summary>
        /// Gets or sets the id of the category to which this transaction belongs. See https://plaid.com/docs/api/#categories.
        /// </summary>
        /// <value>The category identifier.</value>
        public string CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the type of the transaction.
        /// </summary>
        /// <value>The type of the transaction.</value>
        public string TransactionType { get; set; }

        /// <summary>
        /// Gets or sets the settled dollar value. Positive values when money moves out of the account; negative values when money moves in. For example, purchases are positive; credit card payments, direct deposits, refunds are negative.
        /// </summary>
        /// <value>The amount.</value>
        public decimal Amount { get; set; }

        /// <summary>
        /// The ISO currency code of the transaction, either USD or CAD. Always null if unofficial_currency_code is non-null.
        /// </summary>
        /// <value>The ISO currency code.</value>
        public string IsoCurrencyCode { get; set; }

        /// <summary>
        /// The unofficial currency code associated with the transaction. Always null if iso_currency_code is non-null.
        /// </summary>
        /// <value>The unofficial currency code.</value>
        public string UnofficialCurrencyCode { get; set; }

        /// <summary>
        /// Gets the currency code from either IsoCurrencyCode or UnofficialCurrencyCode. If non-null, IsoCurrencyCode is returned, else if non-null, UnofficialCurrencyCode, else null is returned.
        /// </summary>
        /// <value>Either available currency code.</value>
        public string CurrencyCode => IsoCurrencyCode ?? UnofficialCurrencyCode ?? null;

        /// <summary>
        /// Gets or sets the date of the transaction.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>For pending transactions, Plaid returns the date the transaction occurred; for posted transactions, Plaid returns the date the transaction posts.</remarks>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the information of the merchant's location. Typically <c>null</c>.
        /// </summary>
        /// <value>The location details.</value>
        public LocationInfoAC Location { get; set; }

        /// <summary>
        /// Gets or sets the information of the payment and payment processor. Typically <c>null</c>.
        /// </summary>
        /// <value>The payment details.</value>
        public PaymentInfoAC PaymentMeta { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PlaidTransactionAC"/> is pending or unsettled. Pending transaction details (name, <see cref="TransactionType"/>, <see cref="Amount"/>) may change before they are settled.
        /// </summary>
        /// <value><c>true</c> if pending; otherwise, <c>false</c>.</value>
        public bool Pending { get; set; }

        /// <summary>
        /// Gets or sets the id of a posted transaction's associated pending transaction.
        /// </summary>
        /// <value>The pending transaction identifier.</value>
        public string PendingTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the account owner. This property is not typically populated and only relevant when dealing with sub-accounts.
        /// </summary>
        /// <value>The account owner.</value>
        public string AccountOwner { get; set; }

        /// <summary>
        /// Gets the transaction type.
        /// </summary>
        /// <value>The transaction type.</value>
        public TransactionType Type
        {
            get
            {
                bool valid = Enum.TryParse(TransactionType, out TransactionType type);
                return valid ? type : Enums.Plaid.TransactionType.Unresolved;
            }
        }
    }
}