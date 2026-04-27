using MyApi.Domain.DomainServices.FileUpload;
using MyApi.HttpApi.Hubs;

namespace MyApi.HttpApi.Extensions;

public static class FileUploadExtension
{
    public static void ConfigureUpload(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<UploadOptions>()
            .Bind(configuration.GetSection("UploadOptions"))
            .Validate(options => options.Client.MaxConcurrentUploads >= 1, "MaxConcurrentUploads must be at least 1.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SignedUrl.SecretKey), "SignedUrl.SecretKey is required.")
            .Validate(options => options.SignedUrl.SecretKey.Length >= 32, "SignedUrl.SecretKey must be at least 32 characters.")
            .Validate(options => options.SignedUrl.PreviewTtlMinutes >= 0, "PreviewTtlMinutes must be greater than or equal to 0.")
            .Validate(options => options.SignedUrl.DownloadTtlMinutes >= 0, "DownloadTtlMinutes must be greater than or equal to 0.")
            .ValidateOnStart();
        services.AddSingleton<IStorageProvider, LocalStorageProvider>();
        services.AddSingleton<FileValidationService>();
        services.AddSingleton<FileAccessSignatureService>();
        services.AddHostedService<CleanupWorker>();
        services.AddSignalR();
    }

    public static void UseUploadHub(this WebApplication app)
    {
        app.MapHub<UploadHub>("/hub/upload"); // SignalR
    }
}
