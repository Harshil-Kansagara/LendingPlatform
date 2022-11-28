using LendingPlatform.DomainModel.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class AuditLog
    {
        /// <summary>
        /// Unique audit LogId.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        /// <summary>
        /// Table name that operation perform on the table.
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Action like "Insert", "Update" or "Delete"
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// Json object for the table details.
        /// </summary>
        public string AuditJson { get; set; }
        /// <summary>
        /// Created date of the log.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// UserId of the who generates the log.
        /// </summary>
        public Guid? CreatedByUserId { get; set; }
        /// <summary>
        ///  User object of Who has generated the logs.
        /// </summary>
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }
        /// <summary>
        ///  Bank user object of Who has generated the logs.
        /// </summary>
        public Guid? CreatedByBankUserId { get; set; }
        /// <summary>
        /// Bank user object.
        /// </summary>
        [ForeignKey("CreatedByBankUserId")]
        public virtual BankUser CreatedByBankUser { get; set; }
        /// <summary>
        /// Primary key of the audit log table.
        /// </summary>
        public Guid TablePk { get; set; }
        /// <summary>
        /// Ip address of the user.
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// It uses to fetch the block wise record.
        /// eg. Company and loan application.
        /// </summary>
        public ResourceType LogBlockName { get; set; }
        /// <summary>
        /// LoanId or EntityId. It's use to fetch the block wise log details of the loan or entity.
        /// </summary>
        public Guid? LogBlockNameId { get; set; }
    }
}
