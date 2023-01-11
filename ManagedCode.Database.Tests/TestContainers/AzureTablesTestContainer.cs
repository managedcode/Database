using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.TestContainers;

[CollectionDefinition(nameof(AzureTablesTestContainer))]
public class AzureTablesTestContainer : ITestContainer<TableId, TestAzureTablesItem>, ICollectionFixture<AzureTablesTestContainer>, IDisposable
{
    //private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestcontainersContainer _azureTablesTestContainer;
    private AzureTablesDatabase _database;
    private DockerClient _dockerClient;
    private const string containerName = "azureTablesContainer";
    private readonly Dictionary<int, ushort> privatePort = new Dictionary<int, ushort>
    {
        {1, 10000},
        {2, 10001},
        {3, 10002}
    };
    private bool containerExsist = false;
    private string containerId;

    public AzureTablesTestContainer()
    {
        //_testOutputHelper = testOutputHelper;
        _azureTablesTestContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .WithName(containerName)
            .WithPortBinding(10000, true)
            .WithPortBinding(10001, true)
            .WithPortBinding(10002, true)
            .WithCleanUp(false)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(10002))
            .Build();

        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    public IDatabaseCollection<TableId, TestAzureTablesItem> Collection =>
        _database.GetCollection<TableId, TestAzureTablesItem>();

    public TableId GenerateId()
    {
        return new TableId($"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}");
    }

    public async Task InitializeAsync()
    {
        Dictionary<int, ushort> publicPort = new Dictionary<int, ushort>
        {
            {1, 0},
            {2, 0},
            {3, 0},
        };

        try
        {
            await _azureTablesTestContainer.StartAsync();

            containerExsist = false;
        }
        catch (Exception ex) //TODO catch name already using exception
        {
            containerExsist = true;
        }

        if (!containerExsist)
        {
            publicPort[1] = _azureTablesTestContainer.GetMappedPublicPort(privatePort[1]);
            publicPort[2] = _azureTablesTestContainer.GetMappedPublicPort(privatePort[2]);
            publicPort[3] = _azureTablesTestContainer.GetMappedPublicPort(privatePort[3]);
            
            containerId = _azureTablesTestContainer.Id;
        }
        else
        {
            var listContainers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters());

            ContainerListResponse containerListResponse = listContainers.Single(container => container.Names.Contains($"/{containerName}"));

            containerId = containerListResponse.ID;

            publicPort[1] = containerListResponse.Ports.Single(port => port.PrivatePort == privatePort[1]).PublicPort;
            publicPort[2] = containerListResponse.Ports.Single(port => port.PrivatePort == privatePort[2]).PublicPort;
            publicPort[3] = containerListResponse.Ports.Single(port => port.PrivatePort == privatePort[3]).PublicPort;
        }

        //_testOutputHelper.WriteLine("=START=");
        //_testOutputHelper.WriteLine($"Azure Tables container State:{_azureTablesContainer.State}");

        _database = new AzureTablesDatabase(new AzureTablesOptions
        {
            ConnectionString =
                $"DefaultEndpointsProtocol=http;" +
                $"AccountName=devstoreaccount1;" +
                $"AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
                $"BlobEndpoint=http://localhost:{publicPort[1]}/devstoreaccount1;" +
                $"QueueEndpoint=http://localhost:{publicPort[2]}/devstoreaccount1;" +
                $"TableEndpoint=http://localhost:{publicPort[3]}/devstoreaccount1;",
            AllowTableCreation = true,
            TableName = $"table{Guid.NewGuid().ToString("N")}",
        });
        
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();

        //_testOutputHelper.WriteLine($"Azure Tables container State:{_azureTablesContainer.State}");
        //_testOutputHelper.WriteLine("=STOP=");
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