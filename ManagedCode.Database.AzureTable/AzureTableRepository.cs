using System.Diagnostics.CodeAnalysis;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

/*
public class AzureDBCollection<TItem> : BaseAzureTableRepository<TableId, TItem>, IAzureTableRepository<TItem>
    where TItem : class, IItem<TableId>, ITableEntity, new()
{
    public AzureDBCollection([NotNull] AzureTableRepositoryOptions options) : base(options)
    {
    }
}*/