namespace LendingPlatform.Utils.ApplicationClass.Plaid.Institution
{
    /// <summary>
    /// Represents a banking institution.
    /// </summary>
    public class InstitutionAC
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string InstitutionId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has Multi-Factor Authentication.
        /// </summary>
        /// <value><c>true</c> if this instance has Multi-Factor Authentication; otherwise, <c>false</c>.</value>
        public bool HasMfa { get; set; }

        /// <summary>
        /// Gets or sets the Multi-Factor Authentication selections.
        /// </summary>
        /// <value>The mfa selections.</value>
        public string[] Mfa { get; set; }

        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        public CredentialAC[] Credentials { get; set; }

        /// <summary>
        /// Gets or sets the products.
        /// </summary>
        /// <value>The products.</value>
        public string[] Products { get; set; }

    }
}