using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.BaseTests;

public abstract class BaseRepositoryQueryableTests<TId, TItem> : IDisposable, IAsyncLifetime
    where TItem : IBaseItem<TId>, new()
{
    protected abstract IDBCollection<TId, TItem> Collection { get; }

    protected abstract TId GenerateId();

    public abstract Task InitializeAsync();

    public abstract Task DisposeAsync();

    public abstract void Dispose();

    public int CountItem = 10;

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
    public virtual async Task WhereQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var countResult = await Collection.Query.Where(w => w.StringData == guid).CountAsync();

        countResult.Should().Be(CountItem);
    }

    [Fact]
    public virtual async Task WhereQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var countResult = await Collection.Query.Where(w => w.StringData == falseGuid).CountAsync();

        countResult.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderByQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.OrderBy(o => o.IntData).ToListAsync();

        itemsResult.Count.Should().Be(CountItem);
    }

    [Fact]
    public virtual async Task OrderByDescendingQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).ToListAsync();

        itemsResult.Count.Should().Be(CountItem);
    }

/*    [Fact]
    public virtual async Task TakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Take(5).ToListAsync();

        itemsResult.Count.Should().Be(5);
        itemsResult.First().IntData.Should().Be(0);
    }*/
/*
    [Fact]
    public virtual async Task TakeQueryBeyond_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Take(CountItem + 5).ToListAsync();

        itemsResult.Count.Should().Be(CountItem);
        itemsResult.First().IntData.Should().Be(0);
    }*/

    [Fact]
    public virtual async Task SkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Skip(7).ToListAsync();

        itemsResult.Count.Should().Be(3);
    }

    [Fact]
    public virtual async Task WhereOrderByQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid).OrderBy(o => o.IntData).ToListAsync();

        itemsResult.Count.Should().Be(CountItem);
        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid).OrderBy(o => o.IntData).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid).OrderByDescending(o => o.IntData).ToListAsync();

        itemsResult.Count.Should().Be(CountItem);
        itemsResult.First().IntData.Should().Be(CountItem - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid).OrderByDescending(o => o.IntData).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid).Take(2).ToListAsync();

        itemsResult.Count.Should().Be(2);
        itemsResult.First().IntData.Should().Be(0);
    }


    [Fact]
    public virtual async Task WhereTakeQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid).Take(2).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }

    public virtual async Task WhereSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(CountItem - 2);
        itemsResult.First().IntData.Should().Be(4);
    }

    [Fact]
    public virtual async Task WhereSkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderByTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Take(2).ToListAsync();

        itemsResult.Count.Should().Be(2);
        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderBySkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(CountItem  - 2);
    }

    [Fact]
    public virtual async Task OrderByDescendingTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Take(2).ToListAsync();

        itemsResult.Count.Should().Be(2);
    }

    [Fact]
    public virtual async Task OrderByDescendingSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(CountItem - 2);
    }

    [Fact]
    public virtual async Task TakeSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            await Collection.InsertAsync(CreateNewItem());
        }

        var itemsResult = await Collection.Query.Take(3).Skip(1).ToListAsync();

        itemsResult.Count.Should().Be(2);
    }

/*    [Fact]
    public virtual async Task TakeSkipQueryBeyond_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < CountItem; i++)
        {
            await Collection.InsertAsync(CreateNewItem());
        }

        var itemsResult = await Collection.Query.Take(CountItem + 5).Skip(5).ToListAsync();

        itemsResult.Count.Should().Be(CountItem - 5);
    }*/

    [Fact]
    public virtual async Task WhereOrderByTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();
        
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).Take(2).ToListAsync();

        itemsResult.Count.Should().Be(2);
        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderBy(o => o.IntData).Take(2).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData).Take(2).ToListAsync();

        itemsResult.Count.Should().Be(2);
        itemsResult.First().IntData.Should().Be(CountItem - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderByDescending(o => o.IntData).Take(2).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderBySkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(CountItem - 2);
        itemsResult.First().IntData.Should().Be(2);
    }

    [Fact]
    public virtual async Task WhereOrderBySkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderBy(o => o.IntData).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(CountItem - 2);
        itemsResult.First().IntData.Should().Be(CountItem - 3);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingSkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderByDescending(o => o.IntData).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).Take(4).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(2);
        itemsResult.First().IntData.Should().Be(2);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeSkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderBy(o => o.IntData).Take(4).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData).Take(4).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(2);
        itemsResult.First().IntData.Should().Be(CountItem - 3);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeSkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < CountItem; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        var itemsResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderByDescending(o => o.IntData).Take(4).Skip(2).ToListAsync();

        itemsResult.Count.Should().Be(0);
    }
}