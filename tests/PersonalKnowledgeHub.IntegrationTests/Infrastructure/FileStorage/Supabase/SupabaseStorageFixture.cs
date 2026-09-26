using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Options;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Storage.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.FileStorage.Supabase;

public class SupabaseStorageFixture : IAsyncLifetime
{
    public PersonalKnowledgeHubWebApplicationFactory? Factory { get; private set; }
    private AsyncServiceScope? _scope;
    private readonly ConcurrentDictionary<(int UserId, string StoredKey), byte> _trackedObjects = new();
    public IFileStorage Storage { get; private set; } = null!;
    
    public Task InitializeAsync()
    {
        try
        {
            Factory = new PersonalKnowledgeHubWebApplicationFactory(
                null,
                null,
                new FactoryOptions
                {
                    EnableHangfireServer = false,
                    EnableRecurringJobs = false,
                    EnableHangfireStorage = false,
                    EnableHangfireWrapper = false,
                    EnableRateLimitMiddleware = false,
                    EnableRedisWrapper = true,
                    EnableExternalHealthChecks = false,
                    Provider = StorageProvider.Supabase
                });
            _scope = Factory.Services.CreateAsyncScope();
            Storage = _scope.Value.ServiceProvider.GetRequiredService<IFileStorage>();
            return Task.CompletedTask;
        }
        catch
        {
            return DisposeAfterInitializationFailureAsync();
        }
    }

    public void Track(int userId, FileResult result)
    {
        _trackedObjects.TryAdd((userId, result.StoredKey), 0);
    }

    public void Untrack(int userId, FileResult result)
    {
        _trackedObjects.TryRemove((userId, result.StoredKey), out _);
    }

    public async Task ResetAsync()
    {
        foreach (var trackedObject in _trackedObjects.Keys)
        {
            await Storage.DeleteFile(trackedObject.StoredKey, trackedObject.UserId, CancellationToken.None);
            _trackedObjects.TryRemove(trackedObject, out _);
        }
    }

    public async Task DisposeAfterInitializationFailureAsync()
    {
        await DisposeAsync();
        throw new InvalidOperationException("Supabase fixture initialization failed");
    }

    public async Task DisposeAsync()
    {
        List<Exception> exceptions = [];

        try
        {
            await ResetAsync();
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }

        try
        {
            if (_scope.HasValue)
            {
                await _scope.Value.DisposeAsync();
            }
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }

        try
        {
            if (Factory is not null)
            {
                await Factory.DisposeAsync();
            }
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException(
                "Supabase fixture cleanup failed",
                exceptions);
        }
    }
}