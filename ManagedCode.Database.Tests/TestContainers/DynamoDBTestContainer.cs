using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core;
using ManagedCode.Database.DynamoDB;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests.TestContainers;

public class DynamoDBTestContainer : ITestContainer<string, TestDynamoDbItem>

{
    private static int _port = 8000;
    private readonly DynamoDBDatabase _dbDatabase;
    private readonly TestcontainersContainer _dynamoDBContainer;
    private readonly string _tableName = "testTable";

    public DynamoDBTestContainer()
    {
        var port = ++_port;
        _dbDatabase = new DynamoDBDatabase(new DynamoDBOptions()
        {
            ServiceURL = $"http://localhost:{port}",
            AuthenticationRegion = "ap-southrast-2",
            AccessKey = $"AccessKey",
            SecretKey = $"SecretKey",
            DataBaseName = "db"
        });

        _dynamoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("amazon/dynamodb-local")
            .WithPortBinding(port, 8000)
            .Build();
    }

    public IDatabaseCollection<string, TestDynamoDbItem> Collection =>
        _dbDatabase.GetCollection<TestDynamoDbItem>(_tableName);

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

    public string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }
}