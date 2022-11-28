using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class TaxAC
    {
        #region Public Properties

        /// <summary>
        /// Unique identifier for the entity tax form object
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Entity tax yearly mapping details
        /// </summary>
        public EntityTaxAccountAC EntityTaxAccount { get; set; }

        /// <summary>
        /// Datetime when taxes file was save
        /// </summary>
        public DateTime CreationDateTime { get; set; }
        #endregion
    }
}
