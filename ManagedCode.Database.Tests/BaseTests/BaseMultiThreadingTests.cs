using ManagedCode.Database.Tests.Common;
using System;
using System.Threading.Tasks;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;
using System.Collections.Generic;
using FluentAssertions;

namespace ManagedCode.Database.Tests.BaseTests;

public abstract class BaseMultiThreadingTests<TId, TItem> : BaseTests<TId, TItem>
    where TItem : IBaseItem<TId>, new()
{
    protected BaseMultiThreadingTests(ITestContainer<TId, TItem> testContainer) : base(testContainer)
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

    #region Insert

    [Fact]
    public virtual async Task InsertFromMultiplieThreads_ReturnsOk()
    {
        // Arrange
        int taskCount = 100;
        List<Task> tasks = new List<Task>();

        for(int i = 0; i < taskCount; i++)
            tasks.Add(Task.Run(async () => await Collection.InsertAsync(CreateNewItem())));

        // Act
        var act = async () => await Task.WhenAll(tasks);

        //Assert
        await act.Should().NotThrowAsync();
        long count = await Collection.CountAsync();
        count.Should().Be(taskCount);
    }

    #endregion
}