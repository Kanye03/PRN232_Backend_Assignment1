using SupabaseClient = Supabase.Client;
using StorageFileOptions = Supabase.Storage.FileOptions;

namespace PRN232_Assignment1.Services
{
    public class SupabaseStorageService
    {
        private readonly SupabaseClient _supabaseClient;
        private readonly string _bucketName;

        public SupabaseStorageService(SupabaseClient supabaseClient, string bucketName = "image")
        {
            _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
            _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
        }

        /// <summary>
        /// Sinh tên file ngẫu nhiên.
        /// </summary>
        private static string GenerateUniqueFileName(string originalFileName)
        {
            var ext = Path.GetExtension(originalFileName) ?? string.Empty;
            var name = Guid.NewGuid().ToString("N");
            return $"{name}{ext}";
        }

        /// <summary>
        /// Upload IFormFile lên Supabase Storage và trả về URL public
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile imageFile)
        {
            if (imageFile == null) throw new ArgumentNullException(nameof(imageFile));

            // Sinh tên file mới
            var fileName = GenerateUniqueFileName(imageFile.FileName);

            // Đọc stream thành byte[]
            await using var ms = new MemoryStream();
            if (imageFile.OpenReadStream().CanSeek)
                imageFile.OpenReadStream().Position = 0;
            await imageFile.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // Lấy bucket
            var bucket = _supabaseClient.Storage.From(_bucketName);

            // Upload file
            await bucket.Upload(bytes, fileName, new StorageFileOptions
            {
                ContentType = imageFile.ContentType ?? "application/octet-stream",
                Upsert = false // không ghi đè
            });

            // Trả về URL public
            return bucket.GetPublicUrl(fileName);
        }
    }
}
