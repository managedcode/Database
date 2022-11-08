using FluentAssertions;
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

        [Fact]
        public virtual async Task InsertOneItem()
        {
            var id = GenerateId();
            var item = CreateNewItem(id);

            var insertItem = await Collection.InsertAsync(item);

            insertItem.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task InsertItemDublicate()
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
    }
}
