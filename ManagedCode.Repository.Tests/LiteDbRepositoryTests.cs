using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.Core;
using ManagedCode.Repository.LiteDB;
using ManagedCode.Repository.Tests.Common;
using Xunit;

namespace ManagedCode.Repository.Tests
{
    public class LiteDbRepositoryTests
    {
        public const string ConnecntionString = "test.db";

        private readonly IRepository<string, LiteDbItem> _repository =
            new LiteDbRepository<string, LiteDbItem>(null, new LiteDbRepositoryOptions
            {
                ConnectionString = GetTempDbName()
            });

        public LiteDbRepositoryTests()
        {
            _repository.InitializeAsync().Wait();
        }

        private static string GetTempDbName()
        {
            return Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ConnecntionString);
        }

        [Fact]
        public async Task InitializeAsync()
        {
            await _repository.InitializeAsync();
            _repository.IsInitialized.Should().BeTrue();
            await _repository.InitializeAsync();
            _repository.IsInitialized.Should().BeTrue();
        }

        [Fact]
        public async Task NotInitializedAsync()
        {
            var localRepository = new LiteDbRepository<string, LiteDbItem>(null, new LiteDbRepositoryOptions
            {
                ConnectionString = GetTempDbName()
            });

            localRepository.IsInitialized.Should().BeFalse();

            var item = await localRepository.InsertAsync(new LiteDbItem());

            item.Should().NotBeNull();
        }

        #region Find

        [Fact]
        public async Task Find()
        {
            var list = new List<LiteDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "Find",
                    Id = "Find" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartKey == "Find" && w.IntData >= 50).ToListAsync();
            items.Count.Should().Be(50);
        }

        [Fact]
        public async Task FindTakeSkip()
        {
            var list = new List<LiteDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "FindTakeSkip",
                    Id = "FindTakeSkip" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items1 = await _repository.FindAsync(w => w.PartKey == "FindTakeSkip" && w.IntData > 0, 15).ToListAsync();
            var items2 = await _repository.FindAsync(w => w.PartKey == "FindTakeSkip" && w.IntData > 0, 15, 10).ToListAsync();
            items1.Count.Should().Be(15);
            items2.Count.Should().Be(15);
            items1[10].Data.Should().BeEquivalentTo(items2[0].Data);
        }

        [Fact]
        public async Task FindTake()
        {
            var list = new List<LiteDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "FindTake",
                    Id = "FindTake" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items1 = await _repository.FindAsync(w => w.PartKey == "FindTake" && w.IntData >= 50, 10).ToListAsync();
            var items2 = await _repository.FindAsync(w => w.PartKey == "FindTake" && w.IntData >= 50, 15).ToListAsync();
            items1.Count.Should().Be(10);
            items2.Count.Should().Be(15);
            items1[0].Data.Should().Be(items2[0].Data);
        }

        [Fact]
        public async Task FindSkip()
        {
            var list = new List<LiteDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "FindSkip",
                    Id = "FindSkip" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items1 = await _repository.FindAsync(w => w.PartKey == "FindSkip" && w.IntData >= 50, skip: 10).ToListAsync();
            var items2 = await _repository.FindAsync(w => w.PartKey == "FindSkip" && w.IntData >= 50, skip: 11).ToListAsync();
            items1.Count.Should().Be(40);
            items2.Count.Should().Be(39);
            items1[1].IntData.Should().Be(items2[0].IntData);
        }

        [Fact]
        public async Task FindOrder()
        {
            var list = new List<LiteDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "FindOrder",
                    Id = "FindOrder" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartKey == "FindOrder" && w.IntData > 9,
                    o => o.Id, 10, 1)
                .ToListAsync();

            var itemsByDescending = await _repository.FindAsync(w => w.PartKey == "FindOrder" && w.IntData > 10,
                    o => o.Id, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].IntData.Should().Be(10);
            items[1].IntData.Should().Be(11);

            itemsByDescending.Count.Should().Be(10);
            itemsByDescending[0].IntData.Should().Be(99);
            itemsByDescending[1].IntData.Should().Be(98);
        }

        [Fact(Skip = "OrderThen is not supported.")]
        public async Task FindOrderThen()
        {
            var list = new List<LiteDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "FindOrderThen",
                    Id = "FindOrderThen" + i,
                    IntData = i,
                    Data = $"item{i % 2}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartKey == "FindOrderThen" && w.IntData >= 9,
                    o => o.Data, t => t.IntData, 10, 1)
                .ToListAsync();

            var itemsBy = await _repository.FindAsync(w => w.PartKey == "FindOrderThen" && w.IntData >= 10,
                    o => o.Data, Order.ByDescending, t => t.IntData, 10)
                .ToListAsync();

            var itemsThenByDescending = await _repository.FindAsync(w => w.PartKey == "FindOrderThen" && w.IntData >= 10,
                    o => o.Data, Order.ByDescending, t => t.IntData, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].IntData.Should().Be(11);
            items[1].IntData.Should().Be(12);

            itemsBy.Count.Should().Be(10);
            itemsBy[0].IntData.Should().Be(10);
            itemsBy[1].IntData.Should().Be(11);

            itemsThenByDescending.Count.Should().Be(10);
            itemsThenByDescending[0].IntData.Should().Be(10);
            itemsThenByDescending[1].IntData.Should().Be(11);
        }

        #endregion

        #region Insert

        [Fact]
        public async Task InsertOneItem()
        {
            var insertFirstItem = await _repository.InsertAsync(new LiteDbItem
            {
                Id = "InsertOneItem",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var insertSecondItem = await _repository.InsertAsync(new LiteDbItem
            {
                Id = "InsertOneItem",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            insertFirstItem.Should().NotBeNull();
            insertSecondItem.Should().BeNull();
        }

        [Fact]
        public async Task InsertListOfItems()
        {
            List<LiteDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = $"InsertListOfItems{i % 2}",
                    Id = "InsertListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            items.Should().Be(150);
        }

        [Fact]
        public async Task Insert100Items()
        {
            await _repository.InsertAsync(new LiteDbItem
            {
                RowKey = "Insert100Items",
                Id = "Insert100Items140",
                Data = Guid.NewGuid().ToString()
            });

            List<LiteDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new LiteDbItem
                {
                    RowKey = "Insert100Items",
                    Id = "Insert100Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            items.Should().Be(0);
        }

        [Fact]
        public async Task InsertOrUpdateOneItem()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new LiteDbItem
            {
                PartKey = "InsertOrUpdateOneItem",
                Id = "InsertOrUpdateOneItem",
                Data = Guid.NewGuid().ToString()
            });

            var insertTwoItem = await _repository.InsertOrUpdateAsync(new LiteDbItem
            {
                PartKey = "InsertOrUpdateOneItem22",
                Id = "InsertOrUpdateOneItem",
                Data = Guid.NewGuid().ToString()
            });

            insertOneItem.Should().NotBeNull();
            insertTwoItem.Should().BeNull();
        }

        [Fact]
        public async Task InsertOrUpdateListOfItems()
        {
            List<LiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = $"InsertOrUpdateListOfItems{i % 2}",
                    Id = "InsertOrUpdateListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var itemsFirst = await _repository.InsertOrUpdateAsync(list);
            var itemsSecond = await _repository.InsertOrUpdateAsync(list);

            itemsFirst.Should().Be(100);
            itemsSecond.Should().Be(0);
        }

        [Fact]
        public async Task InsertOrUpdate100Items()
        {
            await _repository.InsertOrUpdateAsync(new LiteDbItem
            {
                PartKey = "InsertOrUpdate100Items",
                Id = "InsertOrUpdate100Items1",
                Data = Guid.NewGuid().ToString()
            });

            List<LiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "InsertOrUpdate100Items",
                    Id = "InsertOrUpdate100Items1" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var itemsFirst = await _repository.InsertOrUpdateAsync(list);
            var itemsSecond = await _repository.InsertOrUpdateAsync(list);

            itemsFirst.Should().Be(100);
            itemsSecond.Should().Be(0);
        }

        #endregion

        #region Update

        [Fact]
        public async Task UpdateOneItem()
        {
            var insertOneItem = await _repository.InsertAsync(new LiteDbItem
            {
                PartKey = "UpdateOneItem",
                Id = "rk",
                Data = "test"
            });

            var updateFirstItem = await _repository.UpdateAsync(new LiteDbItem
            {
                PartKey = "UpdateOneItem",
                Id = "rk",
                Data = "test-test"
            });

            var updateSecondItem = await _repository.UpdateAsync(new LiteDbItem
            {
                PartKey = "UpdateOneItem",
                Id = "rk-rk",
                Data = "test"
            });

            insertOneItem.Should().NotBeNull();
            updateFirstItem.Should().NotBeNull();
            updateSecondItem.Should().BeNull();
        }

        [Fact]
        public async Task UpdateListOfItems()
        {
            List<LiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "UpdateListOfItems",
                    Id = "UpdateListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            list.Clear();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "UpdateListOfItems",
                    Id = "UpdateListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var updatedItems = await _repository.UpdateAsync(list);

            items.Should().Be(100);
            updatedItems.Should().Be(100);
        }

        [Fact]
        public async Task Update5Items()
        {
            List<LiteDbItem> list = new();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "Update5Items",
                    Id = "Update5Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
            list.Clear();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "Update5Items",
                    Id = "Update5Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var updatedItems = await _repository.UpdateAsync(list);

            items.Should().Be(5);
            updatedItems.Should().Be(5);
        }

        [Fact]
        public async Task Update10Items()
        {
            List<LiteDbItem> list = new();

            for (var i = 0; i < 10; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "Update10Items",
                    Id = "Update10Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
            list.Clear();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "Update10Items",
                    Id = "Update10Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var insertedItems = await _repository.InsertAsync(list);

            items.Should().Be(10);
            insertedItems.Should().Be(0);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteOneItemById()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new LiteDbItem
            {
                Id = "DeleteOneItemById",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var deleteOneItem = await _repository.DeleteAsync("DeleteOneItemById");
            insertOneItem.Should().NotBeNull();
            deleteOneItem.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteOneItem()
        {
            var item = new LiteDbItem
            {
                PartKey = "DeleteOneItem",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            };

            var insertOneItem = await _repository.InsertOrUpdateAsync(item);

            var deleteOneTimer = await _repository.DeleteAsync(item);
            insertOneItem.Should().NotBeNull();
            deleteOneTimer.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteListOfItems()
        {
            List<LiteDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "DeleteListOfItems",
                    Id = "DeleteListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);
            var deletedItems = await _repository.DeleteAsync(list);

            deletedItems.Should().Be(150);
            items.Should().Be(150);
        }

        [Fact]
        public async Task DeleteListOfItemsById()
        {
            List<LiteDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "DeleteListOfItemsById",
                    Id = "DeleteListOfItemsById" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);
            var deletedItems = await _repository.DeleteAsync(list.Select(s => s.Id));

            items.Should().Be(150);
            deletedItems.Should().Be(150);
        }

        [Fact]
        public async Task DeleteByQuery()
        {
            List<LiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    PartKey = "DeleteByQuery",
                    Id = "DeleteByQuery" + i,
                    IntData = i,
                    Data = i >= 50 ? i.ToString() : string.Empty
                });
            }

            await _repository.InsertOrUpdateAsync(list);
            var items = await _repository.DeleteAsync(w => w.PartKey == "DeleteByQuery" && w.IntData >= 50);
            items.Should().Be(50);
        }

        [Fact]
        public async Task DeleteAll()
        {
            List<LiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new LiteDbItem
                {
                    Id = "DeleteAll" + i,
                    PartKey = "DeleteAll",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);
            var deletedItems = await _repository.DeleteAllAsync();
            var count = await _repository.CountAsync();

            deletedItems.Should().BeTrue();
            items.Should().Be(100);
            count.Should().Be(0);
        }

        #endregion

        #region Get

        [Fact]
        public async Task GetByWrongId()
        {
            var insertOneItem = await _repository.InsertAsync(new LiteDbItem
            {
                PartKey = "GetByWrongId",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var item = await _repository.GetAsync("GetByWrongId");
            insertOneItem.Should().NotBeNull();
            item.Should().BeNull();
        }

        [Fact]
        public async Task GetById()
        {
            var items = new List<LiteDbItem>();
            for (var i = 0; i < 100; i++)
            {
                items.Add(new LiteDbItem
                {
                    Id = "GetById" + i,
                    PartKey = "GetById",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var insertOneItem = await _repository.InsertOrUpdateAsync(items);

            var item = await _repository.GetAsync("GetById10");
            insertOneItem.Should().Be(100);
            item.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByQuery()
        {
            var items = new List<LiteDbItem>();
            for (var i = 0; i < 100; i++)
            {
                items.Add(new LiteDbItem
                {
                    PartKey = "GetByQuery",
                    Id = "GetByQuery" + i,
                    RowKey = "4",
                    Data = $"item{i}"
                });
            }

            var addedItems = await _repository.InsertOrUpdateAsync(items);

            var item = await _repository.GetAsync(w => w.Data == "item4" && w.RowKey == "4" && w.PartKey == "GetByQuery");
            addedItems.Should().Be(100);
            item.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByWrongQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new LiteDbItem
                {
                    PartKey = "GetByWrongQuery",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var item = await _repository.GetAsync(w => w.Data == "some");
            item.Should().BeNull();
        }

        #endregion

        #region Count

        [Fact]
        public async Task Count()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new LiteDbItem
            {
                PartKey = "Count",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var count = await _repository.CountAsync();
            insertOneItem.Should().NotBeNull();
            count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public async Task CountByQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new LiteDbItem
                {
                    PartKey = "CountByQuery",
                    Id = "CountByQuery" + i,
                    IntData = i
                });
            }

            var count = await _repository.CountAsync(w => w.PartKey == "CountByQuery" && w.IntData == 4);
            count.Should().Be(1);
        }

        #endregion
    }
}