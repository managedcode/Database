using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.AzureTablesTests
{
    public class AzureTablesDatabaseTests : BaseDatabaseTests<TableId, TestAzureTablesItem>, IAsyncLifetime
    {
        private readonly AzureTablesTestContainer _azureTablesContainer;

        public AzureTablesDatabaseTests()
        {
            _azureTablesContainer = new AzureTablesTestContainer();
        }


        protected override IDatabaseCollection<TableId, TestAzureTablesItem> Collection =>
            _azureTablesContainer.GetCollection();

        protected override TableId GenerateId()
        {
            return _azureTablesContainer.GenerateId();
        }

        public override async Task DisposeAsync()
        {
            await _azureTablesContainer.DisposeAsync();
        }

        public override async Task InitializeAsync()
        {
            await _azureTablesContainer.InitializeAsync();
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