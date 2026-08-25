using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;

public sealed class ResettableCache : IDistributedCache, IResettableCache
{
    private readonly IDistributedCache _cache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public ResettableCache(MemoryDistributedCache cache)
    {
        _cache = cache;
    }
    
    public byte[]? Get(string key)
    {
        return _cache.Get(key);
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = new CancellationToken())
    {
        return await _cache.GetAsync(key, token);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = new CancellationToken())
    {
        await _cache.SetAsync(key, value, options, token);
        _keys.TryAdd(key, 0);
    }

    public void Refresh(string key)
    {
        _cache.Refresh(key);
    }

    public async Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
    {
        await _cache.RefreshAsync(key, token);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out var _);
    }

    public async Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
    {
        await _cache.RemoveAsync(key, token);
        _keys.TryRemove(key, out var _);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        foreach (var item in _keys)
        {
            await _cache.RemoveAsync(item.Key, cancellationToken);
        }
        _keys.Clear();
    }
}