using ManagedCode.Database.EntityFramework.MSSQL;

namespace ManagedCode.Database.Tests.MSSQL.Models;

public class Customer : MSSQLItem<int>
{
    public string Name { get; set; }
    public int Age { get; set; }
}