using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.Cosmos;
using ManagedCode.Database.Tests.Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
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
        var outputConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
        var waitStrategy = Wait.ForUnixContainer().UntilMessageIsLogged(outputConsumer.Stdout, "Started");

        _cosmosTestContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator")
            .WithName(containerName)
            //.WithName($"azure-cosmos-emulator{Guid.NewGuid().ToString("N")}")
            .WithExposedPort(8081)
            .WithExposedPort(10251)
            .WithExposedPort(10252)
            .WithExposedPort(10253)
            .WithExposedPort(10254)
            .WithPortBinding(8081, true)
            .WithPortBinding(10251, true)
            .WithPortBinding(10252, true)
            .WithPortBinding(10253, true)
            .WithPortBinding(10254, true)
            .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "30")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
            .WithOutputConsumer(outputConsumer)
            .WithWaitStrategy(waitStrategy)
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
            DatabaseName = $"db{Guid.NewGuid().ToString("N")}",
            CollectionName = "testContainer",
            AllowTableCreation = true,
            CosmosClientOptions = new CosmosClientOptions()
            {
                HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    
                    return new HttpClient(httpMessageHandler);
                },
                ConnectionMode = ConnectionMode.Gateway
            },
        });

        await _database.InitializeAsync();
        
        //_testOutputHelper.WriteLine("=START=");
        //_testOutputHelper.WriteLine($"Cosmos container State:{_cosmosContainer.State}");

    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        //await _cosmosContainer.StopAsync();
        //await _cosmosContainer.CleanUpAsync();

        //_testOutputHelper.WriteLine($"Cosmos container State:{_cosmosContainer.State}");
        //_testOutputHelper.WriteLine("=STOP=");
    }
}