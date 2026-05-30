using Infrastructure.Services.MinIO;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.Services.Storage
{
    public class MinioObjectStorageService : IObjectStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly StorageOptions _options;
        private readonly IMinioClient _presignClient;

        public MinioObjectStorageService(IMinioClient minioClient, IOptions<StorageOptions> options)
        {
            _minioClient = minioClient;
            _options = options.Value;
            _presignClient = string.IsNullOrWhiteSpace(_options.PublicEndpoint)
                ? minioClient
                : new MinioClient()
                    .WithEndpoint(_options.PublicEndpoint)
                    .WithCredentials(_options.AccessKey, _options.SecretKey)
                    .WithSSL(_options.PublicUseSsl)
                    .WithRegion("us-east-1")
                    .Build();
        }

        public async Task<string> UploadBlockPhotoAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(fileName);
            var objectName = $"blocks/{Guid.NewGuid():N}{extension}";

            await EnsureBucketExistsAsync(_options.PublicPhotosBucket, cancellationToken);

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_options.PublicPhotosBucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType), cancellationToken);

            return objectName;
        }

        public async Task DeleteBlockPhotoAsync(string objectName, CancellationToken cancellationToken = default)
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_options.PublicPhotosBucket)
                .WithObject(objectName), cancellationToken);
        }

        public string GetBlockPhotoPublicUrl(string objectName)
        {
            return $"{_options.PublicPhotosBaseUrl.TrimEnd('/')}/{objectName}";
        }

        public async Task<string> UploadBlockDocumentAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(fileName);
            var objectName = $"blocks-documents/{Guid.NewGuid():N}{extension}";

            await EnsureBucketExistsAsync(_options.PrivateDocumentsBucket, cancellationToken);

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_options.PrivateDocumentsBucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType), cancellationToken);

            return objectName;
        }

        public async Task DeleteBlockDocumentAsync(string objectName, CancellationToken cancellationToken = default)
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_options.PrivateDocumentsBucket)
                .WithObject(objectName), cancellationToken);
        }

        public async Task<(string Url, DateTime ExpiresAtUtc)> GetBlockDocumentDownloadUrlAsync(
            string objectName,
            TimeSpan lifetime,
            CancellationToken cancellationToken = default)
        {
            await EnsureBucketExistsAsync(_options.PrivateDocumentsBucket, cancellationToken);

            var expiresInSeconds = (int)Math.Ceiling(lifetime.TotalSeconds);
            var url = await _presignClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(_options.PrivateDocumentsBucket)
                .WithObject(objectName)
                .WithExpiry(expiresInSeconds));
            return (url, DateTime.UtcNow.AddSeconds(expiresInSeconds));
        }

        private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
        {
            var exists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucketName), cancellationToken);

            if (!exists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(bucketName), cancellationToken);
            }
        }
    }
}