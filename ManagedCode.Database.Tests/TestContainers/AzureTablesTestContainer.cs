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
    private static int _port = 10000;
    private readonly AzureTablesDatabase _database;
    private readonly TestcontainersContainer _azureTablesContainer;

    public AzureTablesTestContainer()
    {
        int[] ports = { ++_port, ++_port, ++_port };

        _database = new AzureTablesDatabase(new AzureTablesOptions
        {
            ConnectionString =
                $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:{ports[0]}/devstoreaccount1;QueueEndpoint=http://localhost:{ports[1]}/devstoreaccount1;TableEndpoint=http://localhost:{ports[2]}/devstoreaccount1;",
            AllowTableCreation = true,
        });

        _azureTablesContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .WithPortBinding(ports[0], 10000)
            .WithPortBinding(ports[1], 10001)
            .WithPortBinding(ports[2], 10002)
            .WithCleanUp(true)
            .Build();
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