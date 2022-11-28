namespace LendingPlatform.Utils.ApplicationClass.Plaid.Management
{
    /// <summary>
    /// Represents a response from plaid's '/item/public_token/exchange' endpoint. Exchange a Link public_token for an API access_token.
    /// </summary>
    /// <seealso cref="Plaid.ResponseBaseAC" />
    public class ExchangeTokenResponseAC : ResponseBaseAC
    {
        /// <summary>
        /// Gets or sets the <see cref="Entity.ItemAC"/> identifier.
        /// </summary>
        /// <value>The item identifier.</value>
        public string ItemId { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public string AccessToken { get; set; }
    }
}