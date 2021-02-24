using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.Tests.Common
{
    public class InMemoryItem : IItem<int>
    {
        public string Data { get; set; }
        public int Id { get; set; }
    }
}