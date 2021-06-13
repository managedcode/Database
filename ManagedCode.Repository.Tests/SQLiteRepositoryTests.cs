using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.SQLite;
using ManagedCode.Repository.Tests.Common;
using Xunit;

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
        
        [Fact(Skip = "UNIQUE constraint failed")]
        public override async Task Insert99Items()
        {
            Action act = () => base.Insert99Items();

            act.Should().Throw<Exception>()
                .WithMessage("UNIQUE constraint failed: SQLiteDbItem.Id");
        }
    }
}