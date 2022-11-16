using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using ManagedCode.Database.AzureTable;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.AzureTableTests
{
    public class AzureTableRepositoryTests : BaseRepositoryTests<TableId, TestAzureTableItem>, IAsyncLifetime
    {
        private readonly AzureTableDatabase _database;
        private readonly TestcontainersContainer _azureTableContainer;

        public AzureTableRepositoryTests()
        {
            _database = new AzureTableDatabase(new AzureTableOptions
            {
                ConnectionString =
                    "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;QueueEndpoint=http://localhost:10001/devstoreaccount1;TableEndpoint=http://localhost:10002/devstoreaccount1;",
                AllowTableCreation = true
            });

            _azureTableContainer = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("mcr.microsoft.com/azure-storage/azurite")
                .WithPortBinding(10000, 10000)
                .WithPortBinding(10001, 10001)
                .WithPortBinding(10002, 10002)
                .Build();
        }

        protected override IDBCollection<TableId, TestAzureTableItem> Collection =>
            _database.GetCollection<TableId, TestAzureTableItem>();

        protected override TableId GenerateId()
        {
            return new TableId(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        public override async Task InitializeAsync()
        {
            await _azureTableContainer.StartAsync();
            await _database.InitializeAsync();
        }

        public override async Task DisposeAsync()
        {
            await _database.DisposeAsync();
            await _azureTableContainer.StopAsync();
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