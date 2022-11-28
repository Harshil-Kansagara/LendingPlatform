using LendingPlatform.Repository.ApplicationClass.Entity;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Applications
{
    public class ApplicationAC
    {
        #region Public Properties
        /// <summary>
        /// The application's basic detail
        /// </summary>
        public ApplicationBasicDetailAC BasicDetails { get; set; }
        /// <summary>
        /// List of all the borrowing entities details.
        /// </summary>
        public List<EntityAC> BorrowingEntities { get; set; }
        /// <summary>
        /// The selected product's detail.
        /// </summary>
        public RecommendedProductAC SelectedProduct { get; set; }
        #endregion
    }
}
