
namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceAccountSeedDataAC
    {
        /// <summary>
        /// Unique identifier for account seed data object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Account name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Account order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Is account enabled or not
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
