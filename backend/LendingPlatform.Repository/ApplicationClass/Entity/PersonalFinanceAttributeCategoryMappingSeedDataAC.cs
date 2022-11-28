
namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceAttributeCategoryMappingSeedDataAC
    {
        /// <summary>
        /// Unique identifier for attribute category mapping seed data object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique identifier for category
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Unique identifier for attribute
        /// </summary>
        public int AttributeId { get; set; }

        /// <summary>
        /// Unique identifier for parent mapping
        /// </summary>
        public int? ParentAttributeCategoryMappingId { get; set; }

        /// <summary>
        /// Attribute's order in its category
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Is attribute used to calculate original amount of category
        /// </summary>
        public bool IsOriginal { get; set; }

        /// <summary>
        /// Is attribute used to calculate current amount of category
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Unique identifier for attribute's constant (dropdown)
        /// </summary>
        public int? PersonalFinanceConstantId { get; set; }
    }
}
