using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteDatabase : BaseDatabase<SQLiteConnection>
{
    private readonly SQLiteRepositoryOptions _options;

    public SQLiteDatabase(SQLiteRepositoryOptions options)
    {
        _options = options;
    }

    public override Task DeleteAsync(CancellationToken token = default)
    {
        DisposeInternal();
        File.Delete(NativeClient.DatabasePath);
        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        NativeClient.Close();
        NativeClient.Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        NativeClient.Close();
        NativeClient.Dispose();
    }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        NativeClient = _options.Connection ?? new SQLiteConnection(_options.ConnectionString);

        return Task.CompletedTask;
    }

    public SQLiteDatabaseCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : class, IItem<TId>, new()
    {
        if (!IsInitialized) throw new DatabaseNotInitializedException(GetType());

        NativeClient.CreateTable<TItem>();

        return new SQLiteDatabaseCollection<TId, TItem>(NativeClient);
    }
}