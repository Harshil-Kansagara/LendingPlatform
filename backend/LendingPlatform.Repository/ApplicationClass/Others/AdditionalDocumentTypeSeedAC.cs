
namespace LendingPlatform.Repository.ApplicationClass.Others
{
    public class AdditionalDocumentTypeSeedAC
    {
        /// <summary>
        /// Unique identifier for AdditionalDocumentTypeSeedAC object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Type of document
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Resource, the document type is related with
        /// </summary>
        public string DocumentTypeFor { get; set; }

        /// <summary>
        /// Is this type enabled or not
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
