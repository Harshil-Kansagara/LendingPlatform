using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class BusinessAgeAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the company age object
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Age of the company
        /// </summary>
        public string Age { get; set; }
        /// <summary>
        /// Order number for sequence
        /// </summary>
        public int? Order { get; set; }
        #endregion
    }
}
