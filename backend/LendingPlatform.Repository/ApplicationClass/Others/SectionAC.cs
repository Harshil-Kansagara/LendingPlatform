using System;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class SectionAC
    {
        /// <summary>
        /// Unique identifier of the section
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Section name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Section order
        /// </summary>
        public decimal Order { get; set; }

        /// <summary>
        /// SectionId - (For internal use)
        /// </summary>
        public int SectionId { get; set; }

        /// <summary>
        /// Parent Section - (For internal use)
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Is Added - (For internal use)
        /// </summary>
        public bool IsAdded { get; set; }

        /// <summary>
        /// Is Updated - (For internal use)
        /// </summary>
        public bool IsUpdated { get; set; }

        /// <summary>
        /// Is Enabled - (For internal use)
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Parent Section object
        /// </summary>
        public List<SectionAC> ChildSection { get; set; }
    }
}
