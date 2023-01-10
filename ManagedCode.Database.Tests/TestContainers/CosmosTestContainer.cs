using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.Cosmos;
using ManagedCode.Database.Tests.Common;
using Microsoft.Azure.Cosmos;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.TestContainers;

public class CosmosTestContainer : ITestContainer<string, TestCosmosItem>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestcontainersContainer _cosmosTestContainer;
    private CosmosDatabase _database;
    private DockerClient _dockerClient;
    private const string containerName = "cosmosContainer";
    private const ushort privatePort = 8081;
    private bool containerExsist = false;

    public CosmosTestContainer(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        // Docker container for cosmos db is not working at all, to test database use local windows emulator
        _cosmosTestContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator")
            //.WithName(containerName)
            .WithName($"azure-cosmos-emulator{Guid.NewGuid().ToString("N")}")
            .WithExposedPort(8081)
            .WithPortBinding(8081, 8081)
            .WithPortBinding(10250, 10250)
            .WithPortBinding(10251, 10251)
            .WithPortBinding(10252, 10252)
            .WithPortBinding(10253, 10253)
            .WithPortBinding(10254, 10254)
            .WithPortBinding(10255, 10255)
            .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
            .WithCleanUp(false)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(8081))
            .Build();

        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    public IDatabaseCollection<string, TestCosmosItem> Collection =>
        _database.GetCollection<TestCosmosItem>();

    public string GenerateId()
    {
        return $"{Guid.NewGuid():N}";
    }

    public async Task InitializeAsync()
    {
        ushort publicPort = 0;

        try
        {
            await _cosmosTestContainer.StartAsync();

            containerExsist = false;
        }
        catch (Exception ex) //TODO catch name already using exception
        {
            containerExsist = true;
        }

        if (!containerExsist)
        {
            publicPort = _cosmosTestContainer.GetMappedPublicPort(privatePort);
        }
        else
        {
            var listContainers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters());

            ContainerListResponse containerListResponse = listContainers.Single(container => container.Names.Contains($"/{containerName}"));

            publicPort = containerListResponse.Ports.Single(port => port.PrivatePort == privatePort).PublicPort;
        }

        _database = new CosmosDatabase(new CosmosOptions
        {
            ConnectionString =
                $"AccountEndpoint=https://localhost:{publicPort}/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            DatabaseName = "database",
            CollectionName = "testContainer",
            AllowTableCreation = true,
            CosmosClientOptions = new CosmosClientOptions()
            {
                HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
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
        await _cosmosTestContainer.StopAsync();
        await _cosmosTestContainer.CleanUpAsync();
        /*     _testOutputHelper.WriteLine($"Cosmos container State:{_cosmosContainer.State}");
             _testOutputHelper.WriteLine("=STOP=");*/
    }
}