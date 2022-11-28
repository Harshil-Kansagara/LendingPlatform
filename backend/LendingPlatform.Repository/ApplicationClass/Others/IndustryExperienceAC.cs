using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class IndustryExperienceAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for the industry experience object
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Industry experience
        /// </summary>
        public string Experience { get; set; }
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
