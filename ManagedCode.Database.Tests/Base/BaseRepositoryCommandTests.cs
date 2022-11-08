﻿using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
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

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);

            items.Should().Be(list.Count);
        }

        [Fact]
        public virtual async Task Insert99Items()
        {
            var id = GenerateId();

            await Collection.InsertAsync(CreateNewItem(id));

            List<TItem> list = new();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < 9; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);

            list.Count.Should().Be(10);
            items.Should().Be(9);
        }

        [Fact]
        public virtual async Task InsertOrUpdateOneItem()
        {
            var id = GenerateId();
            for (var i = 0; i < 100; i++)
            {
                var insertOneItem = await Collection.InsertOrUpdateAsync(CreateNewItem(id));
                insertOneItem.Should().NotBeNull();
            }
        }
        
        [Fact]
        public virtual async Task InsertOrUpdateListOfItems()
        {
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
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
            
            itemsUpdate.Should().Be(100);
            itemsInsert.Should().Be(100);
            list.Count.Should().Be(100);
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
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Collection.InsertAsync(list);
            var updatedItems = await Collection.UpdateAsync(list.ToArray());

            items.Should().Be(100);
            updatedItems.Should().Be(100);
        }

        #endregion
    }
}
