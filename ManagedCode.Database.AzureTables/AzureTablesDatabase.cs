using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Humanizer;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.AzureTables;

public class AzureTablesDatabase : BaseDatabase<TableServiceClient>
{
    private readonly Dictionary<string, object> _collections = new();
    private readonly AzureTablesOptions _options;

    public AzureTablesDatabase(AzureTablesOptions options)
    {
        _options = options;
    }

    public override Task DeleteAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        NativeClient = string.IsNullOrEmpty(_options.ConnectionString) switch
        {
            true => new TableServiceClient(_options.TableStorageUri, _options.TableSharedKeyCredential),
            false => new TableServiceClient(_options.ConnectionString)
        };

        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        DisposeInternal();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        _collections.Clear();
    }

    public AzureTablesDatabaseCollection<TItem> GetCollection<TId, TItem>()
        where TItem : AzureTablesItem, IItem<TId>, new()
    {
        if (!IsInitialized) throw new DatabaseNotInitializedException(GetType());

        var collectionName = typeof(TItem).FullName;
        var tableName = GetTableName<TItem>();

        lock (_collections)
        {
            if (_collections.TryGetValue(collectionName, out var obj))
                return obj as AzureTablesDatabaseCollection<TItem>;

            var table = NativeClient.GetTableClient(tableName);

            if (_options.AllowTableCreation)
            {
                table.CreateIfNotExists();
            }

            // var exists = table.
            //
            // if (!exists)
            // {
            //     throw new InvalidOperationException($"Table '{tableName}' does not exist");
            // }
            var collection = new AzureTablesDatabaseCollection<TItem>(table);

            _collections[collectionName] = collection;

            return collection;
        }
    }

    private string GetTableName<TItem>()
    {
        return string.IsNullOrEmpty(_options.TableName) ? typeof(TItem).Name.Pluralize() : _options.TableName;
    }
}