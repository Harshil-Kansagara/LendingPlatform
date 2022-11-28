namespace LendingPlatform.Utils.ApplicationClass.Plaid.Management
{
    /// <summary>
    /// Represents a response from plaid's '/link/token/create' endpoint. Create a link_token.
    /// </summary>
    /// <seealso cref="Plaid.ResponseBaseAC" />
    public class CreateLinkTokenResponseAC : ResponseBaseAC
    {
        /// <summary>
        /// Gets or sets the link token.
        /// </summary>
        /// <value>The link token.</value>
        public string LinkToken { get; set; }

        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>The expiration.</value>
        public string Expiration { get; set; }
    }
}