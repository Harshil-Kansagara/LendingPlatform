
namespace LendingPlatform.Utils.ApplicationClass.Product
{
    public class LoanPurposeRangeTypeMappingSeedAC
    {
        public int LoanPurposeId { get; set; }
        public int RangeTypeId { get; set; }
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }
        public decimal StepperAmount { get; set; }
    }
}
