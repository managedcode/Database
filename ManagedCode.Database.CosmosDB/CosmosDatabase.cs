/*using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDatabase : BaseDatabase, IDatabase<CosmosClient>
{
    private readonly CosmosDbRepositoryOptions _options;

    public CosmosDatabase(CosmosDbRepositoryOptions options)
    {
        _options = options;
    }

    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public CosmosClient DataBase { get; }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        DisposeInternal();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
    }

    public CosmosDBCollection<TItem> GetCollection<TItem>() where TItem : CosmosDbItem, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        return new CosmosDBCollection<TItem>(_options);
    }
}*/