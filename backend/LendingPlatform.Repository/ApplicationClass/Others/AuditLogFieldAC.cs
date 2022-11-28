using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class AuditLogFieldAC
    {
        /// <summary>
        /// Field name.
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Old value of the field.
        /// </summary>
        public object OriginalValue { get; set; }
        /// <summary>
        /// New value of the field.
        /// </summary>
        public object NewValue { get; set; }
        /// <summary>
        /// Field type is guid.
        /// </summary>
        public bool IsGuid { get; set; }
        /// <summary>
        /// Log created date to show accurate data in the popup.
        /// </summary>
        public DateTime LogDate { get; set; }
        /// <summary>
        /// List of audit log ids.
        /// </summary>
        public List<object> Ids { get; set; }
    }
}
