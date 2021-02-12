using System;
using ManagedCode.Repository.Core;
using Newtonsoft.Json;

namespace ManagedCode.Repository.CosmosDB
{
    public class BaseLiteDbItem<TId> : IRepositoryItem<TId>
    {
        [JsonProperty("_id")] 
        public TId Id { get; set; }
    }
}