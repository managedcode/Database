using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.AzureTable;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using System;
using System.Threading.Tasks;

namespace ManagedCode.Database.Tests.AzureTableTests
{
    public class AzureTableCommandTests : BaseCommandTests<TableId, TestAzureTableItem>
    {
        private readonly AzureTableDatabase _database;
        private readonly TestcontainersContainer _azureTableContainer;

        public AzureTableCommandTests()
        {
            _database = new AzureTableDatabase(new AzureTableRepositoryOptions
            {
                ConnectionString =
                    "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;QueueEndpoint=http://localhost:10001/devstoreaccount1;TableEndpoint=http://localhost:10002/devstoreaccount1;"
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
        protected override TableId GenerateId() =>
            new TableId(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        public override async Task DisposeAsync()
        {
            await _database.DisposeAsync();
            await _azureTableContainer.StopAsync();
        }

        public override async Task InitializeAsync()
        {
            await _azureTableContainer.StartAsync();
            await _database.InitializeAsync();
        }
    }
}
