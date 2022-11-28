using System;

namespace LendingPlatform.Utils.ApplicationClass.Product
{
    public class ProductSubPurposeMappingAC
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public string SubPurposeName { get; set; }
    }
}