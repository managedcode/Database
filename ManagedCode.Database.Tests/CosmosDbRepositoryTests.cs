/*using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.CosmosDB;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests
{
    public class CosmosDbRepositoryTests : BaseRepositoryTests<string, TestCosmosDbItem>
    {
        public const string ConnectionString =
            "AccountEndpoint=http://localhost:3000/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private CosmosDatabase _databaseb;
        public CosmosDbRepositoryTests()
        {
            _databaseb = new CosmosDatabase(new CosmosDbRepositoryOptions
            {
                ConnectionString = ConnectionString,
                DatabaseName = "database",
                CollectionName = "container",
            });
        }

        protected override IDBCollection<string, TestCosmosDbItem> Collection => _databaseb.GetCollection<TestCosmosDbItem>();

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

        public override void Dispose()
        {
            _databaseb.Dispose();
        }
    }
}

*/