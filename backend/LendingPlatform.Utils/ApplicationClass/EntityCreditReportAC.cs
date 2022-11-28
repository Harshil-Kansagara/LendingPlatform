using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass
{
    public class EntityCreditReportAC
    {
        public string CompanyBIN { get; set; }

        public PremierProfilesResponseAC PremierProfilesResponse { get; set; }

        public List<UserInfoAC> Users { get; set; }
    }
}
