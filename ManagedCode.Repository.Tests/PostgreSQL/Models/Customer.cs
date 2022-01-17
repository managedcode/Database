using ManagedCode.Repository.EntityFramework.PostgreSQL;

namespace ManagedCode.Repository.Tests.PostgreSQL.Models
{
    public class Customer : PostgresItem<int>
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
