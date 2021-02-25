using LiteDB;

namespace ManagedCode.Repository.LiteDB
{
    public class LiteDbRepositoryOptions
    {
        public string ConnectionString { get; set; }
        public LiteDatabase Database { get; set; }
    }
}