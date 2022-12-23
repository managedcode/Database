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


    private readonly CosmosDatabase _database;
    private readonly TestcontainersContainer _cosmosContainer;

    public CosmosTestContainer()
    {

        // Docker container for cosmos db is not working at all, to test database use local windows emulator
        _cosmosContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator")
            .WithName("azure-cosmos-emulator")
            .WithExposedPort(8081)
            .WithPortBinding(8081, true)
            .WithPortBinding(10251, true)
            .WithPortBinding(10252, true)
            .WithPortBinding(10253, true)
            .WithPortBinding(10254, true)
            .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(8081))
            .Build();

        var mappedPort = _cosmosContainer.GetMappedPublicPort(8081);
        _database = new CosmosDatabase(new CosmosOptions
        {
            ConnectionString =
                $"AccountEndpoint=https://localhost:{mappedPort}/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
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
    }
    
    /*
    await Task.Delay(TimeSpan.FromSeconds(30))
    .ConfigureAwait(false);

        using (var handler = new HttpClientHandler())
    {
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        using (var client = new HttpClient(handler))
        {
            var mappedPort = this.container.GetMappedPublicPort(8081);

            var response = await client.GetAsync($"https://localhost:{mappedPort}/_explorer/emulator.pem")
                .ConfigureAwait(false);

            var pem = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            Debug.WriteLine(pem);
        }
    }
    
    
    
    */
    

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
        await _database.DisposeAsync();
        //await _cosmosContainer.StopAsync();
    }
}