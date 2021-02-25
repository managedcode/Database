using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    public class AzureTableRepositoryOptions
    {
        public string ConnectionString { get; set; }
        public StorageCredentials TableStorageCredentials { get; set; }
        public StorageUri TableStorageUri { get; set; }
    }
}