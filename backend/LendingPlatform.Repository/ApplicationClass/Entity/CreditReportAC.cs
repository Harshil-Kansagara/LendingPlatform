using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class CreditReportAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for credit report object
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// JSON string of credit report
        /// </summary>
        public string CreditReportJson { get; set; }
        /// <summary>
        /// Unique identifier for integarted service configuration
        /// </summary>
        public Guid IntegratedServiceConfigurationId { get; set; }
        /// <summary>
        /// Integarted service configuration name
        /// </summary>
        public string IntegratedServiceConfigurationName { get; set; }
        #endregion
    }
}
