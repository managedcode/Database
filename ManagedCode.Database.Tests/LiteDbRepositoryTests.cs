using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.LiteDB;
using ManagedCode.Database.Tests.Common;
using SQLite;
using Xunit;

namespace ManagedCode.Database.Tests
{
    public class LiteDbRepositoryTests : BaseRepositoryTests<string, TestLiteDbItem>, IDisposable
    {
        public const string ConnectionString = "litedb_test.db";
        private LiteDbDatabase _databaseb;

        public LiteDbRepositoryTests()
        {
            _databaseb = new LiteDbDatabase(new LiteDbRepositoryOptions
            {
                ConnectionString = ConnectionString
            });
            _databaseb.InitializeAsync().Wait();
        }



        protected override IDBCollection<string, TestLiteDbItem> Collection => _databaseb.GetCollection<string, TestLiteDbItem>();

        protected override string GenerateId()
        {
            return Guid.NewGuid().ToString();
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

        public override void Dispose()
        {
            _databaseb.Dispose();
        }
    }
}

