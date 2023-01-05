using System;
using System.Threading.Tasks;
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
    private readonly TestcontainersContainer _mongoDBContainer;

    public MongoDBTestContainer(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _mongoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo")
            .WithName($"mongo{Guid.NewGuid().ToString("N")}")
            .WithPortBinding(27017, true)
            .WithCleanUp(false)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(27017))
            .Build();


    }

    public IDatabaseCollection<ObjectId, TestMongoDBItem> Collection =>
        _dbDatabase.GetCollection<TestMongoDBItem>();

    public ObjectId GenerateId()
    {
        return ObjectId.GenerateNewId();
    }

    public async Task InitializeAsync()
    {
        //await _mongoDBContainer.StartAsync();

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
       // await _mongoDBContainer.StopAsync();
        //await _mongoDBContainer.CleanUpAsync();

        _testOutputHelper.WriteLine($"Mongo container State:{_mongoDBContainer.State}");
        _testOutputHelper.WriteLine("=STOP=");
    }
}