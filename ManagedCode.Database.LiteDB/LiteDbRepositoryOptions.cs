using LiteDB;

namespace ManagedCode.Database.LiteDB;

public class LiteDbRepositoryOptions
{
    public string ConnectionString { get; set; }
    public LiteDatabase? Database { get; set; }
}