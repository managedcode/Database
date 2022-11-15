using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.LiteDB;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.LiteDbTests
{
    public class LiteDbRepositoryTests : BaseRepositoryTests<string, TestLiteDbItem>
    {
        private readonly LiteDbDatabase _database;

        public LiteDbRepositoryTests()
        {
            _database = new LiteDbDatabase(new LiteDbRepositoryOptions
            {
                ConnectionString = "litedb_test.db"
            });
        }

        protected override IDBCollection<string, TestLiteDbItem> Collection =>
            _database.GetCollection<string, TestLiteDbItem>();

        protected override string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        public override async Task InitializeAsync()
        {
            await _database.InitializeAsync();
        }

        public override async Task DisposeAsync()
        {
            await _database.DisposeAsync();
        }

        [Fact]
        public override async Task InsertOneItem()
        {
            Func<Task> act = () => base.Insert99Items();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Cannot insert duplicate key in unique index '_id'*");
        }

        [Fact]
        public override async Task Insert99Items()
        {
            Func<Task> act = () => base.Insert99Items();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Cannot insert duplicate key in unique index '_id'*");
        }
    }
}