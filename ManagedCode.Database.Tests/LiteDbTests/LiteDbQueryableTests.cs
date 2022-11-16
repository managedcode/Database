using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.LiteDB;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.LiteDbTests
{
    public class LiteDbQueryableTests : BaseQueryableTests<string, TestLiteDbItem>
    {
        private readonly LiteDBDatabase _database;

        public LiteDbQueryableTests()
        {
            _database = new LiteDBDatabase(new LiteDBOptions()
            {
                ConnectionString = "litedb_test.db"
            });
        }

        protected override IDatabaseCollection<string, TestLiteDbItem> Collection =>
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

    }
}