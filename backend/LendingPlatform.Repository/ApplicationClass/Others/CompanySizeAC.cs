using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class CompanySizeAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the company size object
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Size of the company
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// Order number for sequence
        /// </summary>
        public int? Order { get; set; }
        /// <summary>
        /// Is option enabled
        /// </summary>
        public bool IsEnabled { get; set; }
        #endregion
    }
}
