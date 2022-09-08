using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using MongoDB.Bson;
using Realms;
using Realms.Sync;

namespace ManagedCode.Repository.Realm;

public class RealmDataBase : BaseDatabase, IDatabase<IMongoDatabase>
{
    private readonly MongoDbRepositoryOptions _options;

    public RealmDataBase([NotNull] MongoDbRepositoryOptions options)
    {
        var client = Realms.Realm.GetInstance();// new MongoClient(options.ConnectionString);
        var database = client.GetDatabase(options.DataBaseName);
        var collectionName = string.IsNullOrEmpty(options.CollectionName) ? typeof(TItem).Name.Pluralize() : options.CollectionName;
        _collection = database.GetCollection<TItem>(collectionName, new MongoCollectionSettings());
        IsInitialized = true;
    }
    
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
        return  new MongoDbCollection<TItem>(DataBase.GetCollection<TItem>(collectionName, new MongoCollectionSettings()));
    }
    
    public MongoDbCollection<TItem> GetCollection<TId, TItem>(string name) where TItem : class, IItem<ObjectId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }
        
        return  new MongoDbCollection<TItem>(DataBase.GetCollection<TItem>(name, new MongoCollectionSettings()));
    }

    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public IMongoDatabase DataBase { get; }
}