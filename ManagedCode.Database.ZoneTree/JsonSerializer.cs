using Tenray.ZoneTree.Serializers;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class JsonSerializer<T> : ISerializer<T>
{
    public T Deserialize(byte[] bytes)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(bytes);
    }

    public byte[] Serialize(in T entry)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(entry);
    }
}