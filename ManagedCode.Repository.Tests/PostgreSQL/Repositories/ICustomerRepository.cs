using ManagedCode.Repository.EntityFramework.PostgreSQL;
using ManagedCode.Repository.Tests.PostgreSQL.Models;

namespace ManagedCode.Repository.Tests.PostgreSQL.Repositories;

public interface ICustomerRepository : IPostgresRepository<int, Customer>
{
}