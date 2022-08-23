using ManagedCode.Database.Core;
using Newtonsoft.Json;

namespace ManagedCode.Database.LiteDB;

public class LiteDbItem<TId> : IItem<TId>
{
    [JsonProperty("_id")]
    public TId Id { get; set; }
}