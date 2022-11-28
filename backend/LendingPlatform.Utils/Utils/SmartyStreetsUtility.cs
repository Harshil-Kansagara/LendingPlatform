using LendingPlatform.Utils.ApplicationClass;
using Newtonsoft.Json;
using SmartyStreets;
using System.Collections.Generic;
using System.Linq;

namespace LendingPlatform.Utils.Utils
{
    public class SmartyStreetsUtility : ISmartyStreetsUtility
    {
        #region Constructor
        public SmartyStreetsUtility()
        {
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Method validates the address and returns it with some extra fields.
        /// </summary>
        /// <param name="address">Autocompleted address</param>
        /// <param name="smartyStreetsConfigurationJson">SmartyStreets ConfigurationJson</param>
        /// <returns>USStreetApi type address</returns>
        public SmartyStreets.USStreetApi.Candidate GetValidatedAddress(AddressAC address, string smartyStreetsConfigurationJson)
        {
            var lookup = new SmartyStreets.USStreetApi.Lookup
            {
                Street = address.StreetLine,
                City = address.City,
                State = address.StateAbbreviation,
                MatchStrategy = "strict",
                MaxCandidates = 1
            };

            var configuration = JsonConvert.DeserializeObject<List<ThirdPartyConfigurationAC>>(smartyStreetsConfigurationJson);

            SmartyStreets.USStreetApi.Client client = new ClientBuilder(configuration.First(x => x.Path == "SmartyStreets:AuthId").Value,
                configuration.First(x => x.Path == "SmartyStreets:AuthToken").Value).BuildUsStreetApiClient();

            client.Send(lookup);

            if (lookup.Result.Count > 0)
            {
                return lookup.Result.ToArray()[0];
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
