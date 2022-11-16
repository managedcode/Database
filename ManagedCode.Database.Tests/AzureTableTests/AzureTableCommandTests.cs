using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using ManagedCode.Database.AzureTable;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.Tests.AzureTableTests
{
    public class AzureTableCommandTests : BaseCollectionTests<TableId, TestAzureTableItem>
    {
        private readonly AzureTableDatabase _database;
        private readonly TestcontainersContainer _azureTableContainer;

        public AzureTableCommandTests()
        {
            _database = new AzureTableDatabase(new AzureTableOptions
            {
                ConnectionString =
                    "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;QueueEndpoint=http://localhost:10001/devstoreaccount1;TableEndpoint=http://localhost:10002/devstoreaccount1;",
                AllowTableCreation = true,
            });

            _azureTableContainer = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("mcr.microsoft.com/azure-storage/azurite")
                .WithPortBinding(10000, 10000)
                .WithPortBinding(10001, 10001)
                .WithPortBinding(10002, 10002)
                .Build();
        }

        protected override IDatabaseCollection<TableId, TestAzureTableItem> Collection =>
            _database.GetCollection<TableId, TestAzureTableItem>();

        protected override TableId GenerateId() =>
            new TableId(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        public override async Task DisposeAsync()
        {
            await _database.DisposeAsync();
            await _azureTableContainer.StopAsync();
        }

        public override async Task InitializeAsync()
        {
            await _azureTableContainer.StartAsync();
            await _database.InitializeAsync();
        }

        public override async Task UpdateListOfItems_WhenOnlyOneItemUpdated()
        {
            List<TestAzureTableItem> list = new();

            var id = GenerateId();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < 9; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);
            list.Clear();

            list.Add(CreateNewItem(id));

            //  ᓚᘏᗢ It throws exception if item with that Id doesn't exists
            var updatedItems = await Collection.UpdateAsync(list);

            list.Count.Should().Be(1);
            items.Should().Be(10);
            updatedItems.Should().Be(1);
        }

/*    public override async Task UpdateItem_WhenItem_DoesntExists()
    {
        var baseMethod = () => base.UpdateItem_WhenItem_DoesntExists();
        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }

    public override async Task InsertItem_WhenItemExist()
    {
        var baseMethod = () => base.InsertItem_WhenItemExist();
        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }*/

        public override async Task InsertItems_WhenOneItemAlreadyExists()
        {
            var baseMethod = () => base.InsertItems_WhenOneItemAlreadyExists();
            await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
        }

        public override async Task DeleteItemById_WhenItemDoesntExists()
        {
            var item = CreateNewItem();

            var deleteAction = async () => await Collection.DeleteAsync(item.Id);

            item.Should().NotBeNull();
            await deleteAction.Should().ThrowExactlyAsync<Exception>();
        }

        public override async Task DeleteListOfItemsById_WhenItemsDontExist()
        {
            int itemsCount = 5;
            List<TestAzureTableItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var ids = list.Select(item => item.Id);

            var deletedItemsAction = async () => await Collection.DeleteAsync(ids);

            // await deletedItemsAction.Should().ThrowExactlyAsync<StorageException>();
        }

        public override async Task DeleteListOfItems_WhenItemsDontExist()
        {
            int itemsCount = 5;
            List<TestAzureTableItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var deletedItemsAction = async () => await Collection.DeleteAsync(list);

            // await deletedItemsAction.Should().ThrowExactlyAsync<StorageException>();
            list.Count.Should().Be(itemsCount);
        }
    }
}