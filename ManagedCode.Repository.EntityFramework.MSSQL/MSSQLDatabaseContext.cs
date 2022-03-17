using ManagedCode.Repository.Core;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Repository.EntityFramework.MSSQL;

public class MSSQLDatabaseContext : EFDbContext<MSSQLDatabaseContext>
{
    public MSSQLDatabaseContext(
        DbContextOptions<MSSQLDatabaseContext> options,
        ServiceCollectionHolder serviceCollectionHolder)
        : base(options, serviceCollectionHolder)
    {
    }
}