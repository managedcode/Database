using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureTableDatabase : BaseDatabase, IDatabase<CloudTableClient>
{
    private readonly AzureTableRepositoryOptions _options;
    private readonly Dictionary<string, object> _tableAdapters = new();

    public AzureTableDatabase(AzureTableRepositoryOptions options)
    {
        _options = options;
        IsInitialized = true;
    }

    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public CloudTableClient DataBase { get; }

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
        _tableAdapters.Clear();
    }

    public AzureTableDBCollection<TItem> GetCollection<TId, TItem>() where TItem : AzureTableItem, IItem<TId>, new()
    {
        return GetCollection<TId, TItem>(typeof(TItem).FullName);
    }

    public AzureTableDBCollection<TItem> GetCollection<TId, TItem>(string name)
        where TItem : AzureTableItem, IItem<TId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        lock (_tableAdapters)
        {
            if (_tableAdapters.TryGetValue(name, out var table))
            {
                return (AzureTableDBCollection<TItem>)table;
            }

            var tableAdapter = string.IsNullOrEmpty(_options.ConnectionString) switch
            {
                true => new AzureTableAdapter<TItem>(_options.TableStorageCredentials, _options.TableStorageUri),
                false => new AzureTableAdapter<TItem>(_options.ConnectionString),
            };


            var collection = new AzureTableDBCollection<TItem>(tableAdapter);

            _tableAdapters[name] = collection;
            return collection;
        }
    }
}