using LendingPlatform.DomainModel.Enums;
using LendingPlatform.Utils.ApplicationClass;
using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceAttributeAC
    {
        /// <summary>
        /// Unique identifier for attribute object
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Attribute (question) text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Attribute order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Attribute's response (answer)
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Boolean type attribute's response (answer) (Used in UI, don't remove it)
        /// </summary>
        public bool? BooleanAnswer { get; set; }

        /// <summary>
        /// Constant (dropdown menu) linked with attribute
        /// </summary>
        public PersonalFinanceConstantAC Constant { get; set; }

        /// <summary>
        /// Attribute response type
        /// </summary>
        public PersonalFinanceAttributeFieldType FieldType { get; set; }

        /// <summary>
        /// List of attribute's ordered child attributes
        /// </summary>
        public List<PersonalFinanceOrderedAttributeAC> ChildAttributeSets { get; set; }

#nullable enable
        /// <summary>
        /// Address type attribute's response (answer) (don't remove it, used in UI)
        /// </summary>
        public AddressAC? Address { get; set; }
#nullable disable
    }
}
