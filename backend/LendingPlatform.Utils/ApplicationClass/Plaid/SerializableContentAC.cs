using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LendingPlatform.Utils.ApplicationClass.Plaid
{
    /// <summary>
    /// Provide an method for converting an object to a JSON string.
    /// </summary>
    public abstract class SerializableContentAC
    {
        /// <summary>
        /// The null value handling.
        /// </summary>
        protected NullValueHandling NullValueHandling = NullValueHandling.Ignore;

        /// <summary>
        /// Returns a string that represents the current object in JSON format.
        /// </summary>
        /// <returns>System.String.</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                DateFormatString = "yyyy-MM-dd",
                NullValueHandling = this.NullValueHandling,
#if DEBUG
                Formatting = Formatting.Indented
#else
                Formatting = Formatting.None
#endif
            });
        }
    }
}