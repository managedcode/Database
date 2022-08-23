using ManagedCode.Database.EntityFramework.PostgreSQL;
using ManagedCode.Database.Tests.PostgreSQL.Models;

namespace ManagedCode.Database.Tests.PostgreSQL.Repositories;

public class CustomerRepository : PostgresRepository<int, Customer>, ICustomerRepository
{
    public CustomerRepository(PostgresDatabaseContext context) : base(context)
    {
    }
}