namespace Application.DTO.Common
{
    public class DownloadUrlResponseDTO
    {
        public string Url { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}