using ManagedCode.Repository.EntityFramework.MSSQL;

namespace ManagedCode.Repository.Tests.MSSQL
{
    public class CustomerRepository : MSSQLRepository<int, Customer>, ICustomerRepository
    {
        public CustomerRepository(MSSQLDatabaseContext context) : base(context)
        {
        }
    }
}
