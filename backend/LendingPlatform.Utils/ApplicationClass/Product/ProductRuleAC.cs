using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.Product
{
    public class ProductRuleAC
    {
        public decimal LoanAmount { get; set; }

        public decimal LoanPeriod { get; set; }

        public string SubLoanPurposeName { get; set; }

        public List<ProductRangeTypeMappingAC> ProductRangeTypeMappings { get; set; }

        public List<ProductSubPurposeMappingAC> ProductSubPurposeMappings { get; set; }
    }
}
