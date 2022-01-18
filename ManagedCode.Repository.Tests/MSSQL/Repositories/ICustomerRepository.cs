using ManagedCode.Repository.EntityFramework.MSSQL;
using ManagedCode.Repository.Tests.MSSQL.Models;

namespace ManagedCode.Repository.Tests.MSSQL.Repositories
{
    public interface ICustomerRepository : IMSSQLRepository<int, Customer>
    {
    }
}
