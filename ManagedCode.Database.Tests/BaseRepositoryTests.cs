using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests;

public abstract class BaseRepositoryTests<TId, TItem> : IDisposable where TItem : IBaseItem<TId>, new()
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
        itemsInsert.Should().Be(100);

        foreach (var item in list)
        {
            item.DateTimeData = DateTime.Now.AddDays(-1);
        }

        var itemsUpdate = await Collection.InsertOrUpdateAsync(list); //TO DO LiteDB must be 100, but result 0
        itemsUpdate.Should().Be(100);

        list.Count.Should().Be(100);
    }

    #endregion

    #region Update

    [Fact]
    public virtual async Task UpdateOneItem()
    {
        var id = GenerateId();

        var insertOneItem = await Collection.InsertAsync(CreateNewItem(id));
        var updateFirstItem = await Collection.UpdateAsync(CreateNewItem(id));
        var updateSecondItem = await Collection.UpdateAsync(CreateNewItem());

        insertOneItem.Should().NotBeNull();
        updateFirstItem.Should().NotBeNull();
        updateSecondItem.Should().BeNull();
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

    [Fact]
    public virtual async Task Update5Items()
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
    public virtual async Task DeleteOneItem()
    {
        var item = CreateNewItem();
        await Collection.InsertAsync(item);
        var deleted = await Collection.DeleteAsync(item);
        item.Should().NotBeNull();
        deleted.Should().BeTrue();
    }

    [Fact]
    public virtual async Task DeleteListOfItems()
    {
        List<TItem> list = new();

        for (var i = 0; i < 100; i++)
        {
            list.Add(CreateNewItem());
        }

        var items = await Collection.InsertAsync(list);
        var deletedItems = await Collection.DeleteAsync(list);

        deletedItems.Should().Be(100);
        items.Should().Be(100);
    }

    [Fact]
    public virtual async Task DeleteListOfItemsById()
    {
        List<TItem> list = new();

        for (var i = 0; i < 100; i++)
        {
            list.Add(CreateNewItem());
        }

        var items = await Collection.InsertAsync(list);
        var ids = list.Select(s => s.Id);
        var deletedItems = await Collection.DeleteAsync(ids);

        deletedItems.Should().Be(100);
        items.Should().Be(100);
    }

    [Fact]
    public virtual async Task DeleteByQuery()
    {
        List<TItem> list = new();

        for (var i = 0; i < 100; i++)
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
        List<TItem> list = new();

        for (var i = 0; i < 100; i++)
        {
            list.Add(CreateNewItem());
        }

        var items = await Collection.InsertAsync(list);
        var deletedItems = await Collection.DeleteAllAsync();
        var count = await Collection.CountAsync();

        deletedItems.Should().BeTrue();
        items.Should().Be(100);
        count.Should().Be(0);
    }

    #endregion

    #region Count

    [Fact]
    public virtual async Task Count()
    {
        await Collection.DeleteAllAsync();

        var insertOneItem = await Collection.InsertAsync(CreateNewItem());

        var count = await Collection.CountAsync();
        insertOneItem.Should().NotBeNull();
        count.Should().Be(1);
    }

    [Fact]
    public virtual async Task CountByQuery()
    {
        var count = 0;
        var guid = Guid.NewGuid().ToString();
        for (var i = 0; i < 100; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var deletedCount = await Collection.Query.Where(w => w.StringData == guid).CountAsync();
        deletedCount.Should().Be(count);
    }

    #endregion

    #region Get

    [Fact]
    public virtual async Task GetByWrongId()
    {
        var id1 = GenerateId();
        var id2 = GenerateId();

        var insertOneItem = await Collection.InsertAsync(CreateNewItem(id1));

        var item = await Collection.GetAsync(id2);
        insertOneItem.Should().NotBeNull();
        item.Should().BeNull();
    }

    [Fact]
    public virtual async Task GetById()
    {
        var id = GenerateId();
        var insertOneItem = await Collection.InsertAsync(CreateNewItem(id));

        var item = await Collection.GetAsync(id);
        insertOneItem.Should().NotBeNull();
        item.Should().NotBeNull();
    }

    [Fact]
    public virtual async Task GetByIdFirst()
    {
        var item1 = CreateNewItem();
        var item2 = CreateNewItem();

        await Collection.InsertAsync(item1);
        await Collection.InsertAsync(item2);

        var item = await Collection.Query.Where(w => w.StringData == item1.StringData || w.StringData == item2.StringData)
            .ToAsyncEnumerable().FirstOrDefaultAsync();
        item.Should().NotBeNull();
    }

    [Fact]
    public virtual async Task GetByQuery()
    {
        var item1 = CreateNewItem();
        var item2 = CreateNewItem();

        await Collection.InsertAsync(item1);
        await Collection.InsertAsync(item2);

        var item = await Collection.Query.Where(w => w.StringData == item1.StringData)
            .ToAsyncEnumerable().FirstOrDefaultAsync();
        item.Should().NotBeNull();
    }

    [Fact]
    public virtual async Task GetByWrongQuery()
    {
        await Collection.InsertAsync(CreateNewItem());
        await Collection.InsertAsync(CreateNewItem());

        var item = await Collection.Query.Where(w => w.StringData == "non existing value")
            .ToAsyncEnumerable().FirstOrDefaultAsync();
        item.Should().BeNull();
    }

    #endregion

    #region Find

    [Fact]
    public virtual async Task FindByCondition()
    {
        await Collection.DeleteAllAsync();

        var item1 = CreateNewItem();
        await Collection.InsertAsync(item1);
        for (var i = 0; i < 10; i++)
        {
            await Collection.InsertAsync(CreateNewItem());
        }

        var items = await Collection.Query
            .Where(x => x.StringData == item1.StringData).Where(x => x.IntData == item1.IntData).ToAsyncEnumerable()
            .ToListAsync();

        items.Count.Should().Be(1);
    }

    [Fact]
    public virtual async Task Find()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 100; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var items = await Collection.Query.Where(w => w.IntData >= 50).ToListAsync();
        items.Count.Should().Be(50);
    }

    [Fact]
    public virtual async Task FindTakeSkip()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 100; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }


        var items = await Collection.Query.Where(w => w.IntData > 10).Skip(15).Take(10).ToListAsync();
        items.Count.Should().Be(10);
        items.First().IntData.Should().Be(26);
        items.Last().IntData.Should().Be(35);
    }

    [Fact]
    public virtual async Task FindTake()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 100; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }


        var items = await Collection.Query.Where(w => w.IntData >= 50).OrderBy(o => o.IntData).Take(10).ToListAsync();

        items.Count.Should().Be(10);
        items.First().IntData.Should().Be(50);


    }

    [Fact]
    public virtual async Task FindSkip()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 100; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var items = await Collection.Query.Where(w => w.IntData >= 50).OrderBy(o => o.IntData).Skip(10).ToListAsync();
        items.Count.Should().Be(40);
        items.First().IntData.Should().Be(60);
    }

    [Fact]
    public virtual async Task FindOrder()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 100; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var items = await Collection.Query.Where(w => w.IntData >= 50).OrderBy(o => o.IntData).Skip(10).Take(2)
            .ToListAsync();

        var itemsByDescending = await Collection.Query.Where(w => w.IntData >= 50).OrderByDescending(o => o.IntData).Take(10)
            .ToListAsync();

        items.Count.Should().Be(2);
        items[0].IntData.Should().Be(60);
        items[1].IntData.Should().Be(61);

        itemsByDescending.Count.Should().Be(10);
        itemsByDescending[0].IntData.Should().Be(99);
        itemsByDescending[1].IntData.Should().Be(98);
    }

    [Fact]
    public virtual async Task FindOrderThen()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 100; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }


        var items = await Collection.Query.Where(w => w.IntData >= 50).OrderBy(
                o => o.IntData).OrderBy(t => t.DateTimeData).Skip(10).Take(2)
            .ToListAsync();

        var itemsBy = await Collection.Query.Where(w => w.IntData >= 50)
            .OrderByDescending(o => o.IntData).Take(10)
            .ToListAsync();

        var itemsThenByDescending = await Collection.Query.Where(w => w.IntData >= 50)
            .OrderByDescending(o => o.IntData, t => t.DateTimeData).Take(10)
            .ToListAsync();

        items.Count.Should().Be(2);
        items[0].IntData.Should().Be(60);
        items[1].IntData.Should().Be(61);

        itemsBy.Count.Should().Be(10);
        itemsBy[0].IntData.Should().Be(99);
        itemsBy[1].IntData.Should().Be(98);

        itemsThenByDescending.Count.Should().Be(10);
        itemsThenByDescending[0].IntData.Should().Be(99);
        itemsThenByDescending[1].IntData.Should().Be(98);
    }

    #endregion
}