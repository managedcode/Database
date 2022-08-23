using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteRepositoryOptions
{
    public string ConnectionString { get; set; }
    public SQLiteConnection Connection { get; set; }
}