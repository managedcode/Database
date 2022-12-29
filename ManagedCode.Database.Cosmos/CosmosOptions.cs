using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.Cosmos;

public class CosmosOptions
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; } = "database";
    public string CollectionName { get; set; } = "collection";
    
    public string PartitionKey { get; set; } = "/id";
    public CosmosClientOptions CosmosClientOptions { get; set; }
    public bool SplitByType { get; set; } = true;
    public bool UseItemIdAsPartitionKey { get; set; } = true;
    public bool AllowTableCreation { get; set; }
}