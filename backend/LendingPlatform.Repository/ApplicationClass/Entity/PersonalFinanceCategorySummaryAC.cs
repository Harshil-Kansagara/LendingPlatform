
namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceCategorySummaryAC
    {
        /// <summary>
        /// Category name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Category order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Original amount of category
        /// </summary>
        public decimal? OriginalAmount { get; set; }

        /// <summary>
        /// Current amount of category
        /// </summary>
        public decimal? CurrentAmount { get; set; }
    }
}
