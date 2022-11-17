using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using System.Threading.Tasks;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.InMemoryTests;

public class InMemoryQueryableTests : BaseQueryableTests<int, InMemoryItem>
{
    public InMemoryQueryableTests() : base(new InMemoryTestContainer())
    {
    }
}