using Amazon.S3;
using Amazon.S3.Model;

namespace Backend.Utiliy
{
    public class R2Service
    {
        private readonly IAmazonS3 _s3;
        private readonly R2Settings _settings;

        public R2Service(IAmazonS3 s3, R2Settings settings)
        {
            _s3 = s3;
            _settings = settings;
        }


        public async Task DeleteAsync(string key)
        {
            await _s3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key
            });
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty.");

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

            // 🔥 Save file to temporary path first
            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fileName,
                FilePath = tempFilePath,
                ContentType = file.ContentType,
                DisablePayloadSigning = true 
            };

            await _s3.PutObjectAsync(request);

            // delete temp file
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);

            return $"{_settings.PublicUrl}/{fileName}";
        }
    }
}