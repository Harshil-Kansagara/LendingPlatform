using LendingPlatform.Repository.ApplicationClass.Products;
using LendingPlatform.Utils.ApplicationClass;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LendingPlatform.Repository.ApplicationClass.Applications
{
    public class RecommendedProductAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for recommended product.
        /// </summary>
        [Required]
        public Guid Id { get; set; }
        /// <summary>
        /// Name of the recommended product.
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Description of the recommended product.
        /// </summary>
        [Required]
        public string Description { get; set; }
        /// <summary>
        /// Start date of the recommended product.
        /// </summary>
        [Required]
        public DateTime ProductStartDate { get; set; }
        /// <summary>
        /// End date of the recommended product.
        /// </summary>
        [Required]
        public DateTime ProductEndDate { get; set; }

        /// <summary>
        /// Percentage of product suitable for your business
        /// </summary>
        public decimal? BusinessPercentageSuitability { get; set; }

        /// <summary>
        /// Product recommended checked
        /// </summary>
        public bool IsProductRecommended { get; set; }

        /// <summary>
        /// The loan product's detail.
        /// </summary>
        public ProductDetailsAC ProductDetails { get; set; }

        /// <summary>
        /// Product amount range
        /// </summary>
        public string ProductAmountRange { get; set; }

        /// <summary>
        /// Product period range
        /// </summary>
        public string ProductPeriodRange { get; set; }
        /// <summary>
        /// Is previous product matched with current product.
        /// </summary>
        public bool? IsPreviousProductMatched { get; set; }
        /// <summary>
        /// List of description points for product.
        /// </summary>
        public List<DescriptionPointSeedAC> DescriptionPoints { get; set; }
        #endregion
    }
}
