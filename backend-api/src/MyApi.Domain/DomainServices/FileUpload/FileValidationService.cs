using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MyApi.Domain.DomainServices.FileUpload;

public class FileValidationService
{
    private readonly Dictionary<string, byte[]> _magicBytes = new() {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { ".zip", new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
        // 添加更多...
    };
    private readonly long _maxSize;

    public FileValidationService(IConfiguration config)
    {
        _maxSize = config.GetValue<long?>("FileValidation:MaxFileSizeMB") ?? 100L;
        _maxSize *= 1024 * 1024;
    }

    public async Task<(bool Valid, string Msg)> ValidateAsync(IFormFile file)
    {
        if (file.Length > _maxSize) return (false, "文件过大");
            
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (_magicBytes.ContainsKey(ext))
        {
            using var stream = file.OpenReadStream();
            byte[] header = new byte[_magicBytes[ext].Length];
            var offset = 0;
            while (offset < header.Length)
            {
                var read = await stream.ReadAsync(header.AsMemory(offset, header.Length - offset));
                if (read == 0) break;
                offset += read;
            }

            if (offset < header.Length) return (false, "文件头读取失败");
            if (!header.SequenceEqual(_magicBytes[ext])) return (false, "文件头校验失败");
        }
            
        // 可在此处加入 ImageSharp 格式深检 logic
            
        return (true, "OK");
    }
}
