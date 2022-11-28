
using LendingPlatform.Repository.ApplicationClass.Entity;
using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class AdditionalDocumentAC
    {
        /// <summary>
        /// Unique identifier for AdditionalDocumentAC object
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Document type's details
        /// </summary>
        public AdditionalDocumentTypeAC DocumentType { get; set; }

        /// <summary>
        /// Document's details
        /// </summary>
        public DocumentAC Document { get; set; }
    }
}
