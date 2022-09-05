using System.Linq;
using ManagedCode.Database.Core;
using Xunit;

namespace ManagedCode.Database.Tests;

public class QueryTests
{
    [Fact]
    public void QueryTest()
    {
        

        var q = new Query<TestItem>();

        var query = q.Where(w => w.Prop2 > 3).Where(s => s.Prop1.Contains("hi"));
        var text = query.ToString();
        var x = query.ToList();
    }
}