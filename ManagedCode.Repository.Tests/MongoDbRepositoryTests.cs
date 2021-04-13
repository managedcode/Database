using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.Core;
using ManagedCode.Repository.MongoDB;
using ManagedCode.Repository.Tests.Common;
using MongoDB.Bson;
using Xunit;

namespace ManagedCode.Repository.Tests
{
    public class MongoDbRepositoryTests
    {
        public const string ConnecntionString =
            "mongodb://localhost:55000";

        private readonly IMongoDbRepository<TestMongoDbItem> _repository = new MongoDbRepository<TestMongoDbItem>(null, new MongoDbRepositoryOptions
        {
            ConnectionString = ConnecntionString,
            DataBaseName = "db"
        });

        public MongoDbRepositoryTests()
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
            var localRepository = new MongoDbRepository<TestMongoDbItem>(null, new MongoDbRepositoryOptions
            {
                ConnectionString = ConnecntionString
            });

            localRepository.IsInitialized.Should().BeFalse();

            var item = await localRepository.InsertAsync(new TestMongoDbItem());

            item.Should().NotBeNull();
        }

        #region Find

        [Fact(Skip = "Emulator issue")]
        public async Task Find()
        {
            var list = new List<TestMongoDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "Find",
                    Id = ObjectId.GenerateNewId(),
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartKey == "Find" && w.IntData >= 50).ToListAsync();
            items.Count.Should().Be(50);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindTakeSkip()
        {
            var list = new List<TestMongoDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "FindTakeSkip",
                    Id = ObjectId.GenerateNewId(),
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

        [Fact(Skip = "Emulator issue")]
        public async Task FindTake()
        {
            var list = new List<TestMongoDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "FindTake",
                    Id = ObjectId.GenerateNewId(),
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

        [Fact(Skip = "Emulator issue")]
        public async Task FindSkip()
        {
            var list = new List<TestMongoDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "FindSkip",
                    Id = ObjectId.GenerateNewId(),
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

        [Fact(Skip = "Emulator issue")]
        public async Task FindOrder()
        {
            var list = new List<TestMongoDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "FindOrder",
                    Id = ObjectId.GenerateNewId(),
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
            items[0].IntData.Should().Be(11);
            items[1].IntData.Should().Be(12);

            itemsByDescending.Count.Should().Be(10);
            itemsByDescending[0].IntData.Should().Be(99);
            itemsByDescending[1].IntData.Should().Be(98);
        }

        [Fact(Skip = "The order by query does not have a corresponding composite index that it can be served from. CompositeIndexes required.")]
        public async Task FindOrderThen()
        {
            var list = new List<TestMongoDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "FindOrderThen",
                    Id = ObjectId.GenerateNewId(),
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

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOneItem()
        {
            var id = ObjectId.GenerateNewId();
            var insertFirstItem = await _repository.InsertAsync(new TestMongoDbItem
            {
                Id = id,
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var insertSecondItem = await _repository.InsertAsync(new TestMongoDbItem
            {
                Id = id,
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            insertFirstItem.Should().NotBeNull();
            insertSecondItem.Should().BeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertListOfItems()
        {
            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = $"InsertListOfItems{i % 2}",
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            items.Should().Be(150);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task Insert100Items()
        {
            var item = new TestMongoDbItem
            {
                RowKey = "Insert100Items",
                Data = Guid.NewGuid().ToString()
            };
            await _repository.InsertAsync(item);

            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    RowKey = "Insert100Items",
                    Data = Guid.NewGuid().ToString()
                });
            }

            list[0] = item;

            var items = await _repository.InsertAsync(list);

            items.Should().Be(149);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOrUpdateOneItem()
        {
            var id = ObjectId.GenerateNewId();
            var insertOneItem = await _repository.InsertOrUpdateAsync(new TestMongoDbItem
            {
                PartKey = "InsertOrUpdateOneItem",
                Id = id,
                Data = Guid.NewGuid().ToString()
            });

            var insertTwoItem = await _repository.InsertOrUpdateAsync(new TestMongoDbItem
            {
                PartKey = "InsertOrUpdateOneItem",
                Id = id,
                Data = Guid.NewGuid().ToString()
            });

            insertOneItem.Should().NotBeNull();
            insertTwoItem.Should().NotBeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOrUpdateListOfItems()
        {
            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = $"InsertOrUpdateListOfItems{i % 2}",
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
            await _repository.InsertOrUpdateAsync(new TestMongoDbItem
            {
                PartKey = "InsertOrUpdate100Items",
                Data = Guid.NewGuid().ToString()
            });

            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "InsertOrUpdate100Items",
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
            var id = ObjectId.GenerateNewId();
            var insertOneItem = await _repository.InsertAsync(new TestMongoDbItem
            {
                PartKey = "UpdateOneItem",
                Id = id,
                Data = "test"
            });

            var updateFirstItem = await _repository.UpdateAsync(new TestMongoDbItem
            {
                PartKey = "UpdateOneItem",
                Id = id,
                Data = "test-test"
            });

            var updateSecondItem = await _repository.UpdateAsync(new TestMongoDbItem
            {
                PartKey = "UpdateOneItem",
                Id = ObjectId.GenerateNewId(),
                Data = "test"
            });

            insertOneItem.Should().NotBeNull();
            updateFirstItem.Should().NotBeNull();
            updateSecondItem.Should().BeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task UpdateListOfItems()
        {
            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "UpdateListOfItems",
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            for (var i = 0; i < 100; i++)
            {
                list[i].Data = Guid.NewGuid().ToString();
            }

            var updatedItems = await _repository.UpdateAsync(list);

            items.Should().Be(100);
            updatedItems.Should().Be(100);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task Update5Items()
        {
            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "Update5Items",
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            for (var i = 0; i < 100; i++)
            {
                list[i].Data = Guid.NewGuid().ToString();
            }

            var updatedItems = await _repository.UpdateAsync(list);

            items.Should().Be(5);
            updatedItems.Should().Be(0);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task Update10Items()
        {
            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 10; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "Update10Items",
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
            ;

            for (var i = 0; i < 100; i++)
            {
                list[i].Data = Guid.NewGuid().ToString();
            }

            var insertedItems = await _repository.InsertAsync(list);

            items.Should().Be(10);
            insertedItems.Should().Be(90);
        }

        #endregion

        #region Delete

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteOneItemById()
        {
            var id = ObjectId.GenerateNewId();
            var insertOneItem = await _repository.InsertOrUpdateAsync(new TestMongoDbItem
            {
                Id = id,
                RowKey = id.ToString(),
                Data = Guid.NewGuid().ToString()
            });

            var deleteOneItem = await _repository.DeleteAsync(id);
            insertOneItem.Should().NotBeNull();
            deleteOneItem.Should().BeTrue();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteOneItem()
        {
            var item = new TestMongoDbItem
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

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteListOfItems()
        {
            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "DeleteListOfItems",
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
            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "DeleteListOfItemsById",
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);
            var deletedItems = await _repository.DeleteAsync(list.Select(s => s.Id));

            items.Should().Be(150);
            deletedItems.Should().Be(150);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteByQuery()
        {
            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
                    PartKey = "DeleteByQuery",
                    IntData = i,
                    Data = i >= 50 ? i.ToString() : string.Empty
                });
            }

            await _repository.InsertOrUpdateAsync(list);
            var items = await _repository.DeleteAsync(w => w.PartKey == "DeleteByQuery" && w.IntData >= 50);
            items.Should().Be(50);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteAll()
        {
            List<TestMongoDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new TestMongoDbItem
                {
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

        [Fact(Skip = "Emulator issue")]
        public async Task GetByWrongId()
        {
            var insertOneItem = await _repository.InsertAsync(new TestMongoDbItem
            {
                PartKey = "GetByWrongId",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var item = await _repository.GetAsync(ObjectId.GenerateNewId());
            insertOneItem.Should().NotBeNull();
            item.Should().BeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task GetById()
        {
            var items = new List<TestMongoDbItem>();
            for (var i = 0; i < 100; i++)
            {
                items.Add(new TestMongoDbItem
                {
                    PartKey = "GetById",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var insertOneItem = await _repository.InsertOrUpdateAsync(items);

            var item = await _repository.GetAsync(items.First().Id);
            insertOneItem.Should().Be(100);
            item.Should().NotBeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task GetByQuery()
        {
            var items = new List<TestMongoDbItem>();
            for (var i = 0; i < 100; i++)
            {
                items.Add(new TestMongoDbItem
                {
                    PartKey = "GetByQuery",
                    RowKey = "4",
                    Data = $"item{i}"
                });
            }

            var addedItems = await _repository.InsertOrUpdateAsync(items);

            var item = await _repository.GetAsync(w => w.Data == "item4" && w.RowKey == "4" && w.PartKey == "GetByQuery");
            addedItems.Should().Be(100);
            item.Should().NotBeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task GetByWrongQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new TestMongoDbItem
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

        [Fact(Skip = "Emulator issue")]
        public async Task Count()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new TestMongoDbItem
            {
                PartKey = "Count",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var count = await _repository.CountAsync();
            insertOneItem.Should().NotBeNull();
            count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task CountByQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new TestMongoDbItem
                {
                    PartKey = "CountByQuery",
                    IntData = i
                });
            }

            var count = await _repository.CountAsync(w => w.PartKey == "CountByQuery" && w.IntData == 4);
            count.Should().Be(1);
        }

        #endregion
    }
}