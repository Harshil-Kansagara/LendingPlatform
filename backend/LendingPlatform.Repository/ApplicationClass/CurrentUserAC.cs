using System;

namespace LendingPlatform.Repository.ApplicationClass
{
    public class CurrentUserAC
    {
        #region Properties
        /// <summary>
        /// Unique user identifier.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// User name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// User email address.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// If user come from bank then true else false.
        /// </summary>
        public bool IsBankUser { get; set; }
        /// <summary>
        /// User's local IP address.
        /// </summary>
        public string IpAddress { get; set; }
        #endregion
    }
}
