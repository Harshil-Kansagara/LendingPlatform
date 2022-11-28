using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceCategoryAC
    {
        /// <summary>
        /// Unique identifier for category object
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Category order
        /// </summary>
        public int Order { get; set; }

#nullable enable

        /// <summary>
        /// Parent attribute (of parent category) object 
        /// </summary>
        public PersonalFinanceAttributeAC? ParentAttribute { get; set; }

#nullable disable

        /// <summary>
        /// List of attributes mapped with category
        /// </summary>
        public List<PersonalFinanceAttributeAC> Attributes { get; set; }

        /// <summary>
        /// List of child categories
        /// </summary>
        public List<PersonalFinanceCategoryAC> ChildCategories { get; set; }
    }
}
