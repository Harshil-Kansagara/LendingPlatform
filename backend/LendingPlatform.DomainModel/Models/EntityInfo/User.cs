using Audit.EntityFramework;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.Utils.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models.EntityInfo
{
    public class User : BaseModel
    {
        [AuditIgnore]
        [Required]
        public Guid Id { get; set; }
        [ForeignKey("Id")]
        public virtual Entity Entity { get; set; }


        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [RegularExpression(StringConstant.SSNRegularExpression,
         ErrorMessage = StringConstant.CharacterNotAllowed)]
        public string SSN { get; set; }

        [RegularExpression(StringConstant.PhoneRegularExpression,
         ErrorMessage = StringConstant.NotValidPhone)]
        public string Phone { get; set; }

        public DateTime? DOB { get; set; }

        public ResidencyStatus? ResidencyStatus { get; set; }
        /// <summary>
        /// User self declared credit score in range eg. 100-250.
        /// </summary>
        public string SelfDeclaredCreditScore { get; set; }
        /// <summary>
        /// If user has any bankruptcy in the past years.
        /// </summary>
        public bool HasBankruptcySelfDeclared { get; set; }
        /// <summary>
        /// If user has any jugements in the past months.
        /// </summary>
        public bool HasAnyJudgementsSelfDeclared { get; set; }
        [AuditIgnore]
        [Required]
        public bool IsRegistered { get; set; }
    }
}
