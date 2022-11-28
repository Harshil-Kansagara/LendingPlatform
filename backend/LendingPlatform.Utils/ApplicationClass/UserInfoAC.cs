using System;

namespace LendingPlatform.Utils.ApplicationClass
{
    public class UserInfoAC
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string SSN { get; set; }

        public DateTime DOB { get; set; }

        public AddressAC Address { get; set; }

        public string PhoneNumber { get; set; }

        public string ConsumerCreditReportResponse { get; set; }
        public decimal? Score { get; set; }
        public bool Bankruptcy { get; set; }
    }
}
