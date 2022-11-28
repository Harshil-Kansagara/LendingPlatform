using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class CompanyStructuresIdAC
    {
        public Guid ProprietorshipId { get; set; }
        public Guid PartnershipId { get; set; }
        public Guid LimitedLiabilityCompanyId { get; set; }
        public Guid CCorporationId { get; set; }
        public Guid SCorporationId { get; set; }
    }
}
