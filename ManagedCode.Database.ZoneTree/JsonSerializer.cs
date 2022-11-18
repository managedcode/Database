using System.Text.Json;
using Tenray.ZoneTree.Serializers;

namespace ManagedCode.Database.ZoneTree;

internal class JsonSerializer<T> : ISerializer<T>
{
    public T Deserialize(byte[] bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes);
    }

    public byte[] Serialize(in T entry)
    {
        return JsonSerializer.SerializeToUtf8Bytes(entry);
    }
}