using System;
using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.Tests.Common
{

    public abstract class BaseItem<TId> : IItem<TId>
    {
        public TId Id { get; set; }
        
        public string StringData { get; set; }
        public int IntData { get; set; }
        public float FloatData { get; set; }
        public DateTime DateTimeData { get; set; }
    }
    
    public class InMemoryItem : BaseItem<int>
    {
    }
}