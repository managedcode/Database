using System;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDbItem : IItem<string>
{
    public CosmosDbItem()
    {
        Id = $"{Guid.NewGuid():N}";
        Type = GetType().Name;
    }

    public CosmosDbItem(string id)
    {
        Id = id;
        Type = GetType().Name;
    }

    [JsonProperty("type")] public string Type { get; set; }

    public virtual PartitionKey PartitionKey => new(Id);

    [JsonProperty("id")] public string Id { get; set; }
}