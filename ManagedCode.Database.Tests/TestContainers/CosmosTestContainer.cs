using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.Cosmos;
using ManagedCode.Database.Tests.Common;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.Tests.TestContainers;

public class CosmosTestContainer : ITestContainer<string, TestCosmosItem>
{
    private static int _port = 20000;

    private readonly CosmosDatabase _database;
    private readonly TestcontainersContainer _cosmosContainer;

    public CosmosTestContainer()
    {
        var port = ++_port;

        // Docker container for cosmos db is not working at all, to test database use local windows emulator
        //_cosmosContainer = new TestcontainersBuilder<TestcontainersContainer>()
        //    .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
        //    .WithPortBinding(port, 8081)
        //    .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "30")
        //    .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
        //    .WithWaitStrategy(Wait.ForUnixContainer())
        //    .Build();

        _database = new CosmosDatabase(new CosmosOptions
        {
            ConnectionString =
                $"AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
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

    public IDatabaseCollection<string, TestCosmosItem> Collection =>
        _database.GetCollection<TestCosmosItem>();

    public string GenerateId()
    {
        return $"{Guid.NewGuid():N}";
    }

    public async Task InitializeAsync()
    {
        //await _cosmosContainer.StartAsync();
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        //+await _database.DisposeAsync();
        //await _cosmosContainer.StopAsync();
    }
}