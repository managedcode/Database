using System.Diagnostics.CodeAnalysis;
using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace ManagedCode.Repository.AzureTable
{
    public class AzureTableRepository<TItem> : BaseAzureTableRepository<TableId, TItem>, IAzureTableRepository<TItem>
        where TItem : class, IItem<TableId>, ITableEntity, new()
    {
        public AzureTableRepository(ILogger logger, [NotNull] AzureTableRepositoryOptions options) : base(logger, options)
        {
        }
    }
}