using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceConstantAC
    {
        /// <summary>
        /// Unique identifier of constant object
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Constant name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of options available for constant (dropdown)
        /// </summary>
        public List<PersonalFinanceConstantOptionAC> Options { get; set; }
    }
}
