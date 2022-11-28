using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class DocumentAC
    {
        #region Public Properties
        /// <summary>
        /// Unique identifier for document object
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// Name of the document
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Path of the upload document
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Path for the download document
        /// </summary>
        public string DownloadPath { get; set; }
        #endregion
    }
}
