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
    private MongoDBDatabase _dbDatabase;
    private readonly TestcontainersContainer _mongoDBContainer;

    public MongoDBTestContainer()
    {
        _mongoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo")
            .WithName($"mongo{Guid.NewGuid().ToString("N")}")
            .WithPortBinding(27017, true)
            //.WithCleanUp(true)
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
        await _mongoDBContainer.StartAsync();
        Console.WriteLine($"MongoContainer State:{_mongoDBContainer.State}");
        
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
        
    }
}