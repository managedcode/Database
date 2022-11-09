using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.BaseTests;

public abstract class BaseRepositoryQueryableTests<TId, TItem> : IDisposable where TItem : IBaseItem<TId>, new()
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
    public virtual async Task WhereQuery_ReturnOk()
    {
        var count = 0;
        var guid = Guid.NewGuid().ToString();
        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid).CountAsync();

        ñountResult.Should().Be(count);
    }

    [Fact]
    public virtual async Task WhereQuery_ReturnNull()
    {
        var count = 0;
        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();
        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid).CountAsync();

        ñountResult.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderByQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
            count++;
        }

        var ñountResult = await Collection.Query.OrderBy(o => o.IntData).ToListAsync();

        ñountResult.Count.Should().Be(count);
        ñountResult.First().IntData.Should().Be(10);
    }

    [Fact]
    public virtual async Task OrderByDescendingQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
            count++;
        }

        var ñountResult = await Collection.Query.OrderByDescending(o => o.IntData).ToListAsync();

        ñountResult.Count.Should().Be(count);
        ñountResult.First().IntData.Should().Be(10);
    }

    [Fact]
    public virtual async Task TakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        var items = await Collection.Query.Take(10).ToListAsync();

        items.Count.Should().Be(10);
        items.First().IntData.Should().Be(0);
    }  
    
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

        var items = await Collection.Query.Skip(10).ToListAsync();

        items.Count.Should().Be(10);
        items.First().IntData.Should().Be(10);
    }

    [Fact]
    public virtual async Task WhereOrderByQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid).OrderBy(o => o.IntData).ToListAsync();

        ñountResult.Count.Should().Be(count);
        ñountResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid).OrderBy(o => o.IntData).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid).OrderByDescending(o => o.IntData).ToListAsync();

        ñountResult.Count.Should().Be(count);
        ñountResult.First().IntData.Should().Be(8);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid).OrderByDescending(o => o.IntData).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid).Take(2).ToListAsync();

        ñountResult.Count.Should().Be(2);
        ñountResult.First().IntData.Should().Be(0);
    }


    [Fact]
    public virtual async Task WhereTakeQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid).Take(2).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }

    public virtual async Task WhereSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(count-2);
        ñountResult.First().IntData.Should().Be(4);
    }

    [Fact]
    public virtual async Task WhereSkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderByTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.OrderBy(o => o.IntData).Take(2).ToListAsync();

        ñountResult.Count.Should().Be(2);
        ñountResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderBySkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.OrderBy(o => o.IntData).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(count-2);
        ñountResult.First().IntData.Should().Be(4);
    }

    [Fact]
    public virtual async Task OrderByDescendingTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.OrderByDescending(o => o.IntData).Take(2).ToListAsync();

        ñountResult.Count.Should().Be(2);
        ñountResult.First().IntData.Should().Be(8);
    }

    [Fact]
    public virtual async Task OrderByDescendingSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.OrderByDescending(o => o.IntData).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(count - 2);
        ñountResult.First().IntData.Should().Be(4);
    }

    [Fact]
    public virtual async Task TakeSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            
            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Take(4).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(2);
        ñountResult.First().IntData.Should().Be(3);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).Take(2).ToListAsync();

        ñountResult.Count.Should().Be(2);
        ñountResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderBy(o => o.IntData).Take(2).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData).Take(2).ToListAsync();

        ñountResult.Count.Should().Be(2);
        ñountResult.First().IntData.Should().Be(8);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderByDescending(o => o.IntData).Take(2).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderBySkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(count - 2);
        ñountResult.First().IntData.Should().Be(4);
    }

    [Fact]
    public virtual async Task WhereOrderBySkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderBy(o => o.IntData).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(count - 2);
        ñountResult.First().IntData.Should().Be(4);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingSkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderByDescending(o => o.IntData).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).Take(4).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(2);
        ñountResult.First().IntData.Should().Be(4);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeSkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderBy(o => o.IntData).Take(4).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeSkipQuery_ReturnOk()
    {
        await Collection.DeleteAllAsync();

        var count = 0;
        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
                count++;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData).Take(4).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(2);
        ñountResult.First().IntData.Should().Be(4);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeSkipQuery_ReturnNull()
    {
        await Collection.DeleteAllAsync();

        var guid = Guid.NewGuid().ToString();
        var falseGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < 10; i++)
        {
            var item = CreateNewItem();
            if (i % 2 == 0)
            {
                item.StringData = guid;
                item.IntData = i;
            }

            await Collection.InsertAsync(item);
        }

        var ñountResult = await Collection.Query.Where(w => w.StringData == falseGuid)
            .OrderByDescending(o => o.IntData).Take(4).Skip(2).ToListAsync();

        ñountResult.Count.Should().Be(0);
    }
}