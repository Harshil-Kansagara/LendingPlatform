using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class AuditLogFilterAC
    {
        /// <summary>
        /// It contain companyId or loanId.
        /// </summary>
        public Guid LogBlockNameId { get; set; }
        /// <summary>
        /// Start date.
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End date.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
