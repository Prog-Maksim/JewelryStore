using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;

namespace JewelryStoreBackend.Services;

public class S3Service
{
    private readonly AmazonS3Client _s3Client;
    private readonly string _bucketName = "jewelry";

    public S3Service()
    {
        var accessKey = Environment.GetEnvironmentVariable("ACCESS_KEY");
        var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
        
        var endpoint = new Uri("https://storage.yandexcloud.net"); 
        
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var config = new AmazonS3Config
        {
            ServiceURL = endpoint.ToString(),
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(credentials, config);
    }
    
    public async Task DeleteFileFromS3Async(string key)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3Client.DeleteObjectAsync(deleteRequest);
            
        if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
            throw new FileNotFoundException("Файл не был найден");

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            throw new FileLoadException("Произошла ошибка при удаление файла");
    }
    
    public async Task<string> UploadFileToS3Async(IFormFile file, string key)
    {
        using (var stream = file.OpenReadStream())
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream
            };

            var response = await _s3Client.PutObjectAsync(putRequest);
            
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                return key;
        }

        throw new FileLoadException();
    }
}