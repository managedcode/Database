using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ManagedCode.Database.MongoDB;

public class MongoDbDatabase : BaseDatabase<IMongoDatabase>
{
    private readonly MongoDbRepositoryOptions _options;

    public MongoDbDatabase(MongoDbRepositoryOptions options)
    {
        _options = options;
    }

    public override Task DeleteAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        var client = new MongoClient(_options.ConnectionString);
        NativeClient = client.GetDatabase(_options.DataBaseName);

        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
    }

    public MongoDbCollection<TItem> GetCollection<TItem>() where TItem : class, IItem<ObjectId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        var collectionName = string.IsNullOrEmpty(_options.CollectionName)
            ? typeof(TItem).Name.Pluralize()
            : _options.CollectionName;

        return new MongoDbCollection<TItem>(
            NativeClient.GetCollection<TItem>(collectionName, new MongoCollectionSettings()));
    }
}