using Audit.EntityFramework;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.LoanApplicationInfo
{
    public class Consent
    {
        [AuditIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string ConsentText { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }
    }
}
