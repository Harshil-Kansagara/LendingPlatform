using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceConstantSeedDataAC
    {
        /// <summary>
        /// Unique identifier for cosntant seed data object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Constant name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of options available for constant (dropdown)
        /// </summary>
        public List<PersonalFinanceConstantOptionSeedDataAC> Options { get; set; }
    }
}
