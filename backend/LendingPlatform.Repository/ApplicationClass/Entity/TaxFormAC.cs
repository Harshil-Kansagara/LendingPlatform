using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class TaxFormAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier of tax form object
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tax form name
        /// </summary>
        public string Name { get; set; }
        #endregion

    }
}