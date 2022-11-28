using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.Equifax
{
    public class ConsumersAC
    {
        #region Properties
        public List<EquifaxNameAC> Name { get; set; }

        public List<SocialNumAC> SocialNum { get; set; }
        public string DateOfBirth { get; set; }
        public List<EquifaxAddressAC> Addresses { get; set; }
        public List<PhoneNumbersAC> PhoneNumbers { get; set; }
        #endregion
    }
}
