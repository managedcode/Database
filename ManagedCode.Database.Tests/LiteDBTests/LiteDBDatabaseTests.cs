using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.LiteDBTests;

public class LiteDBDatabaseTests : BaseDatabaseTests<string, TestLiteDBItem>
{
    public LiteDBDatabaseTests() : base(new LiteDBTestContainer())
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