
namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceConstantOptionAC
    {
        /// <summary>
        /// Unique identifier for option (of dropdown) object
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
    }
}
