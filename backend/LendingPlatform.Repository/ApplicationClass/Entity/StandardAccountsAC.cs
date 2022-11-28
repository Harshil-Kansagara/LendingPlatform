using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class StandardAccountsAC
    {
        /// <summary>
        /// Id of account
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Standard account name
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// Amount for account
        /// </summary>
        public List<decimal?> Amount { get; set; }

        /// <summary>
        /// True if the account is a parent account
        /// </summary>
        public bool IsParent { get; set; }

        /// <summary>
        /// Source accounts List
        /// </summary>
        public List<AccountChartAC> SourceList { get; set; }

    }
}
