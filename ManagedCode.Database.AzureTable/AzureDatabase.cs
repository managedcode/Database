using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using ManagedCode.Database.Core.InMemory;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureDatabase : BaseDatabase, IDatabase<CloudTableClient>
{
    private readonly AzureTableRepositoryOptions _options;
    private Dictionary<string, object> tableAdapters = new();
    public AzureDatabase(AzureTableRepositoryOptions options)
    {
        _options = options;
        IsInitialized = true;
    }
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
        tableAdapters.Clear();
    }
    
    public AzureTableDBCollection<TItem>  GetCollection<TId, TItem>()  where TItem : AzureTableItem, IItem<TId>, new()
    {
        return GetCollection<TId, TItem>(typeof(TItem).FullName);
    }
    
    public AzureTableDBCollection<TItem> GetCollection<TId, TItem>(string name) where TItem : AzureTableItem, IItem<TId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }
        
        lock (tableAdapters)
        {
            if (tableAdapters.TryGetValue(name, out var table))
            {
                return (AzureTableDBCollection<TItem>)table;
            }

            AzureTableAdapter<TItem> tableAdapter;
            if (string.IsNullOrEmpty(_options.ConnectionString))
            {
                tableAdapter = new AzureTableAdapter<TItem>(Logger, _options.TableStorageCredentials, _options.TableStorageUri);
            }
            else
            {
                tableAdapter = new AzureTableAdapter<TItem>(Logger, _options.ConnectionString);
            }
            
            var collection = new AzureTableDBCollection<TItem>(tableAdapter);

            tableAdapters[name] = collection;
            return collection;
        }
    }
    
    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public CloudTableClient DataBase { get; }
}