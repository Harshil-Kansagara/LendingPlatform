using System;

namespace LendingPlatform.Utils.ApplicationClass.Product
{
    public class ProductPercentageSuitabilityAC
    {
        public Guid ProductId { get; set; }
        public decimal PercentageSuitability { get; set; }

        public bool IsRecommended { get; set; }
    }
}
