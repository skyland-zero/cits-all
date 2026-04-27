using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace MyApi.Domain.DomainServices.FileUpload;

public class FileAccessSignatureService
{
    private const string DownloadAction = "download";
    private const string PreviewAction = "preview";
    private readonly IOptionsMonitor<UploadOptions> _uploadOptions;

    public FileAccessSignatureService(IOptionsMonitor<UploadOptions> uploadOptions)
    {
        _uploadOptions = uploadOptions;
    }

    public string CreateDownloadUrl(Guid fileId, int accessVersion)
    {
        return BuildAccessUrl(fileId, DownloadAction, accessVersion);
    }

    public string CreatePreviewUrl(Guid fileId, int accessVersion)
    {
        return BuildAccessUrl(fileId, PreviewAction, accessVersion);
    }

    public bool TryValidateDownloadToken(string token, out FileAccessPayload payload)
    {
        return TryValidateToken(token, DownloadAction, out payload);
    }

    public bool TryValidatePreviewToken(string token, out FileAccessPayload payload)
    {
        return TryValidateToken(token, PreviewAction, out payload);
    }

    private string BuildAccessUrl(Guid fileId, string action, int accessVersion)
    {
        var expiresAt = GetExpiresAt(action);
        var payload = new FileAccessPayload(fileId, action, expiresAt, accessVersion);
        var serializedPayload = SerializePayload(payload);
        var payloadPart = Base64UrlEncode(Encoding.UTF8.GetBytes(serializedPayload));
        var signaturePart = Base64UrlEncode(SignPayload(serializedPayload));
        var token = $"{payloadPart}.{signaturePart}";
        return $"/api/basic/upload/upload/access/{action}?token={Uri.EscapeDataString(token)}";
    }

    private long GetExpiresAt(string action)
    {
        var ttlMinutes = action == DownloadAction
            ? _uploadOptions.CurrentValue.SignedUrl.DownloadTtlMinutes
            : _uploadOptions.CurrentValue.SignedUrl.PreviewTtlMinutes;
        if (ttlMinutes <= 0)
        {
            return 0;
        }

        return DateTimeOffset.UtcNow.AddMinutes(ttlMinutes).ToUnixTimeSeconds();
    }

    private byte[] SignPayload(string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_uploadOptions.CurrentValue.SignedUrl.SecretKey);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        return hmac.ComputeHash(payloadBytes);
    }

    private bool TryValidateToken(string token, string expectedAction, out FileAccessPayload payload)
    {
        payload = default;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        string serializedPayload;
        byte[] providedSignature;
        try
        {
            serializedPayload = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
            providedSignature = Base64UrlDecode(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        var expectedSignature = SignPayload(serializedPayload);
        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, providedSignature))
        {
            return false;
        }

        if (!TryParsePayload(serializedPayload, out payload))
        {
            return false;
        }

        if (!string.Equals(payload.Action, expectedAction, StringComparison.Ordinal))
        {
            return false;
        }

        if (payload.ExpiresAt > 0 && payload.ExpiresAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            return false;
        }

        return true;
    }

    private static string SerializePayload(FileAccessPayload payload)
    {
        return string.Join('|', payload.FileId.ToString("N"), payload.Action, payload.ExpiresAt, payload.AccessVersion);
    }

    private static bool TryParsePayload(string serializedPayload, out FileAccessPayload payload)
    {
        payload = default;
        var parts = serializedPayload.Split('|', StringSplitOptions.None);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!Guid.TryParse(parts[0], out var fileId)
            || string.IsNullOrWhiteSpace(parts[1])
            || !long.TryParse(parts[2], out var expiresAt)
            || !int.TryParse(parts[3], out var accessVersion))
        {
            return false;
        }

        payload = new FileAccessPayload(fileId, parts[1], expiresAt, accessVersion);
        return true;
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string data)
    {
        var padded = data.Replace('-', '+').Replace('_', '/');
        var remainder = padded.Length % 4;
        if (remainder > 0)
        {
            padded = padded.PadRight(padded.Length + (4 - remainder), '=');
        }

        return Convert.FromBase64String(padded);
    }
}

public readonly record struct FileAccessPayload(Guid FileId, string Action, long ExpiresAt, int AccessVersion);
