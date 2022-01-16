using ManagedCode.Repository.EntityFramework.MSSQL;
using ManagedCode.Repository.Tests.MSSQL.Models;

namespace ManagedCode.Repository.Tests.MSSQL.Repositories
{
    public class CustomerRepository : MSSQLRepository<int, Customer>, ICustomerRepository
    {
        public CustomerRepository(MSSQLDatabaseContext context) : base(context)
        {
        }
    }
}
