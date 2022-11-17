using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.MongoDB;
using ManagedCode.Database.Tests.Common;
using MongoDB.Bson;

namespace ManagedCode.Database.Tests.TestContainers;

public class MongoDBTestContainer : ITestContainer<ObjectId, TestMongoDbItem>
{
    private static int _port = 27017;
    private readonly MongoDBDatabase _dbDatabase;
    private readonly TestcontainersContainer _mongoDBContainer;

    public MongoDBTestContainer()
    {
        var port = ++_port;
        _dbDatabase = new MongoDBDatabase(new MongoDBOptions()
        {
            ConnectionString = $"mongodb://localhost:{port}",
            DataBaseName = "db"
        });

        _mongoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo")
            .WithPortBinding(port, 27017)
            .Build();
    }

    public IDatabaseCollection<ObjectId, TestMongoDbItem> Collection =>
        _dbDatabase.GetCollection<TestMongoDbItem>();

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