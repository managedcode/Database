using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.InMemoryTests
{
    public class InMemoryMultiThreadingTests : BaseMultiThreadingTests<int, InMemoryItem>
    {
        // Sometimes it throws exception that entity is already exist,
        //      seems like race condition
        public InMemoryMultiThreadingTests() : base(new InMemoryTestContainer())
        {
        }
    }
}
