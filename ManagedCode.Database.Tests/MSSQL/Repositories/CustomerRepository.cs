using ManagedCode.Database.EntityFramework.MSSQL;
using ManagedCode.Database.Tests.MSSQL.Models;

namespace ManagedCode.Database.Tests.MSSQL.Repositories;

public class CustomerRepository : MSSQLRepository<int, Customer>, ICustomerRepository
{
    public CustomerRepository(MSSQLDatabaseContext context) : base(context)
    {
    }
}