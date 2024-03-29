using System;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.DynamoDB;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.TestContainers;

[CollectionDefinition(nameof(DynamoDBTestContainer))]
public class DynamoDBTestContainer : ITestContainer<string, TestDynamoDbItem>, 
    ICollectionFixture<DynamoDBTestContainer>, IDisposable
{
   // private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestcontainersContainer _dynamoDBTestContainer;
    private DynamoDBDatabase _dbDatabase;
    private DockerClient _dockerClient;
    private const string containerName = "dynamoContainer";
    private const ushort privatePort = 8000;
    private bool containerExsist = false;
    private string containerId;

    public DynamoDBTestContainer()
    {
        //_testOutputHelper = testOutputHelper;
        _dynamoDBTestContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("amazon/dynamodb-local")
            .WithName(containerName)
            .WithPortBinding(privatePort, true)
            .WithCleanUp(false)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(privatePort))
            .Build();

        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    public IDatabaseCollection<string, TestDynamoDbItem> Collection =>
        _dbDatabase.GetCollection<TestDynamoDbItem>();

    public string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }

    public async Task InitializeAsync()
    {
        ushort publicPort = 0;

        try
        {
            await _dynamoDBTestContainer.StartAsync();

            containerExsist = false;
        }
        catch (Exception ex) //TODO catch name already using exception
        {
            containerExsist = true;
        }

        if (!containerExsist)
        {
            publicPort = _dynamoDBTestContainer.GetMappedPublicPort(privatePort);

            containerId = _dynamoDBTestContainer.Id;
        }
        else
        {
            var listContainers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters());

            ContainerListResponse containerListResponse = listContainers.Single(container => container.Names.Contains($"/{containerName}"));

            if (containerListResponse != null)
            {
                publicPort = containerListResponse.Ports.Single(port => port.PrivatePort == privatePort).PublicPort;

                containerId = containerListResponse.ID;
            }
        }

        _dbDatabase = new DynamoDBDatabase(new DynamoDBOptions()
        {
            ServiceURL = $"http://localhost:{publicPort}",
            AuthenticationRegion = "eu-central-1",
            AccessKey = $"AccessKey",
            SecretKey = $"SecretKey",
            DataBaseName = $"db{Guid.NewGuid().ToString("N")}",
            CollectionName = $"collection{Guid.NewGuid().ToString("N")}",
        });

        await _dbDatabase.InitializeAsync();

        /*_testOutputHelper.WriteLine($"DynamoDb container State:{_dynamoDBTestContainer.State}");
        _testOutputHelper.WriteLine("=START=");*/
    }

    public async Task DisposeAsync()
    {
        await _dbDatabase.DisposeAsync();

        //_testOutputHelper.WriteLine($"DynamoDb container State:{_dynamoDBContainer.State}");
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