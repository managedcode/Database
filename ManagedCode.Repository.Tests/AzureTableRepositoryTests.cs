using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.AzureTable;
using ManagedCode.Repository.Core;
using ManagedCode.Repository.Tests.Common;
using Xunit;
using AzureTableItem = ManagedCode.Repository.Tests.Common.AzureTableItem;

namespace ManagedCode.Repository.Tests
{
    public class AzureTableRepositoryTests
    {
        public const string ConnecntionString =
            "DefaultEndpointsProtocol=http;AccountName=localhost;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;TableEndpoint=http://localhost:8902/;";

        private readonly IAzureTableRepository<AzureTableItem> _repository = new AzureTableRepository<AzureTableItem>(ConnecntionString);

        public AzureTableRepositoryTests()
        {
            _repository.InitializeAsync().Wait();
            //_repository.DeleteAllAsync().Wait();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InitializeAsync()
        {
            await _repository.InitializeAsync();
            _repository.IsInitialized.Should().BeTrue();
            await _repository.InitializeAsync();
            _repository.IsInitialized.Should().BeTrue();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task NotInitializedAsync()
        {
            var localRepository = new AzureTableRepository<AzureTableItem>(ConnecntionString);

            localRepository.IsInitialized.Should().BeFalse();

            await localRepository.InsertAsync(new AzureTableItem
            {
                PartitionKey = "NotInitializedAsync",
                RowKey = "rk",
                Data = string.Empty
            });
        }

        #region Find

        [Fact(Skip = "Emulator issue")]
        public async Task Find()
        {
            var list = new List<AzureTableItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "Find",
                    RowKey = i.ToString(),
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartitionKey == "Find" && w.IntData >= 50).ToListAsync();
            items.Count.Should().Be(50);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindTakeSkip()
        {
            var list = new List<AzureTableItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "FindTakeSkip",
                    RowKey = i.ToString(),
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartitionKey == "FindTakeSkip" && w.IntData > 0, 15, 10).ToListAsync();
            items.Count.Should().Be(15);
            items.First().IntData.Should().Be(19);
            items.Last().IntData.Should().Be(31);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindTake()
        {
            var list = new List<AzureTableItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "FindTake",
                    RowKey = i.ToString(),
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartitionKey == "FindTake" && w.IntData >= 50, 10).ToListAsync();
            items.Count.Should().Be(10);
            items.First().IntData.Should().Be(50);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindSkip()
        {
            var list = new List<AzureTableItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "FindSkip",
                    RowKey = i.ToString(),
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartitionKey == "FindSkip" && w.IntData >= 50, skip: 10).ToListAsync();
            items.Count.Should().Be(40);
            items.First().IntData.Should().Be(60);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindOrder()
        {
            var list = new List<AzureTableItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "FindOrder",
                    RowKey = i.ToString(),
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartitionKey == "FindOrder" && w.IntData > 9,
                    o => o.Id, 10, 1)
                .ToListAsync();

            var itemsByDescending = await _repository.FindAsync(w => w.PartitionKey == "FindOrder" && w.IntData > 10,
                    o => o.Id, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].IntData.Should().Be(11);
            items[1].IntData.Should().Be(12);

            itemsByDescending.Count.Should().Be(10);
            itemsByDescending[0].IntData.Should().Be(11);
            itemsByDescending[1].IntData.Should().Be(12);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindOrderThen()
        {
            var list = new List<AzureTableItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "FindOrderThen",
                    RowKey = i.ToString(),
                    IntData = i,
                    Data = $"item{i % 2}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartitionKey == "FindOrderThen" && w.IntData >= 9,
                    o => o.Data, t => t.IntData, 10, 1)
                .ToListAsync();

            var itemsBy = await _repository.FindAsync(w => w.PartitionKey == "FindOrderThen" && w.IntData >= 10,
                    o => o.Data, Order.ByDescending, t => t.IntData, 10)
                .ToListAsync();

            var itemsThenByDescending = await _repository.FindAsync(w => w.PartitionKey == "FindOrderThen" && w.IntData >= 10,
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

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOneItem()
        {
            var insertFirstItem = await _repository.InsertAsync(new AzureTableItem
            {
                PartitionKey = "InsertOneItem",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var insertSecondItem = await _repository.InsertAsync(new AzureTableItem
            {
                PartitionKey = "InsertOneItem",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            insertFirstItem.Should().NotBeNull();
            insertSecondItem.Should().BeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertListOfItems()
        {
            List<AzureTableItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = $"InsertListOfItems{i % 2}",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            items.Should().Be(150);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task Insert100Items()
        {
            await _repository.InsertAsync(new AzureTableItem
            {
                PartitionKey = "Insert100Items",
                RowKey = "140",
                Data = Guid.NewGuid().ToString()
            });

            List<AzureTableItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "Insert100Items",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            items.Should().Be(100);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOrUpdateOneItem()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new AzureTableItem
            {
                PartitionKey = "InsertOrUpdateOneItem",
                RowKey = "1",
                Data = Guid.NewGuid().ToString()
            });

            var insertTwoItem = await _repository.InsertOrUpdateAsync(new AzureTableItem
            {
                PartitionKey = "InsertOrUpdateOneItem",
                RowKey = "1",
                Data = Guid.NewGuid().ToString()
            });

            insertOneItem.Should().BeTrue();
            insertTwoItem.Should().BeTrue();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOrUpdateListOfItems()
        {
            List<AzureTableItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = $"InsertOrUpdateListOfItems{i % 2}",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var itemsFirst = await _repository.InsertOrUpdateAsync(list);
            var itemsSecond = await _repository.InsertOrUpdateAsync(list);

            itemsFirst.Should().Be(100);
            itemsSecond.Should().Be(100);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOrUpdate100Items()
        {
            await _repository.InsertOrUpdateAsync(new AzureTableItem
            {
                PartitionKey = "InsertOrUpdate100Items",
                RowKey = "99",
                Data = Guid.NewGuid().ToString()
            });

            List<AzureTableItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "InsertOrUpdate100Items",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var itemsFirst = await _repository.InsertOrUpdateAsync(list);
            var itemsSecond = await _repository.InsertOrUpdateAsync(list);

            itemsFirst.Should().Be(100);
            itemsSecond.Should().Be(100);
        }

        #endregion

        #region Update

        [Fact(Skip = "Emulator issue")]
        public async Task UpdateOneItem()
        {
            var insertOneItem = await _repository.InsertAsync(new AzureTableItem
            {
                PartitionKey = "UpdateOneItem",
                RowKey = "rk",
                Data = "test"
            });

            var updateFirstItem = await _repository.UpdateAsync(new AzureTableItem
            {
                PartitionKey = "UpdateOneItem",
                RowKey = "rk",
                Data = "test-test"
            });

            var updateSecondItem = await _repository.UpdateAsync(new AzureTableItem
            {
                PartitionKey = "UpdateOneItem",
                RowKey = "rk-rk",
                Data = "test"
            });

            insertOneItem.Should().NotBeNull();
            updateFirstItem.Should().BeTrue();
            updateSecondItem.Should().BeFalse();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task UpdateListOfItems()
        {
            List<AzureTableItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "UpdateListOfItems",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            list.Clear();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "UpdateListOfItems",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var updatedItems = await _repository.UpdateAsync(list);

            items.Should().Be(100);
            updatedItems.Should().Be(100);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task Update5Items()
        {
            List<AzureTableItem> list = new();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "Update5Items",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
            list.Clear();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "Update5Items",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var updatedItems = await _repository.UpdateAsync(list);

            items.Should().Be(5);
            updatedItems.Should().Be(0);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task Update10Items()
        {
            List<AzureTableItem> list = new();

            for (var i = 0; i < 10; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "Update10Items",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "Update10Items",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var insertedItems = await _repository.InsertAsync(list);

            items.Should().Be(10);
            insertedItems.Should().Be(0);
        }

        #endregion

        #region Delete

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteOneItemById()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new AzureTableItem
            {
                PartitionKey = "DeleteOneItemById",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var deleteOneTimer = await _repository.DeleteAsync(new TableId("DeleteOneItemById", "rk"));
            insertOneItem.Should().BeTrue();
            deleteOneTimer.Should().BeTrue();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteOneItem()
        {
            var item = new AzureTableItem
            {
                PartitionKey = "DeleteOneItem",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            };

            var insertOneItem = await _repository.InsertOrUpdateAsync(item);

            var deleteOneTimer = await _repository.DeleteAsync(item);
            insertOneItem.Should().BeTrue();
            deleteOneTimer.Should().BeTrue();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteListOfItems()
        {
            List<AzureTableItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "DeleteListOfItems",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);
            var deletedItems = await _repository.DeleteAsync(list);

            deletedItems.Should().Be(150);
            items.Should().Be(150);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteListOfItemsById()
        {
            List<AzureTableItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "DeleteListOfItemsById",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);
            var ids = Enumerable.Range(0, 150);
            var deletedItems = await _repository.DeleteAsync(ids.Select(s => new TableId("DeleteListOfItemsById", s.ToString())));

            deletedItems.Should().Be(150);
            items.Should().Be(150);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteByQuery()
        {
            List<AzureTableItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "DeleteByQuery",
                    RowKey = i.ToString(),
                    IntData = i,
                    Data = i >= 50 ? i.ToString() : string.Empty
                });
            }

            await _repository.InsertOrUpdateAsync(list);
            var items = await _repository.DeleteAsync(w => w.PartitionKey == "DeleteByQuery" && w.IntData >= 50);
            items.Should().Be(50);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteAll()
        {
            List<AzureTableItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new AzureTableItem
                {
                    PartitionKey = "DeleteAll",
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

        [Fact(Skip = "Emulator issue")]
        public async Task GetByWrongId()
        {
            var insertOneItem = await _repository.InsertAsync(new AzureTableItem
            {
                PartitionKey = "GetByWrongId",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var item = await _repository.GetAsync(new TableId("GetByWrongId", "wrong"));
            insertOneItem.Should().NotBeNull();
            item.Should().BeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task GetById()
        {
            var items = new List<AzureTableItem>();
            for (var i = 0; i < 100; i++)
            {
                items.Add(new AzureTableItem
                {
                    PartitionKey = "GetById",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var insertOneItem = await _repository.InsertOrUpdateAsync(items);

            var item = await _repository.GetAsync(new TableId("GetById", "10"));
            insertOneItem.Should().Be(100);
            item.Should().NotBeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task GetByQuery()
        {
            var items = new List<AzureTableItem>();
            for (var i = 0; i < 100; i++)
            {
                items.Add(new AzureTableItem
                {
                    PartitionKey = "GetByQuery",
                    RowKey = i.ToString(),
                    Data = $"item{i}"
                });
            }

            var addedItems = await _repository.InsertOrUpdateAsync(items);

            var item = await _repository.GetAsync(w => w.Data == "item4" && w.RowKey == "4" && w.PartitionKey == "GetByQuery");
            addedItems.Should().Be(100);
            item.Should().NotBeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task GetByWrongQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new AzureTableItem
                {
                    PartitionKey = "GetByWrongQuery",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var item = await _repository.GetAsync(w => w.Data == "some");
            item.Should().BeNull();
        }

        #endregion

        #region Count

        [Fact(Skip = "Emulator issue")]
        public async Task Count()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new AzureTableItem
            {
                PartitionKey = "Count",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var count = await _repository.CountAsync();
            insertOneItem.Should().BeTrue();
            count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task CountByQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new AzureTableItem
                {
                    PartitionKey = "CountByQuery",
                    RowKey = i.ToString(),
                    IntData = i
                });
            }

            var count = await _repository.CountAsync(w => w.PartitionKey == "CountByQuery" && w.IntData == 4);
            count.Should().Be(1);
        }

        #endregion
    }
}