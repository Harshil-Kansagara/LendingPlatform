using System;

namespace LendingPlatform.Utils.ApplicationClass.Plaid.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a response from the Plaid API contains an error.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class PlaidException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaidException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PlaidException(string message) : base(message)
        {
        }

        /// <summary>
        /// Gets or sets the type of the error. A broad categorization of the error.
        /// </summary>
        /// <value>The type of the error.</value>
        public string ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the error code. The particular error code. Each error_type has a specific set
        /// </summary>
        /// <value>The error code.</value>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the display message. A user-friendly representation of the error code. null if the error is not related to user action. This may change over time and is not safe for programmatic use.
        /// </summary>
        /// <value>The display message.</value>
        public string DisplayMessage { get; set; }
    }
}