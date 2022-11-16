using System.Linq.Expressions;
using Tenray.ZoneTree.Options;

namespace ManagedCode.Database.ZoneTree
{
    public class DbCollection<TValue> : IDisposable where TValue : ZoneTreeItem
    {
        private readonly ZoneTreeWrapper<Guid, TValue> _zoneTree;

        public DbCollection(string path)
        {
            _zoneTree = new ZoneTreeWrapper<Guid, TValue>(path);
            _zoneTree.Open(new ZoneTreeOptions<Guid, TValue?>()
            {
                Path = path,
                WALMode = WriteAheadLogMode.Sync,
                DiskSegmentMode = DiskSegmentMode.SingleDiskSegment,
                StorageType = StorageType.File,
                ValueSerializer = new JsonSerializer<TValue?>()
            });
        }

        public void Dispose()
        {
            _zoneTree.Dispose();
        }

        public bool Insert(TValue value)
        {
            return _zoneTree.Insert(value.Id, value);
        }

        public void Update(TValue value)
        {
            _zoneTree.Update(value.Id, value);
        }

        public void InsertOrUpdate(TValue value)
        {
            _zoneTree.InsertOrUpdate(value.Id, value);
        }

        public void InsertOrUpdate(IEnumerable<TValue> value)
        {
            foreach (var item in value)
            {
                _zoneTree.Upsert(item.Id, item);
            }
        }

        public void Upsert(TValue value)
        {
            _zoneTree.Upsert(value.Id, value);
        }

        public void Upsert(IEnumerable<TValue> value)
        {
            foreach (var item in value)
            {
                _zoneTree.Upsert(item.Id, item);
            }
        }

        public TValue? Get(Guid key)
        {
            return _zoneTree.Get(key);
        }

        public bool Contains(Guid key)
        {
            return _zoneTree.Contains(key);
        }

        public void Delete(Guid key)
        {
            _zoneTree.Delete(key);
        }

        public long Count()
        {
            return _zoneTree.Count();
        }

        public IEnumerable<TValue> Find(Expression<Func<TValue, bool>> expression)
        {
            var query = expression.Compile();
            foreach (var value in _zoneTree.Enumerate())
            {
                if (query(value))
                    yield return value;
            }
        }
    }
}