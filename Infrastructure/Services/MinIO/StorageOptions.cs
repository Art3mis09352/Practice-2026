namespace Infrastructure.Services.MinIO
{
    public class StorageOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool UseSsl { get; set; }

        public string PublicPhotosBucket { get; set; } = string.Empty;
        public string PrivateDocumentsBucket { get; set; } = string.Empty;

        public string PublicPhotosBaseUrl { get; set; } = string.Empty;
        public string? PublicEndpoint { get; set; }
        public bool PublicUseSsl { get; set; }
    }
}