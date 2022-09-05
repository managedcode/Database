using ManagedCode.Database.EntityFramework.PostgreSQL;

namespace ManagedCode.Database.Tests.PostgreSQL.Models;

public class Customer : PostgresItem<int>
{
    public string Name { get; set; }
    public int Age { get; set; }
}