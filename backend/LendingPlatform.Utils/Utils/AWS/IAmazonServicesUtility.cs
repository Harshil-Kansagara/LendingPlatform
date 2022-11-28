using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public interface IAmazonServicesUtility
    {
        /// <summary>
        /// Deletes object from amazon s3
        /// </summary>
        /// <param name="objectKeys">Contains the list of object key</param>
        Task DeleteObjectsAsync(List<string> objectKeys);

        /// <summary>
        /// To get Presigned url for the bucket with given object.
        /// </summary>
        /// <param name="objectKey">Object key of the object for which URL need to be generated.</param>
        /// <returns>Pre Signed Url</returns>
        string GetPreSignedURL(string objectKey);

        /// <summary>
        /// Copies object from one object key to another on amazon s3
        /// </summary>
        /// <param name="sourceObjectKey">sourceObjectKey</param>
        /// <param name="destinationObjectKey">destinationObjectKey</param>
        void CopyObject(string sourceObjectKey, string destinationObjectKey);

        /// <summary>
        /// Trigger prepare finances queue async
        /// </summary>
        /// <param name="serializedQueueMessage"></param>
        /// <returns></returns>
        Task TriggerQueueAsync(string serializedQueueMessage);

        /// <summary>
        /// To get upload presigned url for the bucket with given object.
        /// </summary>
        /// <param name="objectKey">Object key of the object for which URL need to be generated.</param>
        /// <returns>Pre Signed Url</returns>
        string GetUploadPreSignedURL(string objectKey);
    }
}
