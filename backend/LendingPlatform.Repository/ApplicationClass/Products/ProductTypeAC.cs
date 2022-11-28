using System;

namespace LendingPlatform.Repository.ApplicationClass.Products
{
    public class ProductTypeAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the product type object.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Name of the product type
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Order number for the sequence
        /// </summary>
        public int Order { get; set; }
        #endregion
    }
}
