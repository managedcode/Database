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
using Xunit;

namespace ManagedCode.Database.Tests.TestContainers;

[CollectionDefinition(nameof(CosmosTestContainer))]
public class CosmosTestContainer : ITestContainer<string, TestCosmosItem>,
    ICollectionFixture<CosmosTestContainer>, IDisposable
{
    private readonly TestcontainersContainer _cosmosTestContainer;
    private CosmosDatabase _database;
    private DockerClient _dockerClient;
    private const string containerName = "cosmosContainer";
    private const ushort privatePort = 8081;
    private bool containerExsist = false;
    private string containerId;

    public CosmosTestContainer()
    {
        _cosmosTestContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator")
            .WithName(containerName)
            .WithExposedPort(8081)
            .WithPortBinding(8081, 8081)
            .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
            .WithCleanUp(false)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(privatePort))
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
        ushort publicPort = privatePort;

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
            containerId = _cosmosTestContainer.Id;
        }
        else
        {
            var listContainers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters());

            ContainerListResponse containerListResponse = listContainers.FirstOrDefault(container => container.Names.Contains($"/{containerName}"));

            if (containerListResponse != null)
            {
                publicPort = containerListResponse.Ports.Single(port => port.PrivatePort == privatePort).PublicPort;

                containerId = containerListResponse.ID;
            }
        }

        var httpMessageHandler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };


        _database = new CosmosDatabase(new CosmosOptions
        {
            ConnectionString =
                $"AccountEndpoint=https://localhost:{publicPort}/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            DatabaseName = "database",
            CollectionName = $"testContainer",
            AllowTableCreation = true,
            CosmosClientOptions = new CosmosClientOptions()
            {
                HttpClientFactory = () => new HttpClient(httpMessageHandler),
                ConnectionMode = ConnectionMode.Gateway,
                RequestTimeout = TimeSpan.FromMinutes(3)
            },
        });


        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        // await _database.DeleteAsync();
        await _database.DisposeAsync();

        /*     _testOutputHelper.WriteLine($"Cosmos container State:{_cosmosContainer.State}");
             _testOutputHelper.WriteLine("=STOP=");*/
    }

    public async void Dispose()
    {

        await _dockerClient.Containers.RemoveContainerAsync(containerId,
              new ContainerRemoveParameters
              {
                  Force = true
              });
    }
}
