using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.Core;
using ManagedCode.Repository.Core.Extensions;
using ManagedCode.Repository.Tests.Common;
using Xunit;

namespace ManagedCode.Repository.Tests
{
    public abstract class BaseRepositoryTests<TId, TItem> where TItem : IItem<TId>
    {
        protected readonly IRepository<TId, TItem> Repository;

        protected BaseRepositoryTests(IRepository<TId, TItem> repository)
        {
            Repository = repository;
        }

        protected abstract TId GenerateId();
        protected abstract TItem CreateNewItem();
        protected abstract TItem CreateNewItem(TId id);
        
        [Fact]
        public async Task InitializeAsync()
        {
            await Repository.InitializeAsync();
            Repository.IsInitialized.Should().BeTrue();
            await Repository.InitializeAsync();
            Repository.IsInitialized.Should().BeTrue();
        }
        
#region Insert

        [Fact]
        public virtual async Task InsertOneItem()
        {
            var id = GenerateId();
            var firstItem = CreateNewItem(id);
            var secondItem = CreateNewItem(id);
            
            var insertFirstItem = await Repository.InsertAsync(firstItem);
            var insertSecondItem = await Repository.InsertAsync(secondItem);

            insertFirstItem.Should().NotBeNull();
            insertSecondItem.Should().BeNull();
        }

        [Fact]
        public virtual async Task InsertListOfItems()
        {
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Repository.InsertAsync(list);

            items.Should().Be(100);
        }

        [Fact]
        public virtual async Task Insert99Items()
        {
            var id = GenerateId();
            
            await Repository.InsertAsync(CreateNewItem(id));

            List<TItem> list = new();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < 99; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Repository.InsertAsync(list);
            items.Should().Be(99);
        }

        [Fact]
        public virtual async Task InsertOrUpdateOneItem()
        {
            var insertOneItem = await Repository.InsertOrUpdateAsync(new InMemoryItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            insertOneItem.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task InsertOrUpdateListOfItems()
        {
            List<InMemoryItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await Repository.InsertOrUpdateAsync(list);

            items.Should().Be(100);
        }

        [Fact]
        public virtual async Task InsertOrUpdate100Items()
        {
            await Repository.InsertOrUpdateAsync(new InMemoryItem
            {
                Id = 99,
                Data = Guid.NewGuid().ToString()
            });

            List<InMemoryItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await Repository.InsertOrUpdateAsync(list);

            items.Should().Be(100);
        }

        #endregion
    }
    
    public class InMemoryRepositoryTests : BaseRepositoryTests<int, InMemoryItem>
    {
        private static int _count = 0;
        public override int GenerateId()
        {
            _count++;
            return _count;
        }

        public override InMemoryItem CreateNewItem()
        {
            return new InMemoryItem
            {
                Id = GenerateId(),
                Data = Guid.NewGuid().ToString()
            };
        }
        
        public override InMemoryItem CreateNewItem(int id)
        {
            return new InMemoryItem
            {
                Id = id,
                Data = Guid.NewGuid().ToString()
            };
        }

        public InMemoryRepositoryTests() : base(new InMemoryRepository<int, InMemoryItem>())
        {
            Repository.InitializeAsync().Wait();
        }

        

        

        #region Find

        [Fact]
        public async Task FindByCondition()
        {
            for (var i = 1000; i < 1010; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await Repository
                .FindAsync(Repository
                    .CreateCondition(x => x.Id != 1001, x => x.Id != 1002))
                .ToListAsync();

            items.Count.Should().Be(8);
        }

        [Fact]
        public async Task Find()
        {
            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await Repository.FindAsync(w => w.Id >= 50).ToListAsync();
            items.Count.Should().Be(50);
        }

        [Fact]
        public async Task FindTakeSkip()
        {
            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await Repository.FindAsync(w => w.Id > 0, 15, 10).ToListAsync();
            items.Count.Should().Be(15);
            items.First().Id.Should().Be(11);
            items.Last().Id.Should().Be(25);
        }

        [Fact]
        public async Task FindTake()
        {
            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await Repository.FindAsync(w => w.Id >= 50, 10).ToListAsync();
            items.Count.Should().Be(10);
            items.First().Id.Should().Be(50);
        }

        [Fact]
        public async Task FindSkip()
        {
            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await Repository.FindAsync(w => w.Id >= 50, skip: 10).ToListAsync();
            items.Count.Should().Be(40);
            items.First().Id.Should().Be(60);
        }

        [Fact]
        public async Task FindOrder()
        {
            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var items = await Repository.FindAsync(w => w.Id >= 9,
                    o => o.Id, 10, 1)
                .ToListAsync();

            var itemsByDescending = await Repository.FindAsync(w => w.Id >= 10,
                    o => o.Id, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].Id.Should().Be(10);
            items[1].Id.Should().Be(11);

            itemsByDescending.Count.Should().Be(10);
            itemsByDescending[0].Id.Should().Be(99);
            itemsByDescending[1].Id.Should().Be(98);
        }

        [Fact]
        public async Task FindOrderThen()
        {
            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i % 2}"
                });
            }

            var items = await Repository.FindAsync(w => w.Id >= 9,
                    o => o.Data, t => t.Id, 10, 1)
                .ToListAsync();

            var itemsBy = await Repository.FindAsync(w => w.Id >= 10,
                    o => o.Data, Order.ByDescending, t => t.Id, 10)
                .ToListAsync();

            var itemsThenByDescending = await Repository.FindAsync(w => w.Id >= 10,
                    o => o.Data, Order.ByDescending, t => t.Id, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].Id.Should().Be(12);
            items[1].Id.Should().Be(14);

            itemsBy.Count.Should().Be(10);
            itemsBy[0].Id.Should().Be(11);
            itemsBy[1].Id.Should().Be(13);

            itemsThenByDescending.Count.Should().Be(10);
            itemsThenByDescending[0].Id.Should().Be(99);
            itemsThenByDescending[1].Id.Should().Be(97);
        }

        #endregion

        #region Update

        [Fact]
        public async Task UpdateOneItem()
        {
            var insertOneItem = await Repository.InsertAsync(new InMemoryItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var updateFirstItem = await Repository.UpdateAsync(new InMemoryItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var updateSecondItem = await Repository.UpdateAsync(new InMemoryItem
            {
                Id = 2,
                Data = Guid.NewGuid().ToString()
            });

            insertOneItem.Should().NotBeNull();
            updateFirstItem.Should().NotBeNull();
            updateSecondItem.Should().BeNull();
        }

        [Fact]
        public async Task UpdateListOfItems()
        {
            List<InMemoryItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await Repository.InsertAsync(list);

            list.Clear();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var updatedItems = await Repository.UpdateAsync(list);

            items.Should().Be(100);
            updatedItems.Should().Be(100);
        }

        [Fact]
        public async Task Update5Items()
        {
            List<InMemoryItem> list = new();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await Repository.InsertAsync(list);
            list.Clear();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var updatedItems = await Repository.UpdateAsync(list);

            items.Should().Be(5);
            updatedItems.Should().Be(5);
        }

        [Fact]
        public async Task Update10Items()
        {
            List<InMemoryItem> list = new();

            for (var i = 0; i < 10; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await Repository.InsertAsync(list);

            for (var i = 0; i < 100; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var insertedItems = await Repository.InsertAsync(list);

            items.Should().Be(10);
            insertedItems.Should().Be(90);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteOneItemById()
        {
            var insertOneItem = await Repository.InsertAsync(new InMemoryItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var deleteOneTimer = await Repository.DeleteAsync(1);
            insertOneItem.Should().NotBeNull();
            deleteOneTimer.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteOneItem()
        {
            var item = new InMemoryItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            };

            var insertOneItem = await Repository.InsertAsync(item);

            var deleteOneTimer = await Repository.DeleteAsync(item);
            insertOneItem.Should().NotBeNull();
            deleteOneTimer.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteListOfItems()
        {
            List<InMemoryItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await Repository.InsertAsync(list);
            var deletedItems = await Repository.DeleteAsync(list);

            deletedItems.Should().Be(100);
            items.Should().Be(100);
        }

        [Fact]
        public async Task DeleteListOfItemsById()
        {
            List<InMemoryItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await Repository.InsertAsync(list);
            var ids = Enumerable.Range(0, 100);
            var deletedItems = await Repository.DeleteAsync(ids);

            deletedItems.Should().Be(100);
            items.Should().Be(100);
        }

        [Fact]
        public async Task DeleteByQuery()
        {
            List<InMemoryItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await Repository.DeleteAsync(w => w.Id >= 50);
            items.Should().Be(50);
        }

        [Fact]
        public async Task DeleteAll()
        {
            List<InMemoryItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new InMemoryItem
                {
                    Id = i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await Repository.InsertAsync(list);
            var deletedItems = await Repository.DeleteAllAsync();
            var count = await Repository.CountAsync();

            deletedItems.Should().BeTrue();
            items.Should().Be(100);
            count.Should().Be(0);
        }

        #endregion

        #region Get

        [Fact]
        public async Task GetByWrongId()
        {
            var insertOneItem = await Repository.InsertAsync(new InMemoryItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var item = await Repository.GetAsync(2);
            insertOneItem.Should().NotBeNull();
            item.Should().BeNull();
        }

        [Fact]
        public async Task GetById()
        {
            var insertOneItem = await Repository.InsertAsync(new InMemoryItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var item = await Repository.GetAsync(1);
            insertOneItem.Should().NotBeNull();
            item.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var item = await Repository.GetAsync(w => w.Data == "item4");
            item.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByWrongQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var item = await Repository.GetAsync(w => w.Data == "some");
            item.Should().BeNull();
        }

        #endregion

        #region Count

        [Fact]
        public async Task Count()
        {
            var insertOneItem = await Repository.InsertAsync(new InMemoryItem
            {
                Id = 1,
                Data = Guid.NewGuid().ToString()
            });

            var count = await Repository.CountAsync();
            insertOneItem.Should().NotBeNull();
            count.Should().Be(1);
        }

        [Fact]
        public async Task CountByQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await Repository.InsertAsync(new InMemoryItem
                {
                    Id = i,
                    Data = $"item{i}"
                });
            }

            var count = await Repository.CountAsync(w => w.Data == "item4");
            count.Should().Be(1);
        }

        #endregion


    }
}