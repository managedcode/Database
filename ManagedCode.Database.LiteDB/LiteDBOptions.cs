using LiteDB;

namespace ManagedCode.Database.LiteDB
{
    public class LiteDBOptions
    {
        public string ConnectionString { get; set; }
        public LiteDatabase? Database { get; set; }
    }
}