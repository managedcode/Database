using System;
using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace ManagedCode.Repository.CosmosDB;

public class CosmosDbItem : IItem<string>
{
    public CosmosDbItem()
    {
        Id = Guid.NewGuid().ToString();
        Type = GetType().Name;
    }

    public CosmosDbItem(string id)
    {
        Id = id;
        Type = GetType().Name;
    }

    [JsonProperty("type")]
    public string Type { get; set; }

    public virtual PartitionKey PartitionKey => new(Id);

    [JsonProperty("id")]
    public string Id { get; set; }
}