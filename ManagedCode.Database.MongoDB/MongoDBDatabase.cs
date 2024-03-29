﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ManagedCode.Database.MongoDB;

public class MongoDBDatabase : BaseDatabase<IMongoDatabase>
{
    private readonly MongoDBOptions _dbOptions;

    public MongoDBDatabase(MongoDBOptions dbOptions)
    {
        _dbOptions = dbOptions;
    }

    public override Task DeleteAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        var client = new MongoClient(_dbOptions.ConnectionString);
        NativeClient = client.GetDatabase(_dbOptions.DataBaseName);

        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
    }

    public MongoDBCollection<TItem> GetCollection<TItem>() where TItem : MongoDBItem, new()
    {
        if (!IsInitialized) throw new DatabaseNotInitializedException(GetType());

        var collectionName = string.IsNullOrEmpty(_dbOptions.CollectionName)
            ? typeof(TItem).Name.Pluralize()
            : _dbOptions.CollectionName;

        return new MongoDBCollection<TItem>(
            NativeClient.GetCollection<TItem>(collectionName, new MongoCollectionSettings()));
    }
}