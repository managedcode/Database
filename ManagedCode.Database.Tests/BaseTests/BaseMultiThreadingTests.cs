using ManagedCode.Database.Tests.Common;
using System;
using System.Threading.Tasks;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;

namespace ManagedCode.Database.Tests.BaseTests;

public abstract class BaseMultiThreadingTests<TId, TItem> : BaseTests<TId, TItem>
    where TItem : IBaseItem<TId>, new()
{
    private static object lock_object = new object();

    protected BaseMultiThreadingTests(ITestContainer<TId, TItem> testContainer) : base(testContainer)
    {
    }

    protected TItem CreateNewItem()
    {
        var item = new TItem();
        lock (lock_object)
        {
            var rnd = new Random();
            item = new TItem
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
        return item;
    }

    protected TItem CreateNewItem(TId id)
    {
        var item = CreateNewItem();
        item.Id = id;
        return item;
    }

    protected IEnumerable<TItem> CreateListOfItems(int itemsCount)
    {
        for (int i = 0; i < itemsCount; i++)
            yield return CreateNewItem();
    }

    #region Insert

    // [Fact]
    // public virtual async Task InsertFromMultiplyThreads_ReturnsOk()
    // {
    //     // Arrange
    //     int taskCount = 100;
    //     List<Task> tasks = new List<Task>();
    //
    //     for(int i = 0; i < taskCount; i++)
    //         tasks.Add(Task.Run(async () => await Collection.InsertAsync(CreateNewItem())));
    //
    //     // Act
    //     var act = async () => await Task.WhenAll(tasks);
    //
    //     //Assert
    //     await act.Should().NotThrowAsync();
    //     long count = await Collection.CountAsync();
    //     count.Should().Be(taskCount);
    // }

    #endregion

    #region Concurency

    [Fact]
    public virtual async Task MutateCollectionByManyThreads_ThrowNoException()
    {
        // Arrange
        int threadsCount = 100;
        int iterations = 300;
        int itemsToInsertCount = 100;
        
        List<Thread> threads = new List<Thread>();

        for (int i = 0; i < threadsCount; i++)
        {
            Thread thread = new Thread( async () =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    TItem itemToDelete = CreateNewItem();
                    TItem itemToFound = CreateNewItem();
                    
                    await Collection.InsertAsync(itemToFound);
                    await Collection.InsertAsync(itemToDelete);
                    await Collection.CountAsync();
                    await Collection.InsertAsync(CreateListOfItems(itemsToInsertCount));
                    for (int k = 0; k < itemsToInsertCount; k++)
                    {
                        await Collection.InsertAsync(CreateNewItem());
                    }
                    await Collection.DeleteAsync(itemToDelete.Id);
                    await Collection.Query.Where(item => item.StringData == itemToFound.StringData).ToListAsync();
                    await Collection.Query.OrderBy(item => item.IntData).ToListAsync();
                }
            });
            thread.Start();
            threads.Add(thread);
        }

        // Act
        var act = () =>
        {
            foreach (var thread in threads)
            {
                thread.Join();
            }
        };

        // Assert
        act.Should().NotThrow();
    }


    #endregion Concurency
}