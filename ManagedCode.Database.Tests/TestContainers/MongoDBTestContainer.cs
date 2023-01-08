using System;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.MongoDB;
using ManagedCode.Database.Tests.Common;
using MongoDB.Bson;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.TestContainers;

public class MongoDBTestContainer : ITestContainer<ObjectId, TestMongoDBItem>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestcontainersContainer _mongoDBTestContainer;
    private MongoDBDatabase _dbDatabase;
    private DockerClient _dockerClient;
    private const string containerName = "mongoContainer";
    private const ushort privatePort = 27017;
    private bool containerExsist = false;

    public MongoDBTestContainer(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _mongoDBTestContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo")
            .WithName(containerName)
            .WithPortBinding(privatePort, true)
            .WithCleanUp(false)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(privatePort))
            .Build();

        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    public IDatabaseCollection<ObjectId, TestMongoDBItem> Collection =>
        _dbDatabase.GetCollection<TestMongoDBItem>();

    public ObjectId GenerateId()
    {
        return ObjectId.GenerateNewId();
    }

    public async Task InitializeAsync()
    {
        ushort publicPort = 0;

        try
        {
            await _mongoDBTestContainer.StartAsync();

            containerExsist = false;
        }
        catch (Exception ex) //TODO catch name already using exception
        {
            containerExsist = true;
        }

        if (!containerExsist)
        {
            publicPort = _mongoDBTestContainer.GetMappedPublicPort(privatePort);
        }
        else
        {
            var listContainers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters());

            ContainerListResponse containerListResponse = listContainers.Single(container => container.Names.Contains($"/{containerName}"));

            publicPort = containerListResponse.Ports.Single(port => port.PrivatePort == privatePort).PublicPort;
        }

        _dbDatabase = new MongoDBDatabase(new MongoDBOptions()
        {
            ConnectionString = $"mongodb://localhost:{publicPort}",
            DataBaseName = $"db{Guid.NewGuid().ToString("N")}"
        });

        await _dbDatabase.InitializeAsync();

        //_testOutputHelper.WriteLine($"Mongo container State:{_mongoContainer.State}");
        //_testOutputHelper.WriteLine("=START=");
    }

    public async Task DisposeAsync()
    {
        await _dbDatabase.DisposeAsync();
        // await _mongoDBContainer.StopAsync();
        //await _mongoDBContainer.CleanUpAsync();

        // _testOutputHelper.WriteLine($"Mongo container State:{_mongoDBContainer.State}");
        //_testOutputHelper.WriteLine("=STOP=");
    }
}