
namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceAttributeSeedDataAC
    {
        /// <summary>
        /// Unique identifier for attribute seed data object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Attribute (question) text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Attribute response type
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// Is attribute enabled or not
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
