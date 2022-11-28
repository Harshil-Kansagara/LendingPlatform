using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceOrderedAttributeAC
    {
        /// <summary>
        /// Attributes' set order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// List of child attributes
        /// </summary>
        public List<PersonalFinanceAttributeAC> ChildAttributes { get; set; }
    }
}
