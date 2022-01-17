using Microsoft.EntityFrameworkCore;
using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.EntityFramework.PostgreSQL
{
    public class PostgresDatabaseContext : EFDbContext<PostgresDatabaseContext>
    {
        public PostgresDatabaseContext(
            DbContextOptions<PostgresDatabaseContext> options,
            ServiceCollectionHolder serviceCollectionHolder)
        : base(options, serviceCollectionHolder) { }
    }
}
