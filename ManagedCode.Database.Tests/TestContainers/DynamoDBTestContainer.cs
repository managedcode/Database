using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core;
using ManagedCode.Database.DynamoDB;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests.TestContainers;

public class DynamoDBTestContainer : ITestContainer<string, TestDynamoDbItem>
{
    private DynamoDBDatabase _dbDatabase;
    private readonly TestcontainersContainer _dynamoDBContainer;

    public DynamoDBTestContainer()
    {
        _dynamoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("amazon/dynamodb-local")
            .WithName($"dynamodb{Guid.NewGuid().ToString("N")}")
            .WithPortBinding(8000, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(8000))
            .Build();
    }

    public IDatabaseCollection<string, TestDynamoDbItem> Collection =>
        _dbDatabase.GetCollection<TestDynamoDbItem>();

    public async Task InitializeAsync()
    {
        await _dynamoDBContainer.StartAsync();
        Console.WriteLine($"DynamoDB container State:{_dynamoDBContainer.State}");
        
        _dbDatabase = new DynamoDBDatabase(new DynamoDBOptions()
        {
            ServiceURL = $"http://localhost:{_dynamoDBContainer.GetMappedPublicPort(8000)}",
            AuthenticationRegion = "eu-central-1",
            AccessKey = $"AccessKey",
            SecretKey = $"SecretKey",
            DataBaseName = "db"
        });

        await _dbDatabase.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbDatabase.DisposeAsync();
        await _dynamoDBContainer.StopAsync();
        await _dynamoDBContainer.CleanUpAsync();
        Console.WriteLine($"DynamoDB container State:{_dynamoDBContainer.State}");
    }

    public string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }
}