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

public class MongoDatabase : BaseDatabase<IMongoDatabase>
{
    private readonly MongoOptions _options;

    public MongoDatabase(MongoOptions options)
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

    public MongoDBCollection<TItem> GetCollection<TItem>() where TItem : class, IItem<ObjectId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        var collectionName = string.IsNullOrEmpty(_options.CollectionName)
            ? typeof(TItem).Name.Pluralize()
            : _options.CollectionName;

        return new MongoDBCollection<TItem>(
            NativeClient.GetCollection<TItem>(collectionName, new MongoCollectionSettings()));
    }
}