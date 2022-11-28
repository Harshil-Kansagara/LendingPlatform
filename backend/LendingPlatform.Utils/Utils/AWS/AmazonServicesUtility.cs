using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class AmazonServicesUtility : IAmazonServicesUtility
    {
        #region Private variables
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;
        private readonly string _awsAccessKeyId;
        private readonly string _awsSecretAccessKey;
        private readonly string _awsRegion;
        private readonly int _uploadPreSignedUrlExpireTime;
        private readonly int _getPreSignedUrlExpireTime;

        #endregion

        public AmazonServicesUtility(IConfiguration configuration)
        {
            _configuration = configuration;
            _bucketName = _configuration.GetValue<string>("Aws:AwsBucketName");
            _awsAccessKeyId = _configuration.GetValue<string>("Aws:AwsSecretKey");
            _awsSecretAccessKey = _configuration.GetValue<string>("Aws:AwsSecretAccessKey");
            _awsRegion = _configuration.GetValue<string>("Aws:AwsRegion");
            _uploadPreSignedUrlExpireTime = _configuration.GetValue<int>("Aws:AwsUploadPreSignedUrlExpireTimeInMinutes");
            _getPreSignedUrlExpireTime = _configuration.GetValue<int>("Aws:AwsGetPreSignedUrlExpireTimeInMinutes");
        }

        #region Public Methods
        /// <summary>
        /// Deletes object from amazon s3
        /// </summary>
        /// <param name="objectKeys">Contains the list of object key</param>
        public async Task DeleteObjectsAsync(List<string> objectKeys)
        {
            using (var client = new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, RegionEndpoint.GetBySystemName(_awsRegion)))
            {
                var keysAndVersions = GetKeyVersions(objectKeys);

                var request = new DeleteObjectsRequest
                {
                    BucketName = _bucketName,
                    Objects = keysAndVersions
                };

                await client.DeleteObjectsAsync(request);
            }
        }

        /// <summary>
        /// To get Presigned url for the bucket with given object.
        /// </summary>
        /// <param name="objectKey">Object key of the object for which URL need to be generated.</param>
        /// <returns>Pre Signed Url</returns>
        public string GetPreSignedURL(string objectKey)
        {
            using (var client = new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, RegionEndpoint.GetBySystemName(_awsRegion)))
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    Expires = DateTime.UtcNow.AddMinutes(_getPreSignedUrlExpireTime)
                };
                return client.GetPreSignedURL(request);
            }
        }

        /// <summary>
        /// To get upload presigned url for the bucket with given object.
        /// </summary>
        /// <param name="objectKey">Object key of the object for which URL need to be generated.</param>
        /// <returns>Upload Pre Signed Url</returns>
        public string GetUploadPreSignedURL(string objectKey)
        {
            using (var client = new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, RegionEndpoint.GetBySystemName(_awsRegion)))
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddMinutes(_uploadPreSignedUrlExpireTime),
                    ContentType = StringConstant.HttpHeaderAcceptDefaultType,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };
                return client.GetPreSignedURL(request);
            }
        }

        /// <summary>
        /// Copies object from one object key to another on amazon s3
        /// </summary>
        /// <param name="sourceObjectKey">sourceObjectKey</param>
        /// <param name="destinationObjectKey">destinationObjectKey</param>
        public void CopyObject(string sourceObjectKey, string destinationObjectKey)
        {
            using (var client = new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, RegionEndpoint.GetBySystemName(_awsRegion)))
            {

                var request = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = sourceObjectKey,
                    DestinationBucket = _bucketName,
                    DestinationKey = destinationObjectKey,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };

                client.CopyObjectAsync(request);
            }
        }

        /// <summary>
        /// Trigger prepare finances queue async
        /// </summary>
        /// <param name="serializedQueueMessage"></param>
        /// <returns></returns>
        public async Task TriggerQueueAsync(string serializedQueueMessage)
        {
            using (var client = new AmazonSQSClient(_awsAccessKeyId, _awsSecretAccessKey, RegionEndpoint.GetBySystemName(_configuration.GetValue<string>("Aws:AwsPrepareFinanceRegion"))))
            {
                var request = new GetQueueUrlRequest
                {
                    QueueName = _configuration.GetValue<string>("Aws:AwsPrepareFinanceQueueName"),
                    QueueOwnerAWSAccountId = _configuration.GetValue<string>("Aws:AwsPrepareFinanceQueueNameAccountId")
                };

                var queueUrlResponse = await client.GetQueueUrlAsync(request);
                var triggerResponse = await client.SendMessageAsync(queueUrlResponse.QueueUrl, serializedQueueMessage);
                if (triggerResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException(triggerResponse.MessageId);
                }

            }

        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Method is to convert the path of document into key version form for multiple deletion of document at one time
        /// </summary>
        /// <param name="paths"> List of path </param>
        /// <returns>List of key and version</returns>
        private List<KeyVersion> GetKeyVersions(List<string> paths)
        {
            List<KeyVersion> keys = new List<KeyVersion>();
            foreach (var path in paths)
            {
                keys.Add(new KeyVersion()
                {
                    Key = path
                });
            }
            return keys;
        }

        #endregion
    }
}
