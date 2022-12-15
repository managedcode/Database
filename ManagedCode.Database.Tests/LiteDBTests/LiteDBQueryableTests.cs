using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.LiteDBTests;

public class LiteDBQueryableTests : BaseQueryableTests<string, TestLiteDBItem>
{
    public LiteDBQueryableTests() : base(new LiteDBTestContainer())
    {
    }
}