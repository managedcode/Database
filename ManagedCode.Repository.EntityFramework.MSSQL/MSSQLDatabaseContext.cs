using Microsoft.EntityFrameworkCore;
using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.EntityFramework.MSSQL
{
    public class MSSQLDatabaseContext : EFDbContext<MSSQLDatabaseContext>
    {
        public MSSQLDatabaseContext(
            DbContextOptions<MSSQLDatabaseContext> options,
            ServiceCollectionHolder serviceCollectionHolder) 
        : base(options, serviceCollectionHolder) { }
    }
}
