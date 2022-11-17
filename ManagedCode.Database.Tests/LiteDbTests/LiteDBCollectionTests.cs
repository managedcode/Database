using System;
using System.IO;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.LiteDB;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.LiteDbTests;

public class LiteDBCollectionTests : BaseCollectionTests<string, TestLiteDbItem>
{
    public LiteDBCollectionTests() : base(new LiteDBTestContainer())
    {
    }
}