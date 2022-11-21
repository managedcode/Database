using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.InMemoryTests;

public class InMemoryDatabaseTests : BaseDatabaseTests<int, InMemoryItem>
{
    public InMemoryDatabaseTests() : base(new InMemoryTestContainer())
    {
    }
}