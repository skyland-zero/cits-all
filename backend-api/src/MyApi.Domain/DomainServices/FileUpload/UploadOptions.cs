using System.ComponentModel.DataAnnotations;

namespace MyApi.Domain.DomainServices.FileUpload;

public class UploadOptions
{
    public UploadClientOptions Client { get; set; } = new();

    public SignedUrlOptions SignedUrl { get; set; } = new();

    public UploadSignalROptions SignalR { get; set; } = new();
}

public class UploadClientOptions
{
    [Range(1, int.MaxValue)]
    public int MaxConcurrentUploads { get; set; } = 3;
}

public class SignedUrlOptions
{
    [MinLength(32)]
    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int PreviewTtlMinutes { get; set; } = 10;

    [Range(0, int.MaxValue)]
    public int DownloadTtlMinutes { get; set; } = 10;
}

public class UploadSignalROptions
{
    public bool Enabled { get; set; } = true;
}
