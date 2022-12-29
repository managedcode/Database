using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests.TestContainers;

public class AzureTablesTestContainer : ITestContainer<TableId, TestAzureTablesItem>
{
    private  AzureTablesDatabase _database;
    private readonly TestcontainersContainer _azureTablesContainer;

    public AzureTablesTestContainer()
    {
        _azureTablesContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .WithName($"azure-storage{Guid.NewGuid().ToString("N")}")
            .WithPortBinding(10000, true)
            .WithPortBinding(10001, true)
            .WithPortBinding(10002, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(10002))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _azureTablesContainer.StartAsync();
        
        _database = new AzureTablesDatabase(new AzureTablesOptions
        {
            ConnectionString =
                $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:{ _azureTablesContainer.GetMappedPublicPort(10000)}/devstoreaccount1;QueueEndpoint=http://localhost:{_azureTablesContainer.GetMappedPublicPort(10001)}/devstoreaccount1;TableEndpoint=http://localhost:{_azureTablesContainer.GetMappedPublicPort(10002)}/devstoreaccount1;",
            AllowTableCreation = true,
        });
        
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await _azureTablesContainer.StopAsync();    
        await _azureTablesContainer.CleanUpAsync();
    }

    public IDatabaseCollection<TableId, TestAzureTablesItem> Collection =>
        _database.GetCollection<TableId, TestAzureTablesItem>();

    public TableId GenerateId()
    {
        return new TableId($"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}");
    }
}