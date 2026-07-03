namespace api.Settings;

public interface IMinioSettings
{
    string Endpoint { get; set; }
    string AccessKey { get; set; }
    string SecretKey { get; set; }
    string BucketName { get; set; }
    bool UseSSL { get; set; }
}