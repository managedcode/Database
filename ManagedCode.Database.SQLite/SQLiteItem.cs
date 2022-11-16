using ManagedCode.Database.Core;
using SQLite;

namespace ManagedCode.Database.SQLite
{
    public class SQLiteItem : IItem<int>
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
    }
}