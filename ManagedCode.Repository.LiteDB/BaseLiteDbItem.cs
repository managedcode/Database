using ManagedCode.Repository.Core;
using Newtonsoft.Json;

namespace ManagedCode.Repository.LiteDB
{
    public class BaseLiteDbItem<TId> : IRepositoryItem<TId>
    {
        [JsonProperty("_id")] public TId Id { get; set; }
    }
}