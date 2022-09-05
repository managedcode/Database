using ManagedCode.Database.EntityFramework.PostgreSQL;
using ManagedCode.Database.Tests.PostgreSQL.Models;

namespace ManagedCode.Database.Tests.PostgreSQL.Repositories;

public interface ICustomerRepository : IPostgresRepository<int, Customer>
{
}