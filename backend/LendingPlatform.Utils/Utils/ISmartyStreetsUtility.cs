using LendingPlatform.Utils.ApplicationClass;

namespace LendingPlatform.Utils.Utils
{
    public interface ISmartyStreetsUtility
    {
        /// <summary>
        /// Method validates the address and returns it with some extra fields.
        /// </summary>
        /// <param name="address">Autocompleted address</param>
        /// <param name="smartyStreetsConfigurationJson">SmartyStreets ConfigurationJson</param>
        /// <returns>USStreetApi type address</returns>
        SmartyStreets.USStreetApi.Candidate GetValidatedAddress(AddressAC address, string smartyStreetsConfigurationJson);
    }
}
