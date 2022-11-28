using System;

namespace LendingPlatform.Repository.ApplicationClass.Products
{
    public class ProductSubTypeAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for product sub type object
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Name of the Product sub type
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Order number for sequence
        /// </summary>
        public int Order { get; set; }
        #endregion
    }
}
