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
    private CosmosDatabase _database;
    private readonly TestcontainersContainer _cosmosContainer;

    public CosmosTestContainer()
    {
        // Docker container for cosmos db is not working at all, to test database use local windows emulator
        _cosmosContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator")
            .WithName($"azure-cosmos-emulator{Guid.NewGuid().ToString("N")}")
            .WithExposedPort(8081)
            .WithPortBinding(8081, true)
            .WithPortBinding(10251, true)
            .WithPortBinding(10252, true)
            .WithPortBinding(10253, true)
            .WithPortBinding(10254, true)
            .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
            //.WithCleanUp(true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(8081))
            .Build();
    }
    
    public IDatabaseCollection<string, TestCosmosItem> Collection =>
        _database.GetCollection<TestCosmosItem>();

    public string GenerateId()
    {
        return $"{Guid.NewGuid():N}";
    }

    public async Task InitializeAsync()
    {
        await _cosmosContainer.StartAsync();
        Console.WriteLine($"Cosmos container State:{_cosmosContainer.State}");
        _database = new CosmosDatabase(new CosmosOptions
        {
            ConnectionString =
                $"AccountEndpoint=https://localhost:{_cosmosContainer.GetMappedPublicPort(8081)}/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            DatabaseName = "database",
            CollectionName = "testContainer",
            AllowTableCreation = true,
            CosmosClientOptions = new CosmosClientOptions()
            {
                HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback =  (_, _, _, _) => true
                    };
                    
                    return new HttpClient(httpMessageHandler);
                },
                ConnectionMode = ConnectionMode.Gateway
            },
        });
        
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await _cosmosContainer.StopAsync();
        await _cosmosContainer.CleanUpAsync();
        Console.WriteLine($"Cosmos container State:{_cosmosContainer.State}");
    }
}