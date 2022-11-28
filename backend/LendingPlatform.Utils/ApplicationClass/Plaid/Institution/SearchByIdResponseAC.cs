namespace LendingPlatform.Utils.ApplicationClass.Plaid.Institution
{
    /// <summary>
    /// Represents a response from plaid's '/institutions/get_by_id' endpoints. The '/institutions/get_by_id' endpoint to retrieve a <see cref="Entity.Institution"/> with the specified id.
    /// </summary>
    /// <seealso cref="Plaid.ResponseBaseAC" />
    public class SearchByIdResponseAC : ResponseBaseAC
    {
        /// <summary>
        /// Gets or sets the institution.
        /// </summary>
        /// <value>The institution.</value>
        public InstitutionAC Institution { get; set; }
    }
}