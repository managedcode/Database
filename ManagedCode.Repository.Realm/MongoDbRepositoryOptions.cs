namespace ManagedCode.Repository.MongoDB;

public class RealmDbRepositoryOptions
{
    public string ConnectionString { get; set; }
    public string DataBaseName { get; set; }

    public string CollectionName { get; set; }
}