using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.SQLite;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.SQLiteTests
{
    public class SQLiteRepositoryTests : BaseQueryableTests<int, SQLiteDbItem>
    {
        public SQLiteRepositoryTests() : base(new SQLiteTestContainer())
        {
        }
    }
}