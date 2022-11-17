using System.Threading;
using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using System.Threading.Tasks;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.InMemoryTests;

public class InMemoryCollectionTests : BaseCollectionTests<int, InMemoryItem>
{
    public InMemoryCollectionTests() : base(new InMemoryTestContainer())
    {
    }
}