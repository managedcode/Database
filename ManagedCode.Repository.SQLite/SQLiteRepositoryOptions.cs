using SQLite;

namespace ManagedCode.Repository.SQLite
{
    public class SQLiteRepositoryOptions
    {
        public string ConnectionString { get; set; }
        public SQLiteConnection Connection { get; set; }
    }
}