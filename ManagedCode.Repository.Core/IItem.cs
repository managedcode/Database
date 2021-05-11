using System;

namespace ManagedCode.Repository.Core
{
    public interface IItem<TId>
    {
        TId Id { get; set; }
    }
    
    public interface IConcurrency<TKey> where TKey : IEquatable<TKey>
    {
        string ConcurrencyStamp { get; set; }
    }
}