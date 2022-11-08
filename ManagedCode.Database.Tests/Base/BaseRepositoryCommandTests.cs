using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ManagedCode.Database.Tests.Base
{
    public abstract class BaseCommandTests<TId, TItem> : IDisposable
        where TItem : IBaseItem<TId>, new()
    {
        protected abstract IDBCollection<TId, TItem> Collection { get; }

        protected abstract TId GenerateId();

        public abstract void Dispose();

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
                DateTimeData = DateTime.Now,
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
        public virtual async Task InsertOneItem()
        {
            var id = GenerateId();
            var item = CreateNewItem(id);

            var insertItem = await Collection.InsertAsync(item);

            insertItem.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task InsertItemDuplicate()
        {
            var id = GenerateId();
            var firstItem = CreateNewItem(id);
            var secondItem = CreateNewItem(id);

            var insertFirstItem = await Collection.InsertAsync(firstItem);
            var insertSecondItem = await Collection.InsertAsync(secondItem);

            insertFirstItem.Should().NotBeNull();
            insertSecondItem.Should().BeNull();
        }

        [Fact]
        public virtual async Task InsertListOfItems()
        {
            List<TItem> list = new();

            for (var i = 0; i < 4; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);

            items.Should().Be(list.Count);
        }

        [Fact]
        public virtual async Task Insert5Items()
        {
            var id = GenerateId();

            await Collection.InsertAsync(CreateNewItem(id));

            List<TItem> list = new();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < 4; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);

            list.Count.Should().Be(5);
            items.Should().Be(4);
        }

        [Fact]
        public virtual async Task InsertOrUpdateOneItem()
        {
            var id = GenerateId();
            for (var i = 0; i < 5; i++)
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
        public virtual async Task UpdateItem_WhenItem_DoesntExists()
        {
            var id = GenerateId();
            
            var updateItem = await Collection.UpdateAsync(CreateNewItem(id));

            updateItem.Should().BeNull();
        }

        [Fact]
        public virtual async Task UpdateListOfItems()
        {
            int itemsCount = 100;
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
        public virtual async Task UpdateOneItemFromList()
        {
            List<TItem> list = new();

            var id = GenerateId();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < 9; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);
            list.Clear();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < 9; i++)
            {
                list.Add(CreateNewItem());
            }

            var updatedItems = await Collection.UpdateAsync(list);

            list.Count.Should().Be(10);
            items.Should().Be(10);
            updatedItems.Should().Be(1);
        }

        #endregion

        #region Delete

        [Fact]
        public virtual async Task DeleteOneItemById()
        {
            var item = CreateNewItem();
            
            await Collection.InsertAsync(item);
            var deleted = await Collection.DeleteAsync(item.Id);
            
            item.Should().NotBeNull();
            deleted.Should().BeTrue();
        }

        [Fact]
        public virtual async Task DeleteOneItemById_WhenItemDoesntExists()
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
        public virtual async Task DeleteByQuery()
        {
            int itemsCount = 6;
            List<TItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            await Collection.InsertOrUpdateAsync(list);

            var q1 = list[0].StringData;
            var q2 = list[1].StringData;
            var q3 = list[2].StringData;

            var equals = await Collection.Query.Where(w => w.StringData == q1).DeleteAsync();
            var or = await Collection.Query.Where(w => w.StringData == q2 || w.StringData == q3).DeleteAsync();

            equals.Should().Be(1);
            or.Should().Be(2);
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
            var deletedItems = await Collection.DeleteAllAsync();
            var count = await Collection.CountAsync();

            deletedItems.Should().BeTrue();
            items.Should().Be(itemsCount);
            count.Should().Be(0);
        }

        #endregion
    }
}
