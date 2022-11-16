using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Cosmos;
using ManagedCode.Database.Tests.Common;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace ManagedCode.Database.Tests.CosmosDbTests
{
    public class CosmosDbRepositoryTests : BaseRepositoryTests<string, TestCosmosItem>
    {
        private readonly CosmosDatabase _database;
        private readonly TestcontainersContainer _cosmosDBContainer;

        public CosmosDbRepositoryTests()
        {
            _cosmosDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
                .WithPortBinding(8081, 8081)
                .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "30")
                .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
                .WithWaitStrategy(Wait.ForUnixContainer())
                .Build();

            _database = new CosmosDatabase(new CosmosOptions
            {
                ConnectionString =
                    "AccountEndpoint=https://localhost:8081;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                DatabaseName = "database",
                CollectionName = "testContainer",
                AllowTableCreation = true,
                CosmosClientOptions = new CosmosClientOptions()
                {
                    HttpClientFactory = () =>
                    {
                        HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };

                        return new HttpClient(httpMessageHandler);
                    },
                    ConnectionMode = ConnectionMode.Gateway
                },
            });
        }

        protected override IDatabaseCollection<string, TestCosmosItem> Collection =>
            _database.GetCollection<TestCosmosItem>();

        protected override string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        public override async Task InitializeAsync()
        {
            await _cosmosDBContainer.StartAsync();
            await _database.InitializeAsync();
        }

        public override async Task DisposeAsync()
        {
            await _database.DisposeAsync();
            await _cosmosDBContainer.StopAsync();
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