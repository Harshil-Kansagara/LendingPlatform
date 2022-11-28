namespace LendingPlatform.Utils.ApplicationClass.Plaid
{
    /// <summary>
    /// Provides methods and properties for making a standard request.
    /// </summary>
    /// <seealso cref="Plaid.SerializableContentAC" />
    public abstract class RequestBaseAC : SerializableContentAC
    {
        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        /// <value>The secret.</value>
        public string Secret { get; set; }

        /// <summary>
        /// Gets or sets the client_id.
        /// </summary>
        /// <value>The client identifier.</value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the access_token.
        /// </summary>
        /// <value>The access token.</value>
        public string AccessToken { get; set; }
    }
}