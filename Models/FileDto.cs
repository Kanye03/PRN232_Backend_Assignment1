namespace PRN232_Assignment1.Models
{
    public class FileDto
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string BucketId { get; set; } = string.Empty;
    }
}
