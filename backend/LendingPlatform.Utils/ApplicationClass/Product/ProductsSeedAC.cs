
using System;
using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.Product
{
    public class ProductsSeedAC
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartDateAvailability { get; set; }

        public DateTime EndDateAvailibility { get; set; }

        public bool IsEnabled { get; set; }
        public List<DescriptionPointSeedAC> DescriptionPoints { get; set; }
    }
}
