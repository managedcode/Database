using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.BaseTests;

public abstract class BaseQueryableTests<TId, TItem> : IAsyncLifetime
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
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert);
        itemsResult.First().StringData.Should().Be(guid);
    }

    [Fact]
    public virtual async Task WhereQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderByQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.OrderBy(o => o.IntData).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert);
        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderByDescendingQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert);
        itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
    }

    [Fact]
    public virtual async Task TakeQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Take(itemsCountToTake).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
    }

    [Fact]
    public virtual async Task TakeQueryBeyond_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTakeBeyond = itemsCountToInsert + 3;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Take(itemsCountToTakeBeyond).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert);
    }

    [Fact]
    public virtual async Task SkipQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 3;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
    }

    [Fact]
    public virtual async Task WhereOrderByQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid).OrderBy(o => o.IntData).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert);
        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid).OrderBy(o => o.IntData).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid).OrderByDescending(o => o.IntData).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert);
        itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid).OrderByDescending(o => o.IntData).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereTakeQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid).Take(itemsCountToTake).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
        itemsResult.First().StringData.Should().Be(guid);
    }


    [Fact]
    public virtual async Task WhereTakeQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid).Take(itemsCountToTake).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    public virtual async Task WhereSkipQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
    }

    [Fact]
    public virtual async Task WhereSkipQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderByTakeQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Take(itemsCountToTake).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderBySkipQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert  - itemsCountToSkip);
        itemsResult.First().IntData.Should().Be(itemsCountToSkip);
    }

    [Fact]
    public virtual async Task OrderByDescendingTakeQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Take(itemsCountToTake).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
        itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
    }

    [Fact]
    public virtual async Task OrderByDescendingSkipQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 3;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    [Fact]
    public virtual async Task TakeSkipQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;


        for (var i = 0; i < itemsCountToInsert; i++)
        {
            await Collection.InsertAsync(CreateNewItem());
        }

        // Act
        var itemsResult = await Collection.Query.Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);

    }

    [Fact]
    public virtual async Task TakeSkipQueryBeyond_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTakeBeyond = itemsCountToInsert + 3;
        int itemsCountToSkip = 2;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            await Collection.InsertAsync(CreateNewItem());
        }

        // Act
        var itemsResult = await Collection.Query.Take(itemsCountToTakeBeyond).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).Take(itemsCountToTake).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid)
            .OrderBy(o => o.IntData).Take(itemsCountToTake).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData).Take(itemsCountToTake).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
        itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid)
            .OrderByDescending(o => o.IntData).Take(itemsCountToTake).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderBySkipQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
        itemsResult.First().IntData.Should().Be(itemsCountToSkip);
    }

    [Fact]
    public virtual async Task WhereOrderBySkipQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid)
            .OrderBy(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingSkipQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingSkipQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid)
            .OrderByDescending(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeSkipQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        itemsResult.First().IntData.Should().Be(itemsCountToSkip);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeSkipQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid)
            .OrderBy(o => o.IntData).Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeSkipQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData).Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeSkipQuery_ReturnNull()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = i;
            item.StringData = guid;
            await Collection.InsertAsync(item);
        }

        // Act
        var itemsResult = await Collection.Query.Where(w => w.StringData == unfaithfulGuid)
            .OrderByDescending(o => o.IntData).Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }
}