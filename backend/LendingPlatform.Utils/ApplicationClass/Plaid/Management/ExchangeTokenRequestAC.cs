namespace LendingPlatform.Utils.ApplicationClass.Plaid.Management
{
    /// <summary>
    /// Represents a request for plaid's '/item/public_token/exchange' endpoint. Exchange a Link public_token for an API access_token.
    /// </summary>
    /// <seealso cref="Plaid.SerializableContentAC" />
    public class ExchangeTokenRequestAC : SerializableContentAC
    {
        /// <summary>
        /// Gets or sets the public_token.
        /// </summary>
        /// <value>The public token.</value>
        public string PublicToken { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>The client identifier.</value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        /// <value>The secret.</value>
        public string Secret { get; set; }
    }
}