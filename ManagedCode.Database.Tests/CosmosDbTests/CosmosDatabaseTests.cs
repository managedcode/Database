using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Cosmos;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace ManagedCode.Database.Tests.CosmosDbTests
{
    public class CosmosDatabaseTests : BaseDatabaseTests<string, TestCosmosItem>
    {
        public CosmosDatabaseTests() : base(new CosmosTestContainer())
        {
        }

        [Fact]
        public override async Task FindOrderThen()
        {
            Func<Task> act = () => base.FindOrderThen();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage(
                    "*The order by query does not have a corresponding composite index that it can be served from*");
        }

        [Fact]
        public override async Task Insert99Items()
        {
            Func<Task> act = () => base.Insert99Items();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Resource with specified id or name already exists*");
        }

        [Fact]
        public override async Task UpdateOneItem()
        {
            Func<Task> act = () => base.UpdateOneItem();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Resource Not Found*");
        }

        [Fact]
        public override async Task InsertOneItem()
        {
            Func<Task> act = () => base.InsertOneItem();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Resource with specified id or name already exists*");
        }

        [Fact]
        public override async Task Update5Items()
        {
            Func<Task> act = () => base.Update5Items();

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Resource Not Found*");
        }
    }
}