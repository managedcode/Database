using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.CosmosDB;
using ManagedCode.Repository.Tests.Common;
using Xunit;

namespace ManagedCode.Repository.Tests
{
    public class CosmosDbRepositoryTests : BaseRepositoryTests<string, TestCosmosDbItem>
    {
        public const string ConnectionString =
            "AccountEndpoint=https://unhardevdb.documents.azure.com:443/;AccountKey=xE9bNGHnSjuj70bFCDkVRBjNZwm4Bkr7RZf8FJHUZYMACLwqy330Bh8mqjCRR0hOQhy9bVzf6x4VnBf6zWvq3Q==;";

        public CosmosDbRepositoryTests() : base(new CosmosDbRepository<TestCosmosDbItem>(
            new CosmosDbRepositoryOptions
            {
                ConnectionString = ConnectionString
            }))
        {
            Repository.InitializeAsync().Wait();
        }

        protected override string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        [Fact]
        public override async Task FindOrderThen()
        {
            Func<Task> act = () => base.FindOrderThen();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*The order by query does not have a corresponding composite index that it can be served from*");
        }
        
        [Fact]
        public override async Task Insert99Items()
        {
            Func<Task> act = () => base.Insert99Items();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Resource with specified id or name already exists*");
        }
        
        [Fact]
        public override async Task UpdateOneItem()
        {
            Func<Task> act = () => base.UpdateOneItem();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Resource Not Found*");
        }
        
        [Fact]
        public override async Task InsertOneItem()
        {
            Func<Task> act = () => base.InsertOneItem();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Resource with specified id or name already exists*");
        }
        
        [Fact]
        public override async Task Update5Items()
        {
            Func<Task> act = () => base.Update5Items();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Resource Not Found*");
        }
    }
}