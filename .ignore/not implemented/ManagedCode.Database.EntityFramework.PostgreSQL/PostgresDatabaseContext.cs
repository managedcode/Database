using ManagedCode.Database.Core;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Database.EntityFramework.PostgreSQL;

public class PostgresDatabaseContext : EFDbContext<PostgresDatabaseContext>
{
    public PostgresDatabaseContext(
        DbContextOptions<PostgresDatabaseContext> options,
        ServiceCollectionHolder serviceCollectionHolder)
        : base(options, serviceCollectionHolder)
    {
    }
}