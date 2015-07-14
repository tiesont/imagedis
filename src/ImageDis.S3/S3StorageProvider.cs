using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using ImageDis;
using System.IO;
using System.Threading.Tasks;

namespace ImageDis.S3
{
    public class S3StorageProvider : IStorageProvider
    {
        private readonly string _awsAccessKeyId;
        private readonly string _awsSecretAccessKey;
        private readonly RegionEndpoint _region;
        private readonly string _bucketName;
        private readonly string _imageRedirect;

        public S3StorageProvider(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region, string bucketName, string imageRedirect)
        {
            _awsAccessKeyId = awsAccessKeyId;
            _awsSecretAccessKey = awsSecretAccessKey;
            _region = region;
            _bucketName = bucketName;
            _imageRedirect = imageRedirect;
        }

        public async Task<bool> CheckIfKeyExists(string key)
        {
            try
            {
                using (var client = new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, _region))
                {
                    await client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                    {
                        BucketName = _bucketName,
                        Key = key
                    });
                    return true;
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode == "NotFound")
                    return false;
                throw;
            }
        }

        public async Task SaveFile(string key, string contentType, Stream stream)
        {
            using (var client = new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, _region))
            {
                await client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    ContentType = contentType,
                    InputStream = stream,
                    CannedACL = S3CannedACL.PublicRead
                });
            }
        }

        public Task<string> GetRedirectPath(string key)
        {
            return Task.FromResult(_imageRedirect + key);
        }

        public async Task<ImageDisFile> GetFile(string key)
        {
            using (var client = new AmazonS3Client(_awsAccessKeyId, _awsSecretAccessKey, _region))
            {
                var obj = await client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                });

                var stream = new MemoryStream();
                await obj.ResponseStream.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                
                return new ImageDisFile
                {
                    ContentType = obj.Headers.ContentType,
                    Stream = stream
                };
            }
        }
    }
}
