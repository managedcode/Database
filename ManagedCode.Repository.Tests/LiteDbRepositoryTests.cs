using System;
using System.IO;
using ManagedCode.Repository.LiteDB;
using ManagedCode.Repository.Tests.Common;

namespace ManagedCode.Repository.Tests
{
    public class LiteDbRepositoryTests : BaseRepositoryTests<string, TestLiteDbItem>
    {
        public const string ConnectionString = "litedb_test.db";

        public LiteDbRepositoryTests() : base(new LiteDbRepository<string, TestLiteDbItem>(new LiteDbRepositoryOptions
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

        protected override string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}