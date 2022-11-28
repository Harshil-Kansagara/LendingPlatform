using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceAccountAC
    {
        /// <summary>
        /// Unique identifier for accpunt object
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Account name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Account order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// List of categories linked with account
        /// </summary>
        public List<PersonalFinanceCategoryAC> Categories { get; set; }
    }
}
