using ManagedCode.Database.EntityFramework.MSSQL;
using ManagedCode.Database.Tests.MSSQL.Models;

namespace ManagedCode.Database.Tests.MSSQL.Repositories;

public interface ICustomerRepository : IMSSQLRepository<int, Customer>
{
}