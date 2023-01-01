using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.Cosmos;
using ManagedCode.Database.Tests.Common;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace ManagedCode.Database.Tests.TestContainers;


public class CosmosDockerContainer : IAsyncLifetime
{
    public TestcontainersContainer CosmosContainer { get; private set; }

    public CosmosDockerContainer()
    {
        CosmosContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator")
            //.WithName($"azure-cosmos-emulator{Guid.NewGuid().ToString("N")}")
            .WithName($"azure-cosmos-emulator")
            .WithExposedPort(8081)
            .WithPortBinding(8081,   8081)
            .WithPortBinding(10250, 10250)
            .WithPortBinding(10251, 10251)
            .WithPortBinding(10252, 10252)
            .WithPortBinding(10253, 10253)
            .WithPortBinding(10254, 10254)
            .WithPortBinding(10255, 10255)
            .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
            //.WithCleanUp(true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(8081))
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await CosmosContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await CosmosContainer.StopAsync();
        await CosmosContainer.CleanUpAsync();
    }
}



public class CosmosTestContainer : ITestContainer<string, TestCosmosItem>
{
    private CosmosDatabase _database;
    private readonly TestcontainersContainer _cosmosContainer;
    private readonly string _collectionName = "testContainer"; 
    private readonly string _databaseName = "database";

    public CosmosTestContainer(CosmosDockerContainer container)
    {
        _cosmosContainer = container.CosmosContainer;
    }
    
    public IDatabaseCollection<string, TestCosmosItem> Collection =>
        _database.GetCollection<TestCosmosItem>();

    public string GenerateId()
    {
        return $"{Guid.NewGuid():N}";
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine($"Cosmos container State:{_cosmosContainer.State}");
        _database = new CosmosDatabase(new CosmosOptions
        {
            ConnectionString =
                $"AccountEndpoint=https://localhost:{_cosmosContainer.GetMappedPublicPort(8081)}/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            DatabaseName = _databaseName,
            CollectionName = _collectionName + Guid.NewGuid().ToString("N"),
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
        Console.WriteLine($"Cosmos container State:{_cosmosContainer.State}");
    }
}