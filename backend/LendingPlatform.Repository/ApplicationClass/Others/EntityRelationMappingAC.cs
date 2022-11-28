using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class EntityRelationMappingAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the entity relationship mapping object.
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// Relationship object
        /// </summary>
        public RelationshipAC Relation { get; set; }
        /// <summary>
        /// Share percentage
        /// </summary>
        public decimal? SharePercentage { get; set; }
        /// <summary>
        /// Unique identifier for primary entity object(borrowing entity)
        /// </summary>
        public Guid PrimaryEntityId { get; set; }
        #endregion
    }
}
