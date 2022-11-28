using LendingPlatform.DomainModel.Enums;
using System;

namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class AdditionalDocumentTypeAC
    {
        /// <summary>
        /// Unique identifier for AdditionalDocumentTypeAC object
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type of document
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Resource, the document type is related with
        /// </summary>
        public ResourceType DocumentTypeFor { get; set; }
    }
}
