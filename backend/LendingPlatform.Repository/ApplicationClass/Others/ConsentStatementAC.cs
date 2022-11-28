using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class ConsentStatementAC
    {
        /// <summary>
        /// Unique identifier for the consent statement.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Text of the consent statement.
        /// </summary>
        public string ConsentText { get; set; }
        /// <summary>
        /// Is consent enabled or not.
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
