using System;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.Cosmos;

public class CosmosOptions
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string CollectionName { get; set; }
    
    public string PartitionKey { get; set; } = "/id";
    public CosmosClientOptions CosmosClientOptions { get; set; }
    public bool SplitByType { get; set; } = true;
    public bool UseItemIdAsPartitionKey { get; set; } = true;
    public bool AllowTableCreation { get; set; }

    public TimeSpan? MaxRetryWaitTimeOnRateLimitedRequests { get; set; } 
    public int? MaxRetryAttemptsOnRateLimitedRequests { get; set; } 
}