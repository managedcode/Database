using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.BaseTests;

public abstract class BaseQueryableTests<TId, TItem> : BaseTests<TId, TItem> where TItem : IBaseItem<TId>, new()
{
    protected BaseQueryableTests(ITestContainer<TId, TItem> testContainer) : base(testContainer)
    {
    }

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

    protected async Task<List<TItem>> CreateAndInsertItemsAsync(int itemsCountToInsert, string guid = null,
        int? intData = null)
    {
        List<TItem> createItems = new();

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            var item = CreateNewItem();
            item.IntData = intData ?? i;
            item.StringData = guid ?? item.StringData;
            createItems.Add(item);
        }

        await Collection.InsertAsync(createItems);

        return createItems;
    }

    #region WhereQuery

    #region Where

    [Fact]
    public virtual async Task Where_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert);
        itemsResult.First().StringData.Should().Be(guid);
    }

    #endregion

    #region WhereOrderBy

    [Fact]
    public virtual async Task WhereOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert);
        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task WhereOrderByTakeSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToSkip);
    }

    [Fact]
    public virtual async Task WhereOrderBySkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToSkip);
    }

    [Fact]
    public virtual async Task WhereOrderBySkipTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToSkip);
    }

    #endregion

    #region WhereOrderByDescending

    [Fact]
    public virtual async Task WhereOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingTakeSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    [Fact]
    public virtual async Task WhereOrderByDescendingSkipTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    #endregion

    #region WhereTake

    [Fact]
    public virtual async Task WhereTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
        itemsResult.First().StringData.Should().Be(guid);
    }

    [Fact]
    public virtual async Task WhereTakeOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 10;
        int itemsCountToTake = 5;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task WhereTakeOrderBySkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task WhereTakeOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task WhereTakeOrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task WhereTakeSkipOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task WhereTakeSkipOrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    #endregion

    #region WhereSkip

    [Fact]
    public virtual async Task WhereSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
    }

    #region WhereSkipOrderByDescending

    [Fact]
    public virtual async Task WhereSkipOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task WhereSkipOrderByDescendingTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task WhereSkipTakeOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task WhereSkipTakeOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    #endregion

    #endregion

    #endregion

    #region OrderByQuery

    #region OrderBy

    [Fact]
    public virtual async Task OrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert)
            .And
            .BeInAscendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(0);
    }

    #endregion

    #region OrderByTake

    [Fact]
    public virtual async Task OrderByTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderBy(o => o.IntData)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(0);
    }

    [Fact]
    public virtual async Task OrderByTakeSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderBy(o => o.IntData)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    #endregion

    #region OrderBySkip

    [Fact]
    public virtual async Task OrderBySkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderBy(w => w.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
        itemsResult.First().IntData.Should().Be(itemsCountToSkip);
    }

    [Fact]
    public virtual async Task OrderBySkipTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToSkip);
    }

    #endregion

    #endregion

    #region OrderByDescendingQuery

    #region OrderByDescending

    [Fact]
    public virtual async Task OrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
    }

    #endregion

    #region OrderByDescendingTake

    [Fact]
    public virtual async Task OrderByDescendingTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderByDescending(o => o.IntData)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
    }

    [Fact]
    public virtual async Task OrderByDescendingTakeSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderByDescending(o => o.IntData)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    #endregion

    #region OrderByDescendingSkip

    [Fact]
    public virtual async Task OrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 3;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    [Fact]
    public virtual async Task OrderByDescendingSkipTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    #endregion

    #endregion

    #region TakeQuery

    #region Take

    [Fact]
    public virtual async Task Take_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
    }

    #endregion

    #region TakeOrderBy

    [Fact]
    public virtual async Task TakeOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeOrderByWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeOrderByWhereSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeOrderBySkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeOrderBySkipWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    #endregion

    #region TakeOrderByDescending

    [Fact]
    public virtual async Task TakeOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeOrderByDescendingWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeOrderByDescendingWhereSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeOrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeOrderByDescendingSkipWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    #endregion

    #region TakeWhere

    [Fact]
    public virtual async Task TakeWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
        itemsResult.First().StringData.Should().Be(guid);
    }

    [Fact]
    public virtual async Task TakeWhereOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeWhereOrderBySkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeWhereOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeWhereOrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeWhereSkipOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeWhereSkipOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    #endregion

    #region TakeSkip

    [Fact]
    public virtual async Task TakeSkip_ReturnOk()
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
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
    }

    [Fact]
    public virtual async Task TakeSkipOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeSkipOrderByWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeSkipOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeSkipOrderByDescendingWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeSkipWhereOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task TakeSkipWhereOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    #endregion

    #endregion

    #region SkipQuery

    #region Skip

    [Fact]
    public virtual async Task Skip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
    }

    #endregion

    #region SkipOrderBy

    [Fact]
    public virtual async Task SkipOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipOrderByWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipOrderByTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    #endregion

    #region SkipOrderByDescending

    [Fact]
    public virtual async Task SkipOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipOrderByDescendingTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    #endregion

    #region SkipWhere

    [Fact]
    public virtual async Task SkipWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
        itemsResult.First().StringData.Should().Be(guid);
    }

    [Fact]
    public virtual async Task SkipWhereOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipWhereOrderByTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipWhereOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipWhereOrderByDescendingTake_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipWhereTakeOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipWhereTakeOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    #endregion

    #region SkipTake

    [Fact]
    public virtual async Task SkipTake_ReturnOk()
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
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
    }

    [Fact]
    public virtual async Task SkipTakeOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipTakeOrderByWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipTakeOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipTakeOrderByDescendingWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }


    [Fact]
    public virtual async Task SkipTakeWhereOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData).ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    [Fact]
    public virtual async Task SkipTakeWhereOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    #endregion

    #endregion

    #region ThenBy

    [Fact]
    public virtual async Task ThenBy_AfterOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 10;

        var list = await CreateAndInsertItemsAsync(itemsCountToInsert, intData: 1);

        var ordered = list
            .OrderBy(o => o.IntData)
            .ThenBy(o => o.LongData)
            .ToList();

        // Act
        var itemsResult = await Collection.Query
            .OrderBy(o => o.IntData)
            .ThenBy(o => o.LongData)
            .ToListAsync();

        // Assert
        itemsResult.Should()
            .HaveCount(itemsCountToInsert)
            .And
            .BeEquivalentTo(ordered, options => options.WithStrictOrdering()
                .Including(x => x.IntData)
                .Including(x => x.LongData));
    }

    [Fact]
    public virtual async Task ThenByDescending_AfterOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 10;

        var list = await CreateAndInsertItemsAsync(itemsCountToInsert, intData: 1);

        var ordered = list
            .OrderBy(o => o.IntData)
            .ThenByDescending(o => o.LongData)
            .ToList();

        // Act
        var itemsResult = await Collection.Query
            .OrderBy(o => o.IntData)
            .ThenByDescending(o => o.LongData)
            .ToListAsync();

        // Assert
        itemsResult.Should()
            .HaveCount(itemsCountToInsert)
            .And
            .BeEquivalentTo(ordered, options => options.WithStrictOrdering()
                .Including(x => x.IntData)
                .Including(x => x.LongData));
    }

    [Fact]
    public virtual async Task ThenBy_AfterOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 10;

        var list = await CreateAndInsertItemsAsync(itemsCountToInsert, intData: 1);

        var ordered = list
            .OrderByDescending(o => o.IntData)
            .ThenBy(o => o.LongData)
            .ToList();

        // Act
        var itemsResult = await Collection.Query
            .OrderByDescending(o => o.IntData)
            .ThenBy(o => o.LongData)
            .ToListAsync();

        // Assert
        itemsResult.Should()
            .HaveCount(itemsCountToInsert)
            .And
            .BeEquivalentTo(ordered, options => options.WithStrictOrdering()
                .Including(x => x.IntData)
                .Including(x => x.LongData));
    }

    [Fact]
    public virtual async Task ThenByDescending_AfterOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 10;

        var list = await CreateAndInsertItemsAsync(itemsCountToInsert, intData: 1);

        var ordered = list
            .OrderByDescending(o => o.IntData)
            .ThenByDescending(o => o.LongData)
            .ToList();

        // Act
        var itemsResult = await Collection.Query
            .OrderByDescending(o => o.IntData)
            .ThenByDescending(o => o.LongData)
            .ToListAsync();

        // Assert
        itemsResult.Should()
            .HaveCount(itemsCountToInsert)
            .And
            .BeEquivalentTo(ordered, options => options.WithStrictOrdering()
                .Including(x => x.IntData)
                .Including(x => x.LongData));
    }

    #endregion

    #region Exceptional Situations

    [Fact]
    public virtual async Task Where_ReturnZero()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == unfaithfulGuid)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(0);
    }

    [Fact]
    public virtual async Task Where_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;


        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .Where(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public virtual async Task OrderBy_AfterIOrdered_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .OrderBy(o => o.IntData)
            .OrderBy(o => o.StringData)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public virtual async Task OrderBy_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .OrderBy(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public virtual async Task OrderByDescending_AfterIOrdered_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = () => Collection.Query
            .OrderBy(o => o.IntData)
            .OrderByDescending(o => o.StringData)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public virtual async Task OrderByDescending_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = () => Collection.Query
            .OrderByDescending(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public virtual async Task ThenBy_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .OrderBy(o => o.StringData)
            .ThenBy(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public virtual async Task ThenByDescending_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .OrderBy(o => o.StringData)
            .ThenByDescending(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public virtual async Task Take_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(null)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert);
    }

    [Fact]
    public virtual async Task Take_Beyond_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = itemsCountToInsert + 3;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert);
    }

    [Fact]
    public virtual async Task TakeSkip_Beyond_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = itemsCountToInsert + 3;
        int itemsCountToSkip = 2;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            await Collection.InsertAsync(CreateNewItem());
        }

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert - itemsCountToSkip);
    }

    [Fact]
    public virtual async Task Skip_Beyond_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = itemsCountToInsert + 2;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            await Collection.InsertAsync(CreateNewItem());
        }

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(0);
    }

    [Fact]
    public virtual async Task Take_NegativeNumber_ReturnZero()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = -3;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(0);
    }

    [Fact]
    public virtual async Task Skip_NegativeNumber_ReturnZero()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = -3;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToInsert);
    }

    #endregion

    #region FirstOrDefaultAsync

    [Fact]
    public virtual async Task FirstOrDefault_Skip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .FirstOrDefaultAsync();

        // Assert
        itemsResult.StringData.Should().Be(guid);
    }

    [Fact]
    public virtual async Task FirstOrDefault_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int intData = 0;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.IntData == intData)
            .FirstOrDefaultAsync();

        // Assert
        itemsResult.IntData.Should().Be(intData);
    }

    [Fact]
    public virtual async Task FirstOrDefault_WithoutAnyQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .FirstOrDefaultAsync();

        // Assert
        itemsResult.StringData.Should().Be(guid);
    }

    [Fact]
    public virtual async Task FirstOrDefault_EmptyCollection_ReturnNull()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString();


        // Act
        var itemsResult = await Collection.Query
            .FirstOrDefaultAsync();

        // Assert
        itemsResult.Should().BeNull();
    }

    #endregion

    #region CountAsync

    [Fact]
    public virtual async Task Count_Skip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var countResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .CountAsync();

        // Assert
        countResult.Should().Be(itemsCountToInsert - itemsCountToSkip);
    }

    [Fact]
    public virtual async Task Count_Take_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var countResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .CountAsync();

        // Assert
        countResult.Should().Be(itemsCountToTake);
    }

    [Fact]
    public virtual async Task Count_WithoutAnyQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var countResult = await Collection.Query
            .CountAsync();

        // Assert
        countResult.Should().Be(itemsCountToInsert);
    }

    [Fact]
    public virtual async Task Count_EmptyCollection_ReturnZero()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString();


        // Act
        var countResult = await Collection.Query
            .CountAsync();

        // Assert
        countResult.Should().Be(0);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public virtual async Task Delete_WhereOneItem_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int intData = 0;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var deleteResult = await Collection.Query
            .Where(w => w.IntData == intData)
            .DeleteAsync();

        var countResult = await Collection.Query
            .CountAsync();

        // Assert
        deleteResult.Should().Be(1);
        countResult.Should().Be(itemsCountToInsert - deleteResult);
    }

    [Fact]
    public virtual async Task Delete_WhereAllItens_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var deleteResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .DeleteAsync();

        var countResult = await Collection.Query
            .CountAsync();

        // Assert
        deleteResult.Should().Be(itemsCountToInsert);
        countResult.Should().Be(itemsCountToInsert - deleteResult);
    }

    [Fact]
    public virtual async Task Delete_WithoutAnyQuery_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var deleteResult = await Collection.Query
            .DeleteAsync();

        var countResult = await Collection.Query
            .CountAsync();

        // Assert
        deleteResult.Should().Be(itemsCountToInsert);
    }

    public virtual async Task Delete_Skip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToSkip = 2;


        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var deleteResult = await Collection.Query
            .Skip(itemsCountToSkip)
            .DeleteAsync();

        var countResult = await Collection.Query
            .CountAsync();

        // Assert
        deleteResult.Should().Be(itemsCountToInsert - itemsCountToSkip);
        countResult.Should().Be(itemsCountToInsert - deleteResult);
    }

    public virtual async Task Delete_Take_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 2;


        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var deleteResult = await Collection.Query
            .Take(itemsCountToTake)
            .DeleteAsync();

        var countResult = await Collection.Query
            .CountAsync();

        // Assert
        deleteResult.Should().Be(itemsCountToTake);
        countResult.Should().Be(itemsCountToInsert - deleteResult);
    }

    [Fact]
    public virtual async Task Delete_EmptyCollection_ReturnZero()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString();


        // Act
        var deleteResult = await Collection.Query
            .DeleteAsync();

        // Assert
        deleteResult.Should().Be(0);
    }

    #endregion
}