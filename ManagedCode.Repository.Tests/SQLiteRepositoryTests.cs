using System;
using System.IO;
using ManagedCode.Repository.SQLite;
using ManagedCode.Repository.Tests.Common;

namespace ManagedCode.Repository.Tests
{
    public class SQLiteRepositoryTests : BaseRepositoryTests<int, SQLiteDbItem>
    {
        public const string ConnectionString = "sqlite_test.db";

        private static int _count;

        public SQLiteRepositoryTests() : base(new SQLiteRepository<int, SQLiteDbItem>(new SQLiteRepositoryOptions
        {
            ConnectionString = GetTempDbName()
        }))
        {
            Repository.InitializeAsync().Wait();
        }

        private static string GetTempDbName()
        {
            return Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ConnectionString);
        }

        protected override int GenerateId()
        {
            _count++;
            return _count;
        }
    }
}