using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.BaseTests
{
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
                DateTimeData = DateTime.Now,
            };
        }

        protected TItem CreateNewItem(TId id)
        {
            var item = CreateNewItem();
            item.Id = id;
            return item;
        }

        #region WhereQuery

        #region Where

        [Fact]
        public virtual async Task Where_ReturnOk()
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
        public virtual async Task Where_ReturnNull()
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

        #endregion

        #region WhereOrderBy

        #region WhereOrderBy

        [Fact]
        public virtual async Task WhereOrderBy_ReturnOk()
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
            var itemsResult = await Collection.Query.Where(w => w.StringData == guid).OrderBy(o => o.IntData)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert);
            itemsResult.First().IntData.Should().Be(0);
        }

        #endregion

        #region WhereOrderByTake

        [Fact]
        public virtual async Task WhereOrderByTake_ReturnOk()
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
        public virtual async Task WhereOrderByTakeSkip_ReturnOk()
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

        #endregion

        #region WhereOrderBySkip

        [Fact]
        public virtual async Task WhereOrderBySkip_ReturnOk()
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
        public virtual async Task WhereOrderBySkipTake_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

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
                .OrderBy(o => o.IntData).Skip(itemsCountToSkip).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        #endregion

        #endregion

        #region WhereOrderByDescending

        #region WhereOrderByDescending

        [Fact]
        public virtual async Task WhereOrderByDescending_ReturnOk()
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
            var itemsResult = await Collection.Query.Where(w => w.StringData == guid).OrderByDescending(o => o.IntData)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
        }

        #endregion

        #region WhereOrderByDescendingTake

        [Fact]
        public virtual async Task WhereOrderByDescendingTake_ReturnOk()
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
        public virtual async Task WhereOrderByDescendingTakeSkip_ReturnOk()
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

        #endregion

        #region WhereOrderByDescendingSkip

        [Fact]
        public virtual async Task WhereOrderByDescendingSkip_ReturnOk()
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
        public virtual async Task WhereOrderByDescendingSkipTake_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

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
                .OrderByDescending(o => o.IntData).Skip(itemsCountToSkip).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
        }

        #endregion

        #endregion

        #region WhereTake

        #region WhereTake

        [Fact]
        public virtual async Task WhereTake_ReturnOk()
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
            var itemsResult = await Collection.Query.Where(w => w.StringData == guid).Take(itemsCountToTake)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().StringData.Should().Be(guid);
        }

        #endregion

        #region WhereTakeOrderby

        [Fact]
        public virtual async Task WhereTakeOrderBy_ReturnOk()
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
                .Take(itemsCountToTake).OrderBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
        }

        [Fact]
        public virtual async Task WhereTakeOrderBySkip_ReturnOk()
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
                .Take(itemsCountToTake).OrderBy(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        #endregion

        #region WhereTakeOrderbyDescending

        [Fact]
        public virtual async Task WhereTakeOrderByDescending_ReturnOk()
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
                .Take(itemsCountToTake).OrderByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
        }

        [Fact]
        public virtual async Task WhereTakeOrderByDescendingSkip_ReturnOk()
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
                .Take(itemsCountToTake).OrderByDescending(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        #endregion

        #region WhereTakeSkip

        [Fact]
        public virtual async Task WhereTakeSkipOrderBy_ReturnOk()
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
                .Take(itemsCountToTake).Skip(itemsCountToSkip).OrderBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task WhereTakeSkipOrderByDescendingSkip_ReturnOk()
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
                .Take(itemsCountToTake).Skip(itemsCountToSkip).OrderByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        #endregion

        #endregion

        #region WhereSkip

        #region WhereSkip

        public virtual async Task WhereSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.Where(w => w.StringData == guid).Skip(itemsCountToSkip)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
        }

        #endregion

        #region WhereSkipOrderBy

        [Fact]
        public virtual async Task WhereSkipOrderBy_ReturnOk()
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
                .Skip(itemsCountToSkip).OrderBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task WhereSkipOrderByTake_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

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
                .Skip(itemsCountToSkip).OrderBy(o => o.IntData).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
        }

        #endregion

        #region WhereSkipOrderByDescending

        [Fact]
        public virtual async Task WhereSkipOrderByDescending_ReturnOk()
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
                .Skip(itemsCountToSkip).OrderByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task WhereSkipOrderByDescendingTake_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

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
                .Skip(itemsCountToSkip).OrderByDescending(o => o.IntData).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
        }

        #endregion

        #region WhereSkipTake

        [Fact]
        public virtual async Task WhereSkipTakeOrderBy_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

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
                .Skip(itemsCountToSkip).Take(itemsCountToTake).OrderBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
        }

        [Fact]
        public virtual async Task WhereSkipTakeOrderByDescending_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

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
                .Skip(itemsCountToSkip).Take(itemsCountToTake).OrderByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
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

        #endregion

        #region OrderByWhere

        [Fact]
        public virtual async Task OrderByWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Where(w => w.StringData == guid)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert);
            itemsResult.First().IntData.Should().Be(0);
        }

        [Fact]
        public virtual async Task OrderByWhereTake_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData)
                .Where(w => w.StringData == guid).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(0);
        }

        [Fact]
        public virtual async Task OrderByWhereTakeSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Where(w => w.StringData == guid)
                .Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        [Fact]
        public virtual async Task OrderByWhereSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData)
                .Where(w => w.StringData == guid).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        [Fact]
        public virtual async Task OrderByWhereSkipTake_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Where(w => w.StringData == guid)
                .Skip(itemsCountToSkip).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        #endregion

        #region OrderByTake

        [Fact]
        public virtual async Task OrderByTake_ReturnOk()
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
        public virtual async Task OrderByTakeWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData)
                .Take(itemsCountToTake).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(0);
        }

        public virtual async Task OrderByTakeSkip_ReturnOk()
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
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData)
                .Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        [Fact]
        public virtual async Task OrderByTakeWhereSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Take(itemsCountToTake)
                .Where(w => w.StringData == guid).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        [Fact]
        public virtual async Task OrderByTakeSkipWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Take(itemsCountToTake)
                .Skip(itemsCountToSkip).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        #endregion

        #region OrderBySkip

        public virtual async Task OrderBySkip_ReturnOk()
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
            var itemsResult = await Collection.Query.Where(w => w.StringData == guid).Skip(itemsCountToSkip)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        [Fact]
        public virtual async Task OrderBySkipWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Skip(itemsCountToSkip)
                .Where(w => w.StringData == guid).ToListAsync();

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
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData)
                .Skip(itemsCountToSkip).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        [Fact]
        public virtual async Task OrderBySkipTakeWhere_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Skip(itemsCountToSkip)
                .Take(itemsCountToTake).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }

        [Fact]
        public virtual async Task OrderBySkipWhereTake_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderBy(o => o.IntData).Skip(itemsCountToSkip)
                .Where(w => w.StringData == guid).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
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

        #endregion

        #region OrderByDescendingWhere

        [Fact]
        public virtual async Task OrderByDescendingWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Where(w => w.StringData == guid)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);

        }

        [Fact]
        public virtual async Task OrderByDescendingWhereTake_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData)
                .Where(w => w.StringData == guid).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
        }

        [Fact]
        public virtual async Task OrderByDescendingWhereTakeSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Where(w => w.StringData == guid)
                .Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
        }

        [Fact]
        public virtual async Task OrderByDescendingWhereSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData)
                .Where(w => w.StringData == guid).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
        }

        [Fact]
        public virtual async Task OrderByDescendingWhereSkipTake_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Where(w => w.StringData == guid)
                .Skip(itemsCountToSkip).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);

        }

        #endregion

        #region OrderByDescendingTake

        [Fact]
        public virtual async Task OrderByDescendingTake_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Take(itemsCountToTake)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);

        }

        [Fact]
        public virtual async Task OrderByDescendingTakeWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData)
                .Take(itemsCountToTake).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);

        }

        public virtual async Task OrderByDescendingTakeSkip_ReturnOk()
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
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData)
                .Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);

        }

        [Fact]
        public virtual async Task OrderByDescendingTakeWhereSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Take(itemsCountToTake)
                .Where(w => w.StringData == guid).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
        }

        [Fact]
        public virtual async Task OrderByDescendingTakeSkipWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Take(itemsCountToTake)
                .Skip(itemsCountToSkip).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
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

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Skip(itemsCountToSkip)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
        }

        [Fact]
        public virtual async Task OrderByDescendingSkipWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Skip(itemsCountToSkip)
                .Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
        }

        [Fact]
        public virtual async Task OrderByDescendingSkipTake_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData)
                .Skip(itemsCountToSkip).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
        }

        [Fact]
        public virtual async Task OrderByDescendingSkipTakeWhere_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Skip(itemsCountToSkip)
                .Take(itemsCountToTake).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
        }

        [Fact]
        public virtual async Task OrderByDescendingSkipWhereTake_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.OrderByDescending(o => o.IntData).Skip(itemsCountToSkip)
                .Where(w => w.StringData == guid).Take(itemsCountToTake).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
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

        #endregion

        #region TakeOrderBy

        [Fact]
        public virtual async Task TakeOrderBy_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake).OrderBy(o => o.IntData)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(0);
        }

        [Fact]
        public virtual async Task TakeOrderByWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake)
                .OrderBy(o => o.IntData).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(0);
        }

        [Fact]
        public virtual async Task TakeOrderByWhereSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake).OrderBy(o => o.IntData)
                .Where(w => w.StringData == guid).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeOrderBySkip_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake)
                .OrderBy(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeOrderBySkipWhere_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.Take(itemsCountToTake)
                .OrderBy(o => o.IntData).Skip(itemsCountToSkip).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToSkip);
        }


        #endregion

        #region TakeOrderByDescending

        [Fact]
        public virtual async Task TakeOrderByDescending_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake).OrderByDescending(o => o.IntData)
                .ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToTake - 1);
        }

        [Fact]
        public virtual async Task TakeOrderByDescendingWhere_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake)
                .OrderByDescending(o => o.IntData).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
            itemsResult.First().IntData.Should().Be(itemsCountToTake - 1);
        }

        [Fact]
        public virtual async Task TakeOrderByDescendingWhereSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake).OrderByDescending(o => o.IntData)
                .Where(w => w.StringData == guid).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
            itemsResult.First().IntData.Should().Be(itemsCountToTake - itemsCountToSkip - 1);
        }

        [Fact]
        public virtual async Task TakeOrderByDescendingSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake)
                .OrderByDescending(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeOrderByDescendingSkipWhere_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.Take(itemsCountToTake).OrderByDescending(o => o.IntData)
                .Skip(itemsCountToSkip).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
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

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Where(w => w.StringData == guid)
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

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.Take(itemsCountToTake)
                .Where(w => w.StringData == guid).OrderBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
        }

        [Fact]
        public virtual async Task TakeWhereOrderBySkip_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Where(w => w.StringData == guid)
                .OrderBy(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeWhereOrderByDescending_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake)
                .Where(w => w.StringData == guid).OrderByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake);
        }

        [Fact]
        public virtual async Task TakeWhereOrderByDescendingSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Where(w => w.StringData == guid)
                .OrderByDescending(o => o.IntData).Skip(itemsCountToSkip).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeWhereSkipOrderBy_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Where(w => w.StringData == guid)
                .Skip(itemsCountToSkip).OrderBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeWhereSkipOrderByDescendingSkip_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Where(w => w.StringData == guid)
                .Skip(itemsCountToSkip).OrderByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Skip(itemsCountToSkip).ToListAsync();

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

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.Take(itemsCountToTake)
                .Skip(itemsCountToSkip).OrderBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeSkipOrderByWhere_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Skip(itemsCountToSkip)
                .OrderBy(o => o.IntData).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeSkipOrderByDescending_ReturnOk()
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
            var itemsResult = await Collection.Query.Take(itemsCountToTake)
                .Skip(itemsCountToSkip).OrderByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeSkipOrderByDescendingWhere_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Skip(itemsCountToSkip)
                .OrderByDescending(o => o.IntData).Where(w => w.StringData == guid).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }


        [Fact]
        public virtual async Task TakeSkipWhereOrderBy_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Skip(itemsCountToSkip)
               .Where(w => w.StringData == guid).OrderBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }

        [Fact]
        public virtual async Task TakeSkipWhereOrderByDescending_ReturnOk()
        {
            // Arrange
            int itemsCountToInsert = 5;
            int itemsCountToTake = 3;
            int itemsCountToSkip = 2; //Take + Skip <= count

            var guid = Guid.NewGuid().ToString();

            for (var i = 0; i < itemsCountToInsert; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                item.StringData = guid;
                await Collection.InsertAsync(item);
            }

            // Act
            var itemsResult = await Collection.Query.Take(itemsCountToTake).Skip(itemsCountToSkip)
                .Where(w => w.StringData == guid).OrderByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToTake - itemsCountToSkip);
        }
        
        #endregion

        #endregion


        [Fact]
        public virtual async Task ThenBy_AfterOrderBy_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.StringData)
                .ThenBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert);
            itemsResult.First().IntData.Should().Be(0);
        }

        [Fact]
        public virtual async Task ThenByDescending_AfterOrderBy_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderBy(o => o.StringData)
                .ThenByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
        }

        [Fact]
        public virtual async Task ThenBy_AfterOrderByDescending_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.StringData)
                .ThenBy(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert);
            itemsResult.First().IntData.Should().Be(0);
        }

        [Fact]
        public virtual async Task ThenByDescending_AfterOrderByDescending_ReturnOk()
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
            var itemsResult = await Collection.Query.OrderByDescending(o => o.StringData)
                .ThenByDescending(o => o.IntData).ToListAsync();

            // Assert
            itemsResult.Count.Should().Be(itemsCountToInsert);
            itemsResult.First().IntData.Should().Be(itemsCountToInsert - 1);
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
        public virtual async Task Skip_ReturnOk()
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
    }
}