using Amazon.DynamoDBv2;
using ManagedCode.Database.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.DynamoDB
{
    internal class DynamoDBDatabase : BaseDatabase<AmazonDynamoDBClient>
    {
        private readonly DynamoDBOptions _dbOptions;

        public DynamoDBDatabase(DynamoDBOptions dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public override Task DeleteAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask DisposeAsyncInternal()
        {
            throw new NotImplementedException();
        }

        protected override void DisposeInternal()
        {
            throw new NotImplementedException();
        }

        protected override Task InitializeAsyncInternal(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
