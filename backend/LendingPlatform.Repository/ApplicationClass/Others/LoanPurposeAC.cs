using LendingPlatform.Repository.ApplicationClass.Applications;
using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class LoanPurposeAC
    {
        /// <summary>
        /// Unique identifier for the purpose.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Name of the purpose.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Sequencial order of the purpose.
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Is purpose enabled or not.
        /// </summary>
        public bool IsEnabled { get; set; }
        /// <summary>
        /// List of range type mappings.
        /// </summary>
        public List<LoanPurposeRangeTypeMappingAC> LoanPurposeRangeTypeMappings { get; set; }
        /// <summary>
        /// List of sub loan purposes.
        /// </summary>
        public List<SubLoanPurposeAC> SubLoanPurposes { get; set; }
    }
}
