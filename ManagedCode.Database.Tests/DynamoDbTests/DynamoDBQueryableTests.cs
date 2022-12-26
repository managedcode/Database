using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ManagedCode.Database.Tests.DynamoDbTests;

public class DynamoDBQueryableTests : BaseQueryableTests<string, TestDynamoDbItem>
{
    public DynamoDBQueryableTests() : base(new DynamoDBTestContainer())
    {
    }
}