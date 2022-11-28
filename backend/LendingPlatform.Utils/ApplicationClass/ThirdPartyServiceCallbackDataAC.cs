using System;
using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass
{
    public class ThirdPartyServiceCallbackDataAC
    {
        /// <summary>
        /// Name of third party service, ex Quickbooks, Xero etc
        /// </summary>
        public string ThirdPartyServiceName { get; set; }

        /// <summary>
        /// State token used to prevent CSRF attacks
        /// </summary>
        public string CSRFToken { get; set; }

        /// <summary>
        /// Quickbooks specific parameter that identifies the connected realm
        /// </summary>
        public string RealmId { get; set; }

        /// <summary>
        /// Quickbooks specific parameter that saves authorization code
        /// </summary>
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// Entity Id from which the API got fired (matched with CSRFToken)
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// JSON that has meta data configuration for third party services
        /// </summary>
        public string ConfigurationJson { get; set; }

        /// <summary>
        /// Deserialized ConfigurationJSON
        /// </summary>
        public List<ThirdPartyConfigurationAC> Configuration { get; set; }

        /// <summary>
        /// Xero specific parameter that saves start time period
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Xero specific parameter that saves end time period
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Financial Statements that are requested
        /// </summary>
        public List<FinancialStatementsAC> ReportListToFetch { get; set; }

        /// <summary>
        /// Token that is received after connecting with third party services
        /// </summary>
        public string BearerToken { get; set; }

        /// <summary>
        /// Indicates last N years for which finances are fetched
        /// </summary>
        public int LastYears { get; set; }

        /// <summary>
        /// Xero specific parameter to get tenantid
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// List of periods for which the finances are to be fetched
        /// </summary>
        public List<string> PeriodList { get; set; }

    }
}
