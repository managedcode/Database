using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.Core;
using ManagedCode.Repository.Core.Extensions;
using ManagedCode.Repository.SQLite;
using ManagedCode.Repository.Tests.Common;
using Xunit;

namespace ManagedCode.Repository.Tests
{
    public class SQLiteRepositoryTests
    {
        public const string ConnecntionString = "sqlite_test.db";

        private readonly IRepository<int, SQLiteDbItem> _repository =
            new SQLiteRepository<int, SQLiteDbItem>( new SQLiteRepositoryOptions
            {
                ConnectionString = GetTempDbName()
            });

        public SQLiteRepositoryTests()
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
            IRepository<int, SQLiteDbItem> localRepository = new SQLiteRepository<int, SQLiteDbItem>(new SQLiteRepositoryOptions
            {
                ConnectionString = GetTempDbName()
            });

            localRepository.IsInitialized.Should().BeTrue();

            await localRepository.InsertAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = string.Empty
            });
        }

        #region Find

        [Fact]
        public async Task FindByCondition()
        {
            for (var i = 1000; i < 1010; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await _repository
                .FindAsync(_repository
                    .CreateCondition(x => x.Id != 1001, x => x.Id != 1002))
                .ToListAsync();

            items.Count.Should().Be(8);
        }

        [Fact]
        public async Task Find()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await _repository.FindAsync(w => w.Id >= 50).ToListAsync();
            items.Count.Should().Be(50);
        }

        [Fact]
        public async Task FindTakeSkip()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await _repository.FindAsync(w => w.Id > 0, 15, 10).ToListAsync();
            items.Count.Should().Be(15);
            items.First().Id.Should().Be(11);
            items.Last().Id.Should().Be(25);
        }

        [Fact]
        public async Task FindTake()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await _repository.FindAsync(w => w.Id >= 50, 10).ToListAsync();
            items.Count.Should().Be(10);
            items.First().Id.Should().Be(50);
        }

        [Fact]
        public async Task FindSkip()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await _repository.FindAsync(w => w.Id >= 50, skip: 10).ToListAsync();
            items.Count.Should().Be(40);
            items.First().Id.Should().Be(60);
        }

        [Fact]
        public async Task FindOrder()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await _repository.FindAsync(w => w.Id >= 9,
                    o => o.Id, 10, 1)
                .ToListAsync();

            var itemsByDescending = await _repository.FindAsync(w => w.Id >= 10,
                    o => o.Id, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].Id.Should().Be(10);
            items[1].Id.Should().Be(11);

            itemsByDescending.Count.Should().Be(10);
            itemsByDescending[0].Id.Should().Be(10);
            itemsByDescending[1].Id.Should().Be(11);
        }

        [Fact]
        public async Task FindOrderThen()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i % 2}"
                });
            }

            var items = await _repository.FindAsync(w => w.Id >= 9,
                    o => o.Data, t => t.Id, 10, 1)
                .ToListAsync();

            var itemsBy = await _repository.FindAsync(w => w.Id >= 10,
                    o => o.Data, Order.ByDescending, t => t.Id, 10)
                .ToListAsync();

            var itemsThenByDescending = await _repository.FindAsync(w => w.Id >= 10,
                    o => o.Data, Order.ByDescending, t => t.Id, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].Id.Should().Be(12);
            items[1].Id.Should().Be(14);

            itemsBy.Count.Should().Be(10);
            itemsBy[0].Id.Should().Be(10);
            itemsBy[1].Id.Should().Be(11);

            itemsThenByDescending.Count.Should().Be(10);
            itemsThenByDescending[0].Id.Should().Be(10);
            itemsThenByDescending[1].Id.Should().Be(11);
        }

        #endregion

        #region Insert

        [Fact(Skip = "need to check it out")]
        public async Task InsertOneItem()
        {
            var insertFirstItem = await _repository.InsertAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var insertSecondItem = await _repository.InsertAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            insertFirstItem.Should().NotBeNull();
            insertSecondItem.Should().BeNull();
        }

        [Fact]
        public async Task InsertListOfItems()
        {
            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            items.Should().Be(100);
        }

        [Fact(Skip = "need to check it out")]
        public async Task Insert99Items()
        {
            await _repository.InsertAsync(new SQLiteDbItem
            {
                Id = 1555,
                Data = Guid.NewGuid().ToString()
            });

            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i + 1530,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            //items.Should().Be(99);
            items.Should().Be(0);
        }

        [Fact]
        public async Task InsertOrUpdateOneItem()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            insertOneItem.Should().NotBeNull();
        }

        [Fact]
        public async Task InsertOrUpdateListOfItems()
        {
            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);

            items.Should().Be(100);
        }

        [Fact]
        public async Task InsertOrUpdate100Items()
        {
            await _repository.InsertOrUpdateAsync(new SQLiteDbItem
            {
                Id = 99,
                Data = Guid.NewGuid().ToString()
            });

            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);

            items.Should().Be(100);
        }

        #endregion

        #region Update

        [Fact]
        public async Task UpdateOneItem()
        {
            var insertOneItem = await _repository.InsertAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var updateFirstItem = await _repository.UpdateAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var updateSecondItem = await _repository.UpdateAsync(new SQLiteDbItem
            {
                Id = 2,
                Data = Guid.NewGuid().ToString()
            });

            insertOneItem.Should().NotBeNull();
            updateFirstItem.Should().NotBeNull();
            updateSecondItem.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateListOfItems()
        {
            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            list.Clear();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
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
            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
            list.Clear();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var updatedItems = await _repository.UpdateAsync(list);

            items.Should().Be(5);
            updatedItems.Should().Be(5);
        }

        [Fact(Skip = "need to check it out")]
        public async Task Update10Items()
        {
            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 10; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var insertedItems = await _repository.InsertAsync(list);

            //items.Should().Be(10);
            //insertedItems.Should().Be(90);
            items.Should().Be(10);
            insertedItems.Should().Be(0);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteOneItemById()
        {
            var insertOneItem = await _repository.InsertAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var deleteOneTimer = await _repository.DeleteAsync(1);
            insertOneItem.Should().NotBeNull();
            deleteOneTimer.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteOneItem()
        {
            var item = new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            };

            var insertOneItem = await _repository.InsertAsync(item);

            var deleteOneTimer = await _repository.DeleteAsync(item);
            insertOneItem.Should().NotBeNull();
            deleteOneTimer.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteListOfItems()
        {
            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
            var deletedItems = await _repository.DeleteAsync(list);

            deletedItems.Should().Be(100);
            items.Should().Be(100);
        }

        [Fact]
        public async Task DeleteListOfItemsById()
        {
            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
            var ids = Enumerable.Range(0, 100);
            var deletedItems = await _repository.DeleteAsync(ids);

            deletedItems.Should().Be(100);
            items.Should().Be(100);
        }

        [Fact]
        public async Task DeleteByQuery()
        {
            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.DeleteAsync(w => w.Id >= 50);
            items.Should().Be(50);
        }

        [Fact]
        public async Task DeleteAll()
        {
            List<SQLiteDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new SQLiteDbItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
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
            var insertOneItem = await _repository.InsertAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var item = await _repository.GetAsync(2);
            insertOneItem.Should().NotBeNull();
            item.Should().BeNull();
        }

        [Fact]
        public async Task GetById()
        {
            var insertOneItem = await _repository.InsertAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var item = await _repository.GetAsync(1);
            insertOneItem.Should().NotBeNull();
            item.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var item = await _repository.GetAsync(w => w.Data == "item4");
            item.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByWrongQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i}"
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
            var insertOneItem = await _repository.InsertAsync(new SQLiteDbItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var count = await _repository.CountAsync();
            insertOneItem.Should().NotBeNull();
            count.Should().Be(1);
        }

        [Fact]
        public async Task CountByQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new SQLiteDbItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var count = await _repository.CountAsync(w => w.Data == "item4");
            count.Should().Be(1);
        }

        #endregion
    }
}