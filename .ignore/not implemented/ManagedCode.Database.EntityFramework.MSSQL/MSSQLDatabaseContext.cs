using ManagedCode.Database.Core;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Database.EntityFramework.MSSQL;

public class MSSQLDatabaseContext : EFDbContext<MSSQLDatabaseContext>
{
    public MSSQLDatabaseContext(
        DbContextOptions<MSSQLDatabaseContext> options,
        ServiceCollectionHolder serviceCollectionHolder)
        : base(options, serviceCollectionHolder)
    {
    }
}