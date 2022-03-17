using ManagedCode.Repository.EntityFramework.MSSQL;

namespace ManagedCode.Repository.Tests.MSSQL.Models;

public class Customer : MSSQLItem<int>
{
    public string Name { get; set; }
    public int Age { get; set; }
}