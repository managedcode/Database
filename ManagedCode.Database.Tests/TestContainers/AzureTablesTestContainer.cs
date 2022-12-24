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
    private readonly AzureTablesDatabase _database;
    private readonly TestcontainersContainer _azureTablesContainer;

    public AzureTablesTestContainer()
    {
        _azureTablesContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .WithPortBinding(10000, 10000)
            .WithPortBinding(10001, 10001)
            .WithPortBinding(10002, 10002)
            .WithCleanUp(true)
            .Build();
        
        _database = new AzureTablesDatabase(new AzureTablesOptions
        {
            ConnectionString =
                $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:{ _azureTablesContainer.GetMappedPublicPort(10000)}/devstoreaccount1;QueueEndpoint=http://localhost:{_azureTablesContainer.GetMappedPublicPort(10001)}/devstoreaccount1;TableEndpoint=http://localhost:{_azureTablesContainer.GetMappedPublicPort(10002)}/devstoreaccount1;",
            AllowTableCreation = true,
        });
       
    }

    public async Task InitializeAsync()
    {
        await _azureTablesContainer.StartAsync();
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await _azureTablesContainer.StopAsync();
    }

    public IDatabaseCollection<TableId, TestAzureTablesItem> Collection =>
        _database.GetCollection<TableId, TestAzureTablesItem>();

    public TableId GenerateId()
    {
        return new TableId($"{Guid.NewGuid():N}", $"{Guid.NewGuid():N}");
    }
}