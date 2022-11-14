using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace ManagedCode.Database.AzureTable.Extensions;

public static class TableClientExtensions
{
    private const int MaxItemsPerRequest = 100;

    public static async Task<List<Response>> SubmitTransactionByChunksAsync<TItem>(this TableClient tableClient,
        IEnumerable<TItem> items,
        TableTransactionActionType actionType, CancellationToken cancellationToken) where TItem : ITableEntity, new()
    {
        var chunks = items.Select(item =>
            new TableTransactionAction(actionType, item, item.ETag)).Chunk(MaxItemsPerRequest);

        List<Response> responses = new();

        foreach (var chunk in chunks)
        {
            var response =
                await ExceptionCatcher.ExecuteAsync(tableClient.SubmitTransactionAsync(chunk, cancellationToken));

            var list = response?.Value.Select(i => i) ?? new List<Response>();
            responses.AddRange(list);
        }

        return responses;
    }
}