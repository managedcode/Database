using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Humanizer;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.AzureTable;

public class AzureTableDatabase : BaseDatabase<TableServiceClient>
{
    private readonly AzureTableRepositoryOptions _options;
    private readonly Dictionary<string, object> _collections = new();

    public AzureTableDatabase(AzureTableRepositoryOptions options)
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
            false => new TableServiceClient(_options.ConnectionString),
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

    public AzureTableDBCollection<TItem> GetCollection<TId, TItem>() where TItem : AzureTableItem, IItem<TId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        var collectionName = typeof(TItem).FullName;
        var tableName = GetTableName<TItem>();

        lock (_collections)
        {
            if (_collections.TryGetValue(collectionName, out var obj))
            {
                return obj as AzureTableDBCollection<TItem>;
            }

            var table = NativeClient.GetTableClient(tableName);

            if (_options.AllowTableCreation)
            {
                table.CreateIfNotExists();
            }
            else
            {
                // var exists = table.
                //
                // if (!exists)
                // {
                //     throw new InvalidOperationException($"Table '{tableName}' does not exist");
                // }
            }

            var collection = new AzureTableDBCollection<TItem>(table);

            _collections[collectionName] = collection;

            return collection;
        }
    }

    private string GetTableName<TItem>()
    {
        return typeof(TItem).Name.Pluralize();
    }
}