namespace LendingPlatform.Utils.ApplicationClass.Plaid.Institution
{
    /// <summary>
    /// Represents a request for plaid's '/institutions/get_by_id' endpoint. The '/institutions/get_by_id' endpoint to retrieve a <see cref="Entity.Institution"/> with the specified id.
    /// </summary>
    /// <seealso cref="Plaid.SerializableContentAC" />
    public class SearchByIdRequestAC : SerializableContentAC
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
        /// Gets or sets the <see cref="Entity.Institution"/> identifier.
        /// </summary>
        /// <value>The institution identifier.</value>
        public string InstitutionId { get; set; }
    }
}