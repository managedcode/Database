using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.SQLite;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests
{
    public class SQLiteRepositoryTests : BaseRepositoryTests<int, SQLiteDbItem>
    {
        private static int _count;
        private readonly SqLiteDatabase _database;

        public SQLiteRepositoryTests()
        {
            _database = new SqLiteDatabase(new SQLiteRepositoryOptions
            {
                ConnectionString = "sqlite_test.db"
            });
        }

        public override async Task InitializeAsync()
        {
            await _database.InitializeAsync();
        }

        public override async Task DisposeAsync()
        {
            await _database.DisposeAsync();
        }

        private static string GetTempDbName()
        {
            return Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + "sqlite_test.db");
        }

        protected override IDBCollection<int, SQLiteDbItem> Collection => _database.GetCollection<int, SQLiteDbItem>();

        protected override int GenerateId()
        {
            _count++;
            return _count;
        }

        [Fact]
        public override async Task InsertOneItem()
        {
            Func<Task> act = () => base.InsertOneItem();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("UNIQUE constraint failed: SQLiteDbItem.Id");
        }

        [Fact]
        public override async Task Insert99Items()
        {
            Func<Task> act = () => base.Insert99Items();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("UNIQUE constraint failed: SQLiteDbItem.Id");
        }
    }
}