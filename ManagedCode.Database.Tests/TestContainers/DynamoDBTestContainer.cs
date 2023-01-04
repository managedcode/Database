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
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.TestContainers;

public class DynamoDBTestContainer : ITestContainer<string, TestDynamoDbItem>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private DynamoDBDatabase _dbDatabase;
    private readonly TestcontainersContainer _dynamoDBContainer;

    public DynamoDBTestContainer(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
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

        _testOutputHelper.WriteLine($"DynamoDb container State:{_dynamoDBContainer.State}");
        _testOutputHelper.WriteLine("=START=");

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

        _testOutputHelper.WriteLine($"DynamoDb container State:{_dynamoDBContainer.State}");
        _testOutputHelper.WriteLine("=STOP=");
    }

    public string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }
}