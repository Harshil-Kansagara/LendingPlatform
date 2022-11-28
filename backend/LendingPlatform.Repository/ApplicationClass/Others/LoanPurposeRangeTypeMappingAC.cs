using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class LoanPurposeRangeTypeMappingAC
    {
        public Guid Id { get; set; }

        public Guid LoanPurposeId { get; set; }

        public string RangeTypeName { get; set; }

        public decimal Minimum { get; set; }

        public decimal Maximum { get; set; }

        public decimal StepperAmount { get; set; }
    }
}
