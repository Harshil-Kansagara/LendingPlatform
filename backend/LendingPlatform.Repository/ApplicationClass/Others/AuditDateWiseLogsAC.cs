using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class AuditDateWiseLogsAC
    {
        /// <summary>
        /// Created date of the log.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// Contains list of audit logs by date.
        /// </summary>
        public List<AuditLogAC> AuditLogs { get; set; }
    }
}
