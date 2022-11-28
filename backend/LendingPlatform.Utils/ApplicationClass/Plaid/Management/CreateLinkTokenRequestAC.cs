using LendingPlatform.Utils.ApplicationClass.Plaid.Entity;
using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.Plaid.Management
{
    /// <summary>
    /// Represents a request for plaid's '/link/token/create' endpoint. Create a link_token.
    /// </summary>
    /// <seealso cref="Plaid.SerializableContentAC" />
    public class CreateLinkTokenRequestAC : SerializableContentAC
    {
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

        /// <summary>
        /// Gets or sets the client name.
        /// </summary>
        /// <value>The client name.</value>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>The language.</value>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the country codes.
        /// </summary>
        /// <value>The country codes.</value>
        public string[] CountryCodes { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>The user.</value>
        public PlaidUserAC User { get; set; }

        /// <summary>
        /// Gets or sets the products.
        /// </summary>
        /// <value>The products.</value>
        public string[] Products { get; set; }

        /// <summary>
        /// Gets or sets the webhook.
        /// </summary>
        /// <value>The webhook.</value>
        public string Webhook { get; set; }

        /// <summary>
        /// Gets or sets the link customization name.
        /// </summary>
        /// <value>The link customization name.</value>
        public string LinkCustomizationName { get; set; }

        /// <summary>
        /// Gets or sets the account filters.
        /// </summary>
        /// <value>The account filters.</value>
        public Dictionary<string, List<string>> AccountFilters { get; set; }

        /// <summary>
        /// Gets or sets the access_token.
        /// </summary>
        /// <value>The access token.</value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the redirect uri.
        /// </summary>
        /// <value>The redirect uri.</value>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the payment initiation.
        /// </summary>
        /// <value>The payment initiation.</value>
        /// <remarks>Payment initiation still needs to be typed and fully implemented.</remarks>
        public object PaymentInitiation { get; set; }
    }
}