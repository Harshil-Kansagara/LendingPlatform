using System;


namespace LendingPlatform.Utils.ApplicationClass.Product
{
    public class ProductRangeTypeMappingAC
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public string RangeTypeName { get; set; }

        public decimal Minimum { get; set; }

        public decimal Maximum { get; set; }
    }
}
