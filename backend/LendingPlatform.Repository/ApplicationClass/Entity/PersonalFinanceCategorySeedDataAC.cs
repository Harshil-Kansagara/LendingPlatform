
namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceCategorySeedDataAC
    {
        /// <summary>
        /// Unique identifier for caregory seed data object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique identifier of account object
        /// </summary>
        public int PersonalFinanceAccountId { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Category order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Is category enabled or not
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
