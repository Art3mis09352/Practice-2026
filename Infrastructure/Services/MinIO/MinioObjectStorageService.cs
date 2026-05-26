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

        public MinioObjectStorageService(IMinioClient minioClient, IOptions<StorageOptions> options)
        {
            _minioClient = minioClient;
            _options = options.Value;
        }

        public async Task<string> UploadBlockPhotoAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(fileName);
            var objectName = $"blocks/{Guid.NewGuid():N}{extension}";

            await EnsureBucketExistsAsync(cancellationToken);

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_options.Bucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType), cancellationToken);

            return objectName;
        }

        public async Task DeleteObjectAsync(string objectName, CancellationToken cancellationToken = default)
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_options.Bucket)
                .WithObject(objectName), cancellationToken);
        }

        public string GetPublicUrl(string objectName)
        {
            return $"{_options.PublicBaseUrl.TrimEnd('/')}/{objectName}";
        }

        private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
        {
            var exists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_options.Bucket), cancellationToken);

            if (!exists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_options.Bucket), cancellationToken);
            }
        }
    }
}