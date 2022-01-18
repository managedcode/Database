using ManagedCode.Repository.Core.Extensions;
using ManagedCode.Repository.Tests.PostgreSQL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using ManagedCode.Repository.EntityFramework.PostgreSQL.Extensions;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using ManagedCode.Repository.Tests.PostgreSQL.Models;
using FluentAssertions;
using System.Linq.Expressions;
using System;
using System.Linq;

namespace ManagedCode.Repository.Tests.PostgreSQL
{
    public class PostgreSQLRepositoryTests
    {
        private ICustomerRepository _customerRepository;

        public PostgreSQLRepositoryTests()
        {
            var services = new ServiceCollection();

            services.AddScoped<ICustomerRepository, CustomerRepository>();

            services.AddManagedCodeRepository()
                .AddPostgreSQLBasedOnEF(opt => {
                    opt.ConnectionString = "Host=localhost;Port=5433;Database=ManagedCode;Username=postgres;Password=123";
                    opt.UseTracking = false;
                });

            var provider = services.BuildServiceProvider();

            _customerRepository = provider.GetService<ICustomerRepository>();
        }

        #region Insert

        [Fact]
        public async Task WhenSingleEntityInsertAsyncIsCalled()
        {
            var customer = GetValidCustomers()[0];
            var createdCustomer = await _customerRepository.InsertAsync(customer);

            createdCustomer.Should().NotBeNull();
            createdCustomer.Id.Should().NotBe(0);
        }

        [Fact]
        public async Task WhenMultipleEntitiesInsertAsyncIsCalled()
        {
            var customers = GetValidCustomers();
            await _customerRepository.InsertAsync(customers);

            customers.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Update

        [Fact]
        public async Task WhenSingleEntityUpdateAsyncIsCalled()
        {
            const string newName = "Kate";

            var customer = GetValidCustomers()[0];
            customer.Id = 1;
            customer.Age = 18;
            customer.Name = newName;

            var updatedCustomer = await _customerRepository.UpdateAsync(customer);

            updatedCustomer.Should().NotBeNull();
            updatedCustomer.Name.Should().Be(newName);
        }

        [Fact]
        public async Task WhenMultipleEntitiesUpdateAsyncIsCalled()
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    Id = 2,
                    Age = 20,
                    Name = "Victoria"
                },
                new Customer
                {
                    Id = 3,
                    Age = 22,
                    Name = "Sofia"
                }
            };

            var number = await _customerRepository.UpdateAsync(customers);

            number.Should().NotBe(0);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task WhenByIdDeleteAsyncIsCalled()
        {
            int idToDelete = 5;
            var result = await _customerRepository.DeleteAsync(idToDelete);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task WhenByItemDeleteAsyncIsCalled()
        {
            var customerToDelete = new Customer
            {
                Id = 6
            };

            var result = await _customerRepository.DeleteAsync(customerToDelete);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task WhenByIdsDeleteAsyncIsCalled()
        {
            var deletedCount = await _customerRepository.DeleteAsync(new List<int> { 3, 4 });

            deletedCount.Should().Be(2);
        }

        [Fact]
        public async Task WhenByItemsDeleteAsyncIsCalled()
        {
            var customersToDelete = new List<Customer>
            {
                new Customer { Id = 1 },
                new Customer { Id = 2 }
            };

            var deletedCount = await _customerRepository.DeleteAsync(customersToDelete);

            deletedCount.Should().Be(2);
        }

        [Fact]
        public async Task WhenByConditionDeleteAsyncIsCalled()
        {
            var deletedCount = await _customerRepository.DeleteAsync(c => c.Age == 40);

            deletedCount.Should().Be(2);
        }

        [Fact]
        public async Task WhenDeleteAllAsyncIsCalled()
        {
            var result = await _customerRepository.DeleteAllAsync();

            result.Should().BeTrue();
        }

        #endregion

        #region Get

        [Fact]
        public async Task WhenByIdGetAsyncIsCalled()
        {
            var retrievedCustomer = await _customerRepository.GetAsync(23);

            retrievedCustomer.Should().NotBeNull();
            retrievedCustomer.Id.Should().Be(23);
        }

        [Fact]
        public async Task WhenByConditionGetAsyncIsCalled()
        {
            var retrievedCustomer = await _customerRepository.GetAsync(c => c.Age == 20);

            retrievedCustomer.Should().NotBeNull();
        }

        [Fact]
        public async Task WhenGetAllAsyncIsCalled()
        {
            var retrievedCustomers = _customerRepository.GetAllAsync();
            var list = await retrievedCustomers.ToListAsync();

            list.Should().NotBeEmpty();
        }

        #endregion

        #region Find

        [Fact]
        public async Task WhenWithoutSortingFindAsyncIsCalled()
        {
            var elements = _customerRepository.FindAsync(
                new List<Expression<Func<Customer, bool>>>
                {
                    c => c.Id >= 20
                },
                4, 0);

            var list = await elements.ToListAsync();

            list.Should().NotBeEmpty();
        }

        [Fact]
        public async Task WhenWithSortingFindAsyncIsCalled()
        {
            var elements = _customerRepository.FindAsync(
                new List<Expression<Func<Customer, bool>>>
                {
                    c => c.Id >= 20
                },
                c => c.Name,
                Core.Order.By,
                4, 0);

            var list = await elements.ToListAsync();

            list.Should().NotBeEmpty();
        }

        [Fact]
        public async Task WhenWithThenSortingFindAsyncIsCalled()
        {
            var elements = _customerRepository.FindAsync(
                new List<Expression<Func<Customer, bool>>>
                {
                    c => c.Id >= 20
                },
                c => c.Name,
                Core.Order.By,
                c => c.Id,
                Core.Order.By,
                4, 0);

            var list = await elements.ToListAsync();

            list.Should().NotBeEmpty();
        }

        #endregion

        #region Count

        [Fact]
        public async Task WhenCountAsyncIsCalled()
        {
            var count = await _customerRepository.CountAsync();

            count.Should().Be(6);
        }

        [Fact]
        public async Task WhenByConditionsCountAsyncIsCalled()
        {
            var count = await _customerRepository.CountAsync(
                new List<Expression<Func<Customer, bool>>>
                {
                    c => c.Age == 20,
                    c => c.Age == 30
                }
            );

            count.Should().Be(4);
        }

        #endregion

        private IList<Customer> GetValidCustomers()
        {
            return new List<Customer>
            {
                new Customer
                {
                    Name = "Bill",
                    Age = 20
                },
                new Customer
                {
                    Name = "John",
                    Age = 30
                },
                new Customer
                {
                    Name = "Elon",
                    Age = 40
                }
            };
        }
    }
}
