using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.LiteDBTests;

public class LiteDBCollectionTests : BaseCollectionTests<string, TestLiteDBItem>
{
    public LiteDBCollectionTests() : base(new LiteDBTestContainer())
    {

    }
}