using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.BaseTests
{
    public abstract class BaseMultiThreadingTests<TId, TItem> : BaseTests<TId, TItem>
        where TItem : IBaseItem<TId>, new()
    {
        protected BaseMultiThreadingTests(ITestContainer<TId, TItem> testContainer) : base(testContainer)
        {
        }

        public Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public Task DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}