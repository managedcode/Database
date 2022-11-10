using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureTableRepositoryOptions
{
    public string ConnectionString { get; set; }
    public StorageCredentials TableStorageCredentials { get; set; }
    public StorageUri TableStorageUri { get; set; }
    public bool AllowTableCreation { get; set; }
}