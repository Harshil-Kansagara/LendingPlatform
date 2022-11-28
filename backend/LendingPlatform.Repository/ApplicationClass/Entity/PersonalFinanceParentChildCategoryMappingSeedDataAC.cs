
namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceParentChildCategoryMappingSeedDataAC
    {
        /// <summary>
        /// Unique identifier for parent child category mapping seed data object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique identifier for parent category object
        /// </summary>
        public int ParentCategoryId { get; set; }

        /// <summary>
        /// Unique identifier for child category object
        /// </summary>
        public int ChildCategoryId { get; set; }

        /// <summary>
        /// Unique identifier for parent category-attribute mapping object
        /// </summary>
        public int? ParentAttributeCategoryMappingId { get; set; }
    }
}
