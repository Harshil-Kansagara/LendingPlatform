
namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceConstantOptionSeedDataAC
    {
        /// <summary>
        /// Unique identifier for option (of dropdown) seed data object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Option value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Option order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Is option enabled or not
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
