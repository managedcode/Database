using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Core;
using ManagedCode.Database.LiteDB;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.LiteDbTests
{
    public class LiteDbQueryableTests : BaseQueryableTests<string, TestLiteDbItem>
    {
        public LiteDbQueryableTests() : base(new LiteDBTestContainer())
        {
        }
    }
}