using ManagedCode.Repository.EntityFramework.MSSQL;

namespace ManagedCode.Repository.Tests.MSSQL
{
    public interface ICustomerRepository : IMSSQLRepository<int, Customer>
    {
    }
}
