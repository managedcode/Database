using System;
using System.IO;
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
        private readonly string _databasePath;

        public LiteDbQueryableTests()
        {
            _databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

            _database = new LiteDBDatabase(new LiteDBOptions
            {
                ConnectionString = _databasePath,
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
            File.Delete(_databasePath);
        }

    }
}