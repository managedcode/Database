using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ManagedCode.Repository.Core.Extensions;
using ManagedCode.Repository.EntityFramework.MSSQL.Extensions;
using Xunit;

namespace ManagedCode.Repository.Tests.MSSQL
{
    public class MSSQLRepositoryTests
    {
        [Fact]
        public async Task TestDI()
        {
            var services = new ServiceCollection();

            services.AddScoped<ICustomerRepository, CustomerRepository>();

            services.AddManagedCodeRepository()
                .AddMSSQLBasedOnEF(opt => {
                    opt.ConnectionString = "Server=localhost;Database=ManagedCode;Trusted_Connection=True;";
                    opt.UseTracking = false;
                });

            var provider = services.BuildServiceProvider();

            var customerRepo = provider.GetService<ICustomerRepository>();

            var customer = await customerRepo.InsertAsync(
                new Customer
                {
                    Name = "Bobik",
                    Age = 5
                }
            );

        }
    }
}
