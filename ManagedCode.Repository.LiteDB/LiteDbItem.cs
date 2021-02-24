using ManagedCode.Repository.Core;
using Newtonsoft.Json;

namespace ManagedCode.Repository.LiteDB
{
    public class LiteDbItem<TId> : IItem<TId>
    {
        [JsonProperty("_id")] public TId Id { get; set; }
    }
}