using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.DynamoDB;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests.TestContainers;

public class DynamoDBTestContainer : ITestContainer<Primitive, TestDynamoDbItem>

{
    private static int _port = 27017;
    private readonly DynamoDBDatabase _dbDatabase;
    private readonly TestcontainersContainer _dynamoDBContainer;

    public DynamoDBTestContainer()
    {
        var port = ++_port;
        _dbDatabase = new DynamoDBDatabase(new DynamoDBOptions()
        {
            ConnectionString = $"dynamodb://localhost:{port}",
            DataBaseName = "db"
        });

        _dynamoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo")
            .WithPortBinding(port, 27017)
            .Build();
    }

    public IDatabaseCollection<Primitive, TestDynamoDbItem> Collection =>
        _dbDatabase.GetCollection<TestDynamoDbItem>();

    public async Task InitializeAsync()
    {
        await _dynamoDBContainer.StartAsync();
        await _dbDatabase.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbDatabase.DisposeAsync();
        await _dynamoDBContainer.StopAsync();
    }

    Primitive ITestContainer<Primitive, TestDynamoDbItem>.GenerateId()
    {
        throw new System.NotImplementedException();
    }
}