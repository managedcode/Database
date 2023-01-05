using System;
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
    private MongoDBDatabase _dbDatabase;
    private DockerClient _client;

    public MongoDBTestContainer(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        DockerClient client = new DockerClientConfiguration()
            .CreateClient();
        
        client.
        
        
        // _mongoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
        //     .WithImage("mongo")
        //     .WithName($"mongo{Guid.NewGuid().ToString("N")}")
        //     .WithPortBinding(27017, true)
        //     //.WithCleanUp(true)
        //     .WithWaitStrategy(Wait.ForUnixContainer()
        //         .UntilPortIsAvailable(27017))
        //     .Build();
    }

    public IDatabaseCollection<ObjectId, TestMongoDBItem> Collection =>
        _dbDatabase.GetCollection<TestMongoDBItem>();

    public ObjectId GenerateId()
    {
        return ObjectId.GenerateNewId();
    }

    public async Task InitializeAsync()
    {
        await _client.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Image = "mongo",
            HostConfig = new HostConfig()
            {
                DNS = new[] { "8.8.8.8", "8.8.4.4" }
            }
        });

        _testOutputHelper.WriteLine($"Mongo container State:{_mongoDBContainer.State}");
        _testOutputHelper.WriteLine("=START=");

        _dbDatabase = new MongoDBDatabase(new MongoDBOptions()
        {
            ConnectionString = $"mongodb://localhost:{_mongoDBContainer.GetMappedPublicPort(27017)}",
            DataBaseName = "db"
        });
        
        await _dbDatabase.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbDatabase.DisposeAsync();
        await _mongoDBContainer.StopAsync();
        await _mongoDBContainer.CleanUpAsync();

        _testOutputHelper.WriteLine($"Mongo container State:{_mongoDBContainer.State}");
        _testOutputHelper.WriteLine("=STOP=");
    }
}