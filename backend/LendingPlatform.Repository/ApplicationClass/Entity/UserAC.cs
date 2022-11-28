using LendingPlatform.DomainModel.Enums;
using LendingPlatform.Utils.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class UserAC
    {
        #region Public Properties
        /// <summary>
        /// First Name of the user
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Middle Name of the user
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Last Name of the user
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Email of the user
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        /// <summary>
        /// SSN of the user
        /// </summary>
        [RegularExpression(StringConstant.SSNRegularExpression,
         ErrorMessage = StringConstant.CharacterNotAllowed)]
        public string SSN { get; set; }
        /// <summary>
        /// Valid phone number of the user
        /// </summary>
        [RegularExpression(StringConstant.PhoneRegularExpression,
         ErrorMessage = StringConstant.NotValidPhone)]
        public string Phone { get; set; }
        /// <summary>
        /// Date of birth of the user
        /// </summary>
        public DateTime? DOB { get; set; }
        /// <summary>
        /// ResidencyStatus of User
        /// </summary>
        public ResidencyStatus? ResidencyStatus { get; set; }
        /// <summary>
        /// User self declared credit score in range eg. 100-250.
        /// </summary>
        public string SelfDeclaredCreditScore { get; set; }
        /// <summary>
        /// If user has any bankruptcy in the past years.
        /// </summary>
        public bool? HasBankruptcySelfDeclared { get; set; }
        /// <summary>
        /// If user has any jugements in the past months.
        /// </summary>
        public bool? HasAnyJudgementsSelfDeclared { get; set; }
        #endregion
    }
}
