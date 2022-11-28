using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class TokenAC
    {
        #region Public Properties
        /// <summary>
        /// Authorization code return from service provider
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// RealmId return from service provider
        /// </summary>
        public string RealmId { get; set; }
        /// <summary>
        /// State of the request
        /// </summary>
        public Guid State { get; set; }
        /// <summary>
        /// Unique identifier for the loan application object
        /// </summary>
        public Guid LoanApplicationId { get; set; }
        #endregion
    }
}
