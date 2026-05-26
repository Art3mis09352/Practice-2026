namespace Infrastructure.Services.MinIO
{
    public interface IObjectStorageService
    {
        Task<string> UploadBlockPhotoAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
        Task DeleteObjectAsync(string objectName, CancellationToken cancellationToken = default);
        string GetPublicUrl(string objectName);
    }
}