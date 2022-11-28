using System.ComponentModel;

namespace LendingPlatform.DomainModel.Enums
{
    public enum ResidencyStatus
    {
        [Description("US Citizen")]
        USCitizen,
        [Description("Permanent Resident")]
        USPermanentResident,
        [Description("Non Resident")]
        NonResident
    }
}
