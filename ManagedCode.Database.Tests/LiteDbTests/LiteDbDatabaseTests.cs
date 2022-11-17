using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.LiteDB;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.LiteDbTests
{
    public class LiteDbDatabaseTests : BaseDatabaseTests<string, TestLiteDbItem>
    {
        public LiteDbDatabaseTests() : base(new LiteDBTestContainer())
        {
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