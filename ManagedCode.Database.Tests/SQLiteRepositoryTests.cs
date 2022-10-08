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
    public class SQLiteRepositoryTests : BaseRepositoryTests<int, SQLiteDbItem>, IDisposable
    {
        public const string ConnectionString = "sqlite_test.db";

        private static int _count;
        
        private SqLiteDatabase _databaseb;

        public SQLiteRepositoryTests()
        {
            _databaseb = new SqLiteDatabase(new SQLiteRepositoryOptions
            {
                ConnectionString = GetTempDbName()
            });
            _databaseb.InitializeAsync().Wait();
            _databaseb.DataBase.CreateTable<SQLiteDbItem>();
        }

        private static string GetTempDbName()
        {
            return Path.Combine(Environment.CurrentDirectory, ConnectionString);
        }

        protected override IDBCollection<int, SQLiteDbItem> Collection => _databaseb.GetCollection<int, SQLiteDbItem>();

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

        public void Dispose()
        {
            _databaseb.DataBase.Execute("VACUUM");
            _databaseb.DataBase.Dispose();
        }
    }
}

