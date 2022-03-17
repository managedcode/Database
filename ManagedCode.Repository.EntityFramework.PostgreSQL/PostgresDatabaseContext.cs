using ManagedCode.Repository.Core;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Repository.EntityFramework.PostgreSQL;

public class PostgresDatabaseContext : EFDbContext<PostgresDatabaseContext>
{
    public PostgresDatabaseContext(
        DbContextOptions<PostgresDatabaseContext> options,
        ServiceCollectionHolder serviceCollectionHolder)
        : base(options, serviceCollectionHolder)
    {
    }
}