using System;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using SQLite;

namespace ManagedCode.Database.SQLite
{
    public class SqLiteDatabase : BaseDatabase<SQLiteConnection>
    {
        private readonly SQLiteRepositoryOptions _options;

        public SqLiteDatabase(SQLiteRepositoryOptions options)
        {
            _options = options;
        }

        public override Task DeleteAsync(CancellationToken token = default)
        {
            DisposeInternal();
            System.IO.File.Delete(NativeClient.DatabasePath);
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

        public SqLiteDatabaseCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : class, IItem<TId>, new()
        {
            if (!IsInitialized)
            {
                throw new DatabaseNotInitializedException(GetType());
            }

            NativeClient.CreateTable<TItem>();

            return new SqLiteDatabaseCollection<TId, TItem>(NativeClient);
        }
    }
}