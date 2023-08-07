using System;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace ManagedCode.Database.Cosmos;

public class CosmosItem : IItem<string>
{
    public CosmosItem()
    {
        Id = $"{Guid.NewGuid():N}";
        Type = GetType().Name;
    }

    public CosmosItem(string id)
    {
        Id = id;
        Type = GetType().Name;
    }

    [JsonProperty("type")] public string Type { get; set; }

    public PartitionKey PartitionKey { get; protected set; }

    [JsonProperty("id")] public string Id { get; set; }
}