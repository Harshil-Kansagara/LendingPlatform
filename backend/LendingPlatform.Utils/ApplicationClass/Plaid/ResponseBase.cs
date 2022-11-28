using System.Net;

namespace LendingPlatform.Utils.ApplicationClass.Plaid
{
    /// <summary>
    /// Provides common members for all Plaid API responses.
    /// </summary>
    public abstract class ResponseBaseAC
    {
        /// <summary>
        /// Gets the http status code.
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; internal set; }
    }
}