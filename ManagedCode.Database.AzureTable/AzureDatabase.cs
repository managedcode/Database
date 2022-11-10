using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureTableDatabase : BaseDatabase<CloudTableClient>
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
        var cloudStorageAccount = string.IsNullOrEmpty(_options.ConnectionString) switch
        {
            true => new CloudStorageAccount(_options.TableStorageCredentials, _options.TableStorageUri),
            false => CloudStorageAccount.Parse(_options.ConnectionString),
        };

        NativeClient = cloudStorageAccount.CreateCloudTableClient();
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
        return GetCollection<TId, TItem>(typeof(TItem).FullName);
    }

    private AzureTableDBCollection<TItem> GetCollection<TId, TItem>(string name)
        where TItem : AzureTableItem, IItem<TId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        var tableName = GetTableName<TItem>();

        lock (_collections)
        {
            if (_collections.TryGetValue(name, out var obj))
            {
                return obj as AzureTableDBCollection<TItem>;
            }

            var table = NativeClient.GetTableReference(tableName);

            if (_options.AllowTableCreation)
            {
                table.CreateIfNotExists();
            }
            else
            {
                var exists = table.Exists();

                if (!exists)
                {
                    throw new InvalidOperationException($"Table '{tableName}' does not exist");
                }
            }

            var collection = new AzureTableDBCollection<TItem>(table);

            _collections[name] = collection;

            return collection;
        }
    }

    private string GetTableName<TItem>()
    {
        return typeof(TItem).Name.Pluralize();
    }
}