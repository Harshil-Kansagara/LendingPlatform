
namespace LendingPlatform.Repository.ApplicationClass.AppSettings
{
    public class AwsSettings
    {
        #region Public Properties
        /// <summary>
        /// Base url of the document
        /// </summary>
        public string BaseUrl { get; set; }
        /// <summary>
        /// Upload Pre signed url of the document
        /// </summary>
        public string UploadPreSignedUrl { get; set; }
        /// <summary>
        /// Pre signed url of the document
        /// </summary>
        public string PreSignedUrl { get; set; }
        #endregion
    }
}
