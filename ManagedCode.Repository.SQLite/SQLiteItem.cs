using ManagedCode.Repository.Core;
using SQLite;

namespace ManagedCode.Repository.SQLite;

public class SQLiteItem : IItem<int>
{
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }
}