using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class CompanyStructureAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the company type object.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Company Type.
        /// </summary>
        public string Structure { get; set; }
        /// <summary>
        /// Order number for sequence.
        /// </summary>
        public int? Order { get; set; }
        #endregion
    }
}
