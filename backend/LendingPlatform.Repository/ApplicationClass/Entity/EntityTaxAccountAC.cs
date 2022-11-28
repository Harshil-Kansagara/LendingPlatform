using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class EntityTaxAccountAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the entity tax yearly mapping object
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Time period of tax account
        /// </summary>
        public string Period { get; set; }
        /// <summary>
        /// Document details
        /// </summary>
        public DocumentAC Document { get; set; }
        #endregion
    }
}