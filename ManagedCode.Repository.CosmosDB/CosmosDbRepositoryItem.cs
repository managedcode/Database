using System;
using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace ManagedCode.Repository.CosmosDB
{
    public class CosmosDbRepositoryItem : IRepositoryItem<string>
    {
        public CosmosDbRepositoryItem()
        {
            Id = Guid.NewGuid().ToString();
        }

        public CosmosDbRepositoryItem(string id)
        {
            Id = id;
        }

        [JsonProperty("type")] public string Type { get; set; }

        public virtual PartitionKey PartitionKey => new(Id);

        [JsonProperty("id")] public string Id { get; set; }
    }
}