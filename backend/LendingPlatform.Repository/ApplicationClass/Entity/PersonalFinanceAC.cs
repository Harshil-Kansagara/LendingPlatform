using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceAC
    {
        /// <summary>
        /// Unique identifier for personal finance object
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Financial Statement name (i.e. Personal Finance)
        /// </summary>
        public string FinancialStatement { get; set; }

#nullable enable

        /// <summary>
        /// Summary of financial information
        /// </summary>
        public PersonalFinanceSummaryAC? Summary { get; set; }

#nullable disable

        /// <summary>
        /// Account wise list of financial details 
        /// </summary>
        public List<PersonalFinanceAccountAC> Accounts { get; set; }
    }
}
