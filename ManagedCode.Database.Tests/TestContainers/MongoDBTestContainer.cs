using System;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
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
    private readonly ITestOutputHelper _testOutputHelper;
    private MongoDBDatabase _dbDatabase;
    private ContainerListResponse _mongoContainer;
    private TestcontainersContainer _mongoDBContainer;
    private string containerName = "mongo3026f50d661c40d699a97de27eafe7e";
    private bool containerExsist = false;

    public MongoDBTestContainer(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
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

            containerExsist = false;
        }
        catch (Exception ex)
        {
            containerExsist = true;
        }

        if (!containerExsist)
        {
            publicPort = _mongoDBContainer.GetMappedPublicPort(27017);
        }
        else
        {
            DockerClient _client = new DockerClientConfiguration().CreateClient();

            var listContainers = await _client.Containers.ListContainersAsync(new ContainersListParameters());

            foreach (var container in listContainers)
            {
                foreach (var name in container.Names) //TODO edit foreach -> .constain
                {
                    if (name == "/" + containerName)
                        _mongoContainer = container;
                }
            }

            foreach (var port in _mongoContainer.Ports) //TODO edit foreach -> .constain
            {
                if (port.PrivatePort == 27017)
                {
                    publicPort = port.PublicPort;
                    break;
                }
            }
        }

        _dbDatabase = new MongoDBDatabase(new MongoDBOptions()
        {
            ConnectionString = $"mongodb://localhost:{publicPort}",
            DataBaseName = "db"
        });

        await _dbDatabase.InitializeAsync();

        /*
                _testOutputHelper.WriteLine($"Mongo container State:{_mongoContainer.State}");
                _testOutputHelper.WriteLine("=START=");*/
    }

    public async Task DisposeAsync()
    {
        await _dbDatabase.DisposeAsync();
        // await _mongoDBContainer.StopAsync();
        //await _mongoDBContainer.CleanUpAsync();

        // _testOutputHelper.WriteLine($"Mongo container State:{_mongoDBContainer.State}");
        //_testOutputHelper.WriteLine("=STOP=");
    }
}