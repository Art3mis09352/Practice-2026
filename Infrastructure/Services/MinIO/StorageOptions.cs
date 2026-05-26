namespace Infrastructure.Services.MinIO
{
    public class StorageOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Bucket { get; set; } = string.Empty;
        public bool UseSsl { get; set; }
        public string PublicBaseUrl { get; set; } = string.Empty;
    }
}