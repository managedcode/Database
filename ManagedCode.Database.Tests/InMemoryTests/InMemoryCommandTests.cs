using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using System.Threading.Tasks;

namespace ManagedCode.Database.Tests.InMemoryTests
{
    public class InMemoryCommandTests : BaseCollectionTests<int, InMemoryItem>
    {
        private static volatile int _count;
        private InMemoryDatabase _databaseb;

        public InMemoryCommandTests()
        {
            _databaseb = new InMemoryDatabase();
        }

        protected override IDatabaseCollection<int, InMemoryItem> Collection 
            => _databaseb.GetCollection<int, InMemoryItem>();

        protected override int GenerateId()
        {
            _count++;
            return _count;
        }

        public override async Task InitializeAsync() =>
            await _databaseb.InitializeAsync();

        public override async Task DisposeAsync() =>
            await _databaseb.DisposeAsync();
    }
}
