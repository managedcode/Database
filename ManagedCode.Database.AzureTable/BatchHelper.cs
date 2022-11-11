using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;

namespace ManagedCode.Database.AzureTable;

public static class BatchHelper
{
    private const int Capacity = 50;

    public static async Task<int> ExecuteAsync(IEnumerable<Task<Response?>> actions, CancellationToken token = default)
    {
        var count = 0;
        var batch = new List<Task>(Capacity);
        foreach (var action in actions)
        {
            token.ThrowIfCancellationRequested();

            batch.Add(action
                .ContinueWith(task =>
                {
                    if (task.Result is not null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
    }
}