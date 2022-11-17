using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.AzureTable;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.AzureTableTests
{
    public class AzureTableDatabaseTests : BaseDatabaseTests<TableId, TestAzureTableItem>, IAsyncLifetime
    {
        private readonly AzureTableTestContainer _azureTableContainer;

        public AzureTableDatabaseTests()
        {
            _azureTableContainer = new AzureTableTestContainer();
        }


        protected override IDatabaseCollection<TableId, TestAzureTableItem> Collection =>
            _azureTableContainer.GetCollection();

        protected override TableId GenerateId()
        {
            return _azureTableContainer.GenerateId();
        }

        public override async Task DisposeAsync()
        {
            await _azureTableContainer.DisposeAsync();
        }

        public override async Task InitializeAsync()
        {
            await _azureTableContainer.InitializeAsync();
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