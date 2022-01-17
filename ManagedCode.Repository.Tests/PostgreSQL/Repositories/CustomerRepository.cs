using ManagedCode.Repository.EntityFramework.PostgreSQL;
using ManagedCode.Repository.Tests.PostgreSQL.Models;

namespace ManagedCode.Repository.Tests.PostgreSQL.Repositories
{
    public class CustomerRepository : PostgresRepository<int, Customer>, ICustomerRepository
    {
        public CustomerRepository(PostgresDatabaseContext context) : base(context)
        {
        }
    }
}
