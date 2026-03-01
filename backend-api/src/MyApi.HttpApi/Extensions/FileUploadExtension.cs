using MyApi.Domain.DomainServices.FileUpload;
using MyApi.HttpApi.Hubs;

namespace MyApi.HttpApi.Extensions;

public static class FileUploadExtension
{
    public static void ConfigureUpload(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IStorageProvider, LocalStorageProvider>();
        services.AddSignalR();
    }

    public static void UseUploadHub(this WebApplication app)
    {
        app.MapHub<UploadHub>("/hub/upload"); // SignalR
    }
}