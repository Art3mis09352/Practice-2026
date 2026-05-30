namespace Infrastructure.Services.MinIO
{
    public interface IObjectStorageService
    {
        Task<string> UploadBlockPhotoAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);

        Task DeleteBlockPhotoAsync(
            string objectName,
            CancellationToken cancellationToken = default);

        string GetBlockPhotoPublicUrl(string objectName);

        Task<string> UploadBlockDocumentAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);

        Task DeleteBlockDocumentAsync(
            string objectName,
            CancellationToken cancellationToken = default);

        Task<(string Url, DateTime ExpiresAtUtc)> GetBlockDocumentDownloadUrlAsync(
            string objectName,
            TimeSpan lifetime,
            CancellationToken cancellationToken = default);
    }
}