namespace LendingPlatform.Utils.ApplicationClass.Plaid.Institution
{

    /// <summary>
    /// Represents an <see cref="InstitutionAC"/> login credentials.
    /// </summary>
    public class CredentialAC
    {
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>The label.</value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        public string Type { get; set; }
    }
}
