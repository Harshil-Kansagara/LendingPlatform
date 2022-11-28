using LendingPlatform.DomainModel.Models.EntityInfo;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class PersonalFinanceDBFieldsAC
    {
        /// <summary>
        /// List of categories, given in JSON, mapped with model class type
        /// </summary>
        public List<PersonalFinanceCategory> Categories { get; set; }

        /// <summary>
        /// List of attributes, given in JSON, mapped with model class type
        /// </summary>
        public List<PersonalFinanceAttribute> Attributes { get; set; }

        /// <summary>
        /// List of constants, given in JSON, mapped with model class type
        /// </summary>
        public List<PersonalFinanceConstant> Constants { get; set; }
    }
}
