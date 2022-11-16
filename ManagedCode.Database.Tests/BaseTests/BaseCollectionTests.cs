﻿using FluentAssertions;
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
        protected abstract IDBCollection<TId, TItem> Collection { get; }

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
            var item = CreateNewItem();
            var insertItem = await Collection.InsertAsync(item);
            insertItem.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task InsertItem_WhenItemExist_DatabaseException()
        {
            // Arrange
            var id = GenerateId();
            var firstItem = CreateNewItem(id);
            var secondItem = CreateNewItem(id);

            // Act
            var insertFirstItemResult = await Collection.InsertAsync(firstItem);
            var insertSecondItemResult = () => Collection.InsertAsync(secondItem);

            // Assert
            insertFirstItemResult.Should().NotBeNull();
            await insertSecondItemResult.Should().ThrowAsync<DatabaseException>();
        }

        [Fact]
        public virtual async Task InsertListOfItems_ReturnsItemCount()
        {
            int itemsToInsert = 4;
            List<TItem> list = new();

            for (var i = 0; i < itemsToInsert; i++)
            {
                list.Add(CreateNewItem());
            }

            var insertedItems = await Collection.InsertAsync(list);

            insertedItems.Should().Be(list.Count);
        }

        [Fact]
        public virtual async Task InsertItems_WhenOneItemAlreadyExists()
        {
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

            var insertedItems = await Collection.InsertAsync(list);

            list.Count.Should().Be(expectedItemsCount);
            insertedItems.Should().Be(itemsCountToInsert);
        }

        [Fact]
        public virtual async Task InsertOrUpdateOneItem()
        {
            int updateItemCount = 5;

            var id = GenerateId();
            for (var i = 0; i < updateItemCount; i++)
            {
                var insertOneItem = await Collection.InsertOrUpdateAsync(CreateNewItem(id));
                insertOneItem.Should().NotBeNull();
            }
        }
        
        [Fact]
        public virtual async Task InsertOrUpdateListOfItems()
        {
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

            var itemsUpdate = await Collection.InsertOrUpdateAsync(list);
            //TODO: LiteDB must be 100, but result 0
            
            itemsUpdate.Should().Be(itemsCount);
            itemsInsert.Should().Be(itemsCount);
            list.Count.Should().Be(itemsCount);
        }
        
        #endregion

        #region Update

        [Fact]
        public virtual async Task UpdateOneItem()
        {
            var id = GenerateId();
            
            var insertItem = await Collection.InsertAsync(CreateNewItem(id));
            var updateItem = await Collection.UpdateAsync(CreateNewItem(id));

            insertItem.Should().NotBeNull();
            updateItem.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task UpdateItem_WhenItemDoesntExists()
        {
            // Arrange
            var id = GenerateId();

            // Act
            var updateItemResult = () => Collection.UpdateAsync(CreateNewItem(id));

            // Assert
            await updateItemResult.Should().ThrowAsync<DatabaseException>();

        }

        [Fact]
        public virtual async Task UpdateListOfItems()
        {
            int itemsCount = 10;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);
            var updatedItems = await Collection.UpdateAsync(list.ToArray());

            items.Should().Be(itemsCount);
            updatedItems.Should().Be(itemsCount);
        }

        [Fact]
        public virtual async Task UpdateListOfItems_WhenOnlyOneItemUpdated()
        {
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

            var items = await Collection.InsertAsync(list);
            list.Clear();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < itemsCountToAdd; i++)
            {
                list.Add(CreateNewItem());
            }

            var updatedItems = await Collection.UpdateAsync(list);

            list.Count.Should().Be(expectedItemsCount);
            items.Should().Be(expectedItemsCount);
            updatedItems.Should().Be(expectedUpdatedItemsCount);
        }

        #endregion

        #region Delete

        [Fact]
        public virtual async Task DeleteItemById()
        {
            var item = CreateNewItem();
            
            await Collection.InsertAsync(item);
            var deleted = await Collection.DeleteAsync(item.Id);
            
            item.Should().NotBeNull();
            deleted.Should().BeTrue();
        }

        [Fact]
        public virtual async Task DeleteItemById_WhenItemDoesntExists()
        {
            var item = CreateNewItem();
            
            var deleted = await Collection.DeleteAsync(item.Id);
            
            item.Should().NotBeNull();
            deleted.Should().BeFalse();
        }

        [Fact]
        public virtual async Task DeleteListOfItems()
        {
            int itemsCount = 5;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);
            var deletedItems = await Collection.DeleteAsync(list);

            deletedItems.Should().Be(itemsCount);
            items.Should().Be(itemsCount);
        }

        [Fact]
        public virtual async Task DeleteListOfItems_WhenItemsDontExist()
        {
            int itemsCount = 5;
            int expectedDeletedItemsCount = 0;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var deletedItems = await Collection.DeleteAsync(list);

            deletedItems.Should().Be(expectedDeletedItemsCount);
            list.Count.Should().Be(itemsCount);
        }

        [Fact]
        public virtual async Task DeleteListOfItemsById()
        {
            int itemsCount = 5;
            List<TItem> list = new();
            
            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var ids = list.Select(item => item.Id);

            var items = await Collection.InsertAsync(list);
            var deletedItems = await Collection.DeleteAsync(ids);

            items.Should().Be(itemsCount);
            deletedItems.Should().Be(itemsCount);
        }

        [Fact]
        public virtual async Task DeleteListOfItemsById_WhenItemsDontExist()
        {
            int itemsCount = 5;
            int expectedDeletedItemsCount = 0;

            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var ids = list.Select(item => item.Id);

            var deletedItems = await Collection.DeleteAsync(ids);

            deletedItems.Should().Be(expectedDeletedItemsCount);
        }

        [Fact]
        public virtual async Task DeleteByQuery()
        {
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

            var equalsQueryResult = await Collection.Query.Where(w => w.StringData == queryParam1).DeleteAsync();
            var orQueryResult = await Collection.Query.Where(w => w.StringData == queryParam2 || w.StringData == queryParam3).DeleteAsync();

            equalsQueryResult.Should().Be(equalsQueryItemsCount);
            orQueryResult.Should().Be(orQueryItemsCount);
        }

        [Fact]
        public virtual async Task DeleteByQuery_WhenItemsDontExist()
        {
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

            var equals = await Collection.Query.Where(w => w.StringData == query1).DeleteAsync();
            var or = await Collection.Query.Where(w => w.StringData == query2 || w.StringData == query3).DeleteAsync();

            equals.Should().Be(0);
            or.Should().Be(0);
        }

        [Fact]
        public virtual async Task DeleteAll()
        {
            int itemsCount = 5;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);
            var deletedItems = await Collection.DeleteCollectionAsync();
            var count = await Collection.CountAsync();

            deletedItems.Should().BeTrue();
            items.Should().Be(itemsCount);
            count.Should().Be(0);
        }

        [Fact]
        public virtual async Task DeleteAll_WhenNoItems()
        {
            var deletedItems = await Collection.DeleteCollectionAsync();
            var count = await Collection.CountAsync();

            deletedItems.Should().BeTrue();
            count.Should().Be(0);
        }

        #endregion

        #region Count

        [Fact]
        public virtual async Task Count()
        {
            long expectedCountBeforeInsert = 0;
            long expectedCountAfterInsert = 1;
            var countBeforeInsert = await Collection.CountAsync();
            var insertOneItem = await Collection.InsertAsync(CreateNewItem());

            var count = await Collection.CountAsync();

            countBeforeInsert.Should().Be(expectedCountBeforeInsert);
            insertOneItem.Should().NotBeNull();
            count.Should().Be(expectedCountAfterInsert);
        }
        #endregion

        #region Get

        [Fact]
        public virtual async Task GetById_ReturnOk()
        {
            // Arrange
            var itemId = GenerateId();

            // Act
            await Collection.InsertAsync(CreateNewItem(itemId));
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
