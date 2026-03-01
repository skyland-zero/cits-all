using Cits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace MyApi.HttpApi.Controllers;

public class CacheController : BaseApiController
{
    private readonly HybridCache _cache;

    public CacheController(HybridCache cache)
    {
        _cache = cache;
    }

    [Authorize("123")]
    [HttpGet("SetCache")]
    public async Task<bool> SetCache()
    {
        var tags = new List<string> { "tag1", "tag2", "tag3" };
        var entryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10),
            LocalCacheExpiration = TimeSpan.FromMinutes(10)
        };

        for (var i = 0; i < 10000; i++)
            await _cache.SetAsync(
                $"test-{i}",
                i,
                entryOptions,
                tags
            );
        return true;
    }

    [HttpGet("DeleteCacheByTag")]
    public async Task<int> DeleteCacheByTag()
    {
        var tags = new List<string> { "tag1" };
        await _cache.RemoveByTagAsync(tags);

        var cache = await _cache.GetOrCreateAsync("test-100",
            _ => ValueTask.FromResult(-1),
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableLocalCacheWrite |
                        HybridCacheEntryFlags.DisableDistributedCacheWrite
            });

        return cache;
    }
}