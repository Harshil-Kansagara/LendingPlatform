using System;

namespace LendingPlatform.Repository.ApplicationClass.Applications
{
    public class SubLoanPurposeAC
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Guid LoanPurposeId { get; set; }
        public bool IsEnabled { get; set; }
    }
}
