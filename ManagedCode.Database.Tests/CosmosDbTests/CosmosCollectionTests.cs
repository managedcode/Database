using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.Cosmos;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.Tests.CosmosDbTests;

public class CosmosCollectionTests : BaseCollectionTests<string, TestCosmosItem>
{
    public CosmosCollectionTests() : base(new CosmosTestContainer())
    {
    }
}