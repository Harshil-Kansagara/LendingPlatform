using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class ConsentAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for user object
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Unique identifier for consent statement
        /// </summary>
        public string ConsentId { get; set; }
        /// <summary>
        /// Consent statement
        /// </summary>
        public string ConsentText { get; set; }
        /// <summary>
        /// Is consent given?
        /// </summary>
        public bool IsConsentGiven { get; set; }
        #endregion
    }
}
