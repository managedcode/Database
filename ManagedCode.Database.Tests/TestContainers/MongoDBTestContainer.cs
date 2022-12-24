using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.MongoDB;
using ManagedCode.Database.Tests.Common;
using MongoDB.Bson;

namespace ManagedCode.Database.Tests.TestContainers;

public class MongoDBTestContainer : ITestContainer<ObjectId, TestMongoDBItem>
{
    private readonly MongoDBDatabase _dbDatabase;
    private readonly TestcontainersContainer _mongoDBContainer;

    public MongoDBTestContainer()
    {
        _mongoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo")
            .WithPortBinding(27017, true)
            .WithCleanUp(true)
            //.WithWaitStrategy(Wait.ForUnixContainer()
             //   .UntilPortIsAvailable(27017))
            .Build();
        
        _dbDatabase = new MongoDBDatabase(new MongoDBOptions()
        {
            ConnectionString = $"mongodb://localhost:{_mongoDBContainer.GetMappedPublicPort(27017)}",
            DataBaseName = "db"
        });

        
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
        await _dbDatabase.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbDatabase.DisposeAsync();
        await _mongoDBContainer.StopAsync();
    }
}