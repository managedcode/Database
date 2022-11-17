using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.AzureTablesTests
{
    public class AzureTablesDatabaseTests : BaseDatabaseTests<TableId, TestAzureTablesItem>
    {
        public AzureTablesDatabaseTests() : base(new AzureTablesTestContainer())
        {
        }

        [Fact]
        public override async Task InsertOneItem()
        {
            var id = GenerateId();
            var firstItem = CreateNewItem(id);
            var secondItem = CreateNewItem(id);

            //  SEPARATE TESTS IN TWO
            var insertFirst = await Collection.InsertAsync(firstItem);
            var insertSecond = async () => await Collection.InsertAsync(secondItem);

            // TODO: check test
            insertFirst.Should().NotBeNull();
        }
    }
}