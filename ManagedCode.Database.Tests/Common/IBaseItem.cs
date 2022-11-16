using System;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.Tests.Common
{
    public interface IBaseItem<TId> : IItem<TId>
    {
        string StringData { get; set; }
        int IntData { get; set; }
        long LongData { get; set; }
        float FloatData { get; set; }
        double DoubleData { get; set; }
        DateTime DateTimeData { get; set; }
    }
}