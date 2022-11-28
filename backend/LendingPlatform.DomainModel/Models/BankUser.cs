using LendingPlatform.Utils.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingPlatform.DomainModel.Models
{
    public class BankUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [RegularExpression(StringConstant.PhoneRegularExpression,
            ErrorMessage = StringConstant.NotValidPhone)]
        public string Phone { get; set; }
    }
}
