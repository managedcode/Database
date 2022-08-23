namespace ManagedCode.Database.MongoDB;

public class MongoDbRepositoryOptions
{
    public string ConnectionString { get; set; }
    public string DataBaseName { get; set; }

    public string CollectionName { get; set; }
}