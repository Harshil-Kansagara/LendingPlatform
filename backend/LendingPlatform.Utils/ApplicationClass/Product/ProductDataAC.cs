using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.Product
{
    public class ProductDataAC
    {
        public List<LoanRangeTypeSeedAC> LoanRangeTypes { get; set; }

        public List<LoanTypeSeedAC> LoanTypes { get; set; }

        public List<ProductsSeedAC> Products { get; set; }

        public List<LoanPurposeSeedAC> LoanPurposes { get; set; }

        public List<ProductTypeMappingSeedAC> ProductTypeMappings { get; set; }

        public List<ProductRangeTypeMappingSeedAC> ProductRangeTypeMappings { get; set; }

        public List<ProductSubPurposeMappingSeedAC> ProductSubPurposeMappings { get; set; }

        public List<LoanPurposeRangeTypeMappingSeedAC> LoanPurposeRangeTypeMappings { get; set; }
        public List<SubLoanPurposeAC> SubLoanPurposes { get; set; }
    }
}
