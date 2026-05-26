namespace Domain.Entities
{
    public class BlockPhoto
    {
        public int Id { get; set; }

        public int BlockId { get; set; }

        public string ObjectName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;

        public long Size { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Block? Block { get; set; }
    }
}