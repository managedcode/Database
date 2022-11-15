using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ManagedCode.Database.Tests.BaseTests
{
    public abstract class BaseMultiThreadingTests<TId, TItem> : IAsyncLifetime
        where TItem : IBaseItem<TId>, new()
    {
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
