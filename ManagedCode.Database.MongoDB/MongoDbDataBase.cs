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

public class MongoDbDatabase : BaseDatabase, IDatabase<IMongoDatabase>
{
    private readonly MongoDbRepositoryOptions _options;

    public MongoDbDatabase([NotNull] MongoDbRepositoryOptions options)
    {
        _options = options;
        var client = new MongoClient(options.ConnectionString);
        DataBase = client.GetDatabase(options.DataBaseName);
        IsInitialized = true;
    }

    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public IMongoDatabase DataBase { get; }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
    }

    public MongoDbCollection<TItem> GetCollection<TId, TItem>() where TItem : class, IItem<ObjectId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        var collectionName = string.IsNullOrEmpty(_options.CollectionName) ? typeof(TItem).Name.Pluralize() : _options.CollectionName;
        return new MongoDbCollection<TItem>(DataBase.GetCollection<TItem>(collectionName, new MongoCollectionSettings()));
    }

    public MongoDbCollection<TItem> GetCollection<TId, TItem>(string name) where TItem : class, IItem<ObjectId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        return new MongoDbCollection<TItem>(DataBase.GetCollection<TItem>(name, new MongoCollectionSettings()));
    }
}