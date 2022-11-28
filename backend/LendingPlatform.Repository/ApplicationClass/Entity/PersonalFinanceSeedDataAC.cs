using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceSeedDataAC
    {
        /// <summary>
        /// List of accounts given in JSON
        /// </summary>
        public List<PersonalFinanceAccountSeedDataAC> PersonalFinanceAccounts { get; set; }

        /// <summary>
        /// List of categories given in JSON
        /// </summary>
        public List<PersonalFinanceCategorySeedDataAC> PersonalFinanceCategories { get; set; }

        /// <summary>
        /// List of attributes given in JSON
        /// </summary>
        public List<PersonalFinanceAttributeSeedDataAC> PersonalFinanceAttributes { get; set; }

        /// <summary>
        /// List of constants given in JSON
        /// </summary>
        public List<PersonalFinanceConstantSeedDataAC> PersonalFinanceConstants { get; set; }

        /// <summary>
        /// List of attribute-category mappings
        /// </summary>
        public List<PersonalFinanceAttributeCategoryMappingSeedDataAC> PersonalFinanceAttributeCategoryMappings { get; set; }

        /// <summary>
        /// List of parent-child category mappings
        /// </summary>
        public List<PersonalFinanceParentChildCategoryMappingSeedDataAC> PersonalFinanceParentChildCategoryMappings { get; set; }
    }
}
