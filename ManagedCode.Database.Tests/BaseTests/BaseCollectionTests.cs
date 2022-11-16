using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ManagedCode.Database.Tests.BaseTests
{
    public abstract class BaseCollectionTests<TId, TItem> : IAsyncLifetime
        where TItem : IBaseItem<TId>, new()
    {
        protected abstract IDatabaseCollection<TId, TItem> Collection { get; }

        protected abstract TId GenerateId();

        public abstract Task InitializeAsync();

        public abstract Task DisposeAsync();

        protected TItem CreateNewItem()
        {
            var rnd = new Random();
            return new TItem
            {
                Id = GenerateId(),
                StringData = Guid.NewGuid().ToString(),
                IntData = rnd.Next(),
                LongData = rnd.Next(),
                FloatData = Convert.ToSingle(rnd.NextDouble()),
                DoubleData = rnd.NextDouble(),
                DateTimeData = DateTime.UtcNow,
            };
        }

        protected TItem CreateNewItem(TId id)
        {
            var item = CreateNewItem();
            item.Id = id;
            return item;
        }

        #region Insert

        [Fact]
        public virtual async Task InsertOneItem_ReturnsInsertedItem()
        {
            // Arrange
            var item = CreateNewItem();

            // Act
            var insertItem = await Collection.InsertAsync(item);

            // Assert
            insertItem.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task InsertItem_WhenItemExist_ShouldThrowDatabaseException()
        {
            // Arrange
            var item = CreateNewItem();

            // Act
            await Collection.InsertAsync(item);
            var insertAction = () => Collection.InsertAsync(item);

            // Assert
            await insertAction.Should().ThrowAsync<DatabaseException>();
        }

        [Fact]
        public virtual async Task InsertListOfItems_ReturnsItemCount()
        {
            // Arrange
            int itemsToInsert = 4;
            List<TItem> list = new();

            for (var i = 0; i < itemsToInsert; i++)
            {
                list.Add(CreateNewItem());
            }

            // Act
            var insertedItems = await Collection.InsertAsync(list);

            // Assert
            insertedItems.Should().Be(list.Count);
        }

        [Fact]
        public virtual async Task InsertItems_WhenOneItemAlreadyExists()
        {
            // Arrange
            var id = GenerateId();
            int itemsCountToInsert = 4;
            int expectedItemsCount = 5;

            await Collection.InsertAsync(CreateNewItem(id));

            List<TItem> list = new();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < itemsCountToInsert; i++)
            {
                list.Add(CreateNewItem());
            }

            // Act
            var insertedItems = await Collection.InsertAsync(list);

            // Assert
            list.Count.Should().Be(expectedItemsCount);
            insertedItems.Should().Be(itemsCountToInsert);
        }

        [Fact]
        public virtual async Task InsertOrUpdateOneItem()
        {
            // Arrange
            int updateItemCount = 5;

            // Act & Assert
            for (var i = 0; i < updateItemCount; i++)
            {
                var id = GenerateId();
                var insertOneItem = await Collection.InsertOrUpdateAsync(CreateNewItem(id));
                insertOneItem.Should().NotBeNull();
            }
        }

        [Fact]
        public virtual async Task InsertOrUpdateListOfItems()
        {
            // Arrange
            int itemsCount = 5;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var itemsInsert = await Collection.InsertOrUpdateAsync(list);

            foreach (var item in list)
            {
                item.DateTimeData = DateTime.Now.AddDays(-1);
            }

            // Act
            var itemsUpdate = await Collection.InsertOrUpdateAsync(list);
            //TODO: LiteDB must be 100, but result 0

            // Assert
            itemsUpdate.Should().Be(itemsCount);
            itemsInsert.Should().Be(itemsCount);
            list.Count.Should().Be(itemsCount);
        }

        #endregion

        #region Update

        [Fact]
        public virtual async Task UpdateOneItem()
        {
            // Arrange
            var id = GenerateId();

            await Collection.InsertAsync(CreateNewItem(id));

            // Act
            var updateItem = await Collection.UpdateAsync(CreateNewItem(id));

            // Assert
            updateItem.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task UpdateItem_WhenItemDoesntExists()
        {
            // Arrange
            var id = GenerateId();

            // Act
            var updateItemAction = () => Collection.UpdateAsync(CreateNewItem(id));

            // Assert
            await updateItemAction.Should().ThrowAsync<DatabaseException>();
        }

        [Fact]
        public virtual async Task UpdateListOfItems()
        {
            // Arrange
            int itemsCount = 10;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            await Collection.InsertAsync(list);
            
            // Act
            var updatedItems = await Collection.UpdateAsync(list.ToArray());

            // Assert
            updatedItems.Should().Be(itemsCount);
        }

        [Fact]
        public virtual async Task UpdateListOfItems_WhenOnlyOneItemUpdated()
        {
            // Arrange
            int expectedItemsCount = 10;
            int itemsCountToAdd = 9;
            int expectedUpdatedItemsCount = 1;
            List<TItem> list = new();

            var id = GenerateId();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < itemsCountToAdd; i++)
            {
                list.Add(CreateNewItem());
            }

            await Collection.InsertAsync(list);
            list.Clear();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < itemsCountToAdd; i++)
            {
                list.Add(CreateNewItem());
            }

            // Act
            var updatedItems = await Collection.UpdateAsync(list);

            // Assert
            list.Count.Should().Be(expectedItemsCount);
            updatedItems.Should().Be(expectedUpdatedItemsCount);
        }

        #endregion

        #region Delete

        [Fact]
        public virtual async Task DeleteItemById()
        {
            // Arrange
            var item = CreateNewItem();

            await Collection.InsertAsync(item);

            // Act
            var deleted = await Collection.DeleteAsync(item.Id);

            // Assert
            deleted.Should().BeTrue();
        }

        [Fact]
        public virtual async Task DeleteItemById_WhenItemDoesntExists()
        {
            // Arrange
            var item = CreateNewItem();

            // Act
            var deleted = await Collection.DeleteAsync(item.Id);

            // Assert
            deleted.Should().BeFalse();
        }

        [Fact]
        public virtual async Task DeleteListOfItems()
        {
            // Arrange
            int itemsCount = 5;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            await Collection.InsertAsync(list);
            
            // Act
            var deletedItems = await Collection.DeleteAsync(list);

            // Assert
            deletedItems.Should().Be(itemsCount);
        }

        [Fact]
        public virtual async Task DeleteListOfItems_WhenItemsDontExist()
        {
            // Arrange
            int itemsCount = 5;
            int expectedDeletedItemsCount = 0;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            // Act
            var deletedItems = await Collection.DeleteAsync(list);

            // Assert
            deletedItems.Should().Be(expectedDeletedItemsCount);
            list.Count.Should().Be(itemsCount);
        }

        [Fact]
        public virtual async Task DeleteListOfItemsById()
        {
            // Arrange
            int itemsCount = 5;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var ids = list.Select(item => item.Id);

            await Collection.InsertAsync(list);
            
            // Act
            var deletedItems = await Collection.DeleteAsync(ids);

            // Assert
            deletedItems.Should().Be(itemsCount);
        }

        [Fact]
        public virtual async Task DeleteListOfItemsById_WhenItemsDontExist()
        {
            // Arrange
            int itemsCount = 5;
            int expectedDeletedItemsCount = 0;

            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var ids = list.Select(item => item.Id);

            // Act
            var deletedItems = await Collection.DeleteAsync(ids);

            // Assert
            deletedItems.Should().Be(expectedDeletedItemsCount);
        }

        [Fact]
        public virtual async Task DeleteByQuery()
        {
            // Arrange
            int itemsCount = 6;
            int equalsQueryItemsCount = 1;
            int orQueryItemsCount = 2;

            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            await Collection.InsertOrUpdateAsync(list);

            var queryParam1 = list[0].StringData;
            var queryParam2 = list[1].StringData;
            var queryParam3 = list[2].StringData;

            // Act
            var equalsQueryResult = await Collection.Query.Where(w => w.StringData == queryParam1).DeleteAsync();
            var orQueryResult = await Collection.Query
                .Where(w => w.StringData == queryParam2 || w.StringData == queryParam3).DeleteAsync();

            // Assert
            equalsQueryResult.Should().Be(equalsQueryItemsCount);
            orQueryResult.Should().Be(orQueryItemsCount);
        }

        [Fact]
        public virtual async Task DeleteByQuery_WhenItemsDontExist()
        {
            // Arrange
            int itemsCount = 5;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            await Collection.InsertOrUpdateAsync(list);

            var query1 = "test0";
            var query2 = "test1";
            var query3 = "test2";

            // Act
            var equalsQueryResult = await Collection.Query.Where(w => w.StringData == query1).DeleteAsync();
            var orQueryResult = await Collection.Query.Where(w => w.StringData == query2 || w.StringData == query3).DeleteAsync();

            // Assert
            equalsQueryResult.Should().Be(0);
            orQueryResult.Should().Be(0);
        }

        [Fact]
        public virtual async Task DeleteAll()
        {
            // Arrange
            int itemsCount = 5;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            await Collection.InsertAsync(list);
            
            // Act
            var deletedItems = await Collection.DeleteCollectionAsync();
            var count = await Collection.CountAsync();

            // Assert
            deletedItems.Should().BeTrue();
            count.Should().Be(0);
        }

        [Fact]
        public virtual async Task DeleteAll_WhenNoItems()
        {
            // Arrange & Act
            var deletedItems = await Collection.DeleteCollectionAsync();
            
            var count = await Collection.CountAsync();

            // Assert
            deletedItems.Should().BeTrue();
            count.Should().Be(0);
        }

        #endregion

        #region Count

        [Fact]
        public virtual async Task Count()
        {
            // Arrange
            int itemsToInsert = 4;
            List<TItem> list = new();

            for (var i = 0; i < itemsToInsert; i++)
            {
                list.Add(CreateNewItem());
            }

            // Act
            var insertedItems = await Collection.InsertAsync(list);

            // Assert
            insertedItems.Should().Be(itemsToInsert);
        }

        #endregion

        #region Get

        [Fact]
        public virtual async Task GetById_ReturnOk()
        {
            // Arrange
            var itemId = GenerateId();
            await Collection.InsertAsync(CreateNewItem(itemId));

            // Act
            var getItemResult = await Collection.GetAsync(itemId);

            // Assert
            getItemResult.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task GetById_WrongId_ReturnNull()
        {
            // Arrange
            var itemId = GenerateId();

            // Act
            var getItemResult = await Collection.GetAsync(itemId);

            // Assert
            getItemResult.Should().BeNull();
        }

        #endregion
    }
}