/*using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.AzureTable;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace ManagedCode.Database.Tests
{
    public class AzureTableRepositoryTests : BaseRepositoryTests<TableId, TestAzureTableItem>
    {
        public const string ConnectionString =
            "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;QueueEndpoint=http://localhost:10001/devstoreaccount1;TableEndpoint=http://localhost:10002/devstoreaccount1;";

        private AzureTableDatabase _databaseb;
        public AzureTableRepositoryTests()
        {
            _databaseb = new AzureTableDatabase(new AzureTableRepositoryOptions
            {
                ConnectionString = ConnectionString
            });

        }

        protected override IDBCollection<TableId, TestAzureTableItem> Collection => _databaseb.GetCollection<TableId, TestAzureTableItem>();

        protected override TableId GenerateId()
        {
            return new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        [Fact]
        public override async Task InsertOneItem()
        {
            var id = GenerateId();
            var firstItem = CreateNewItem(id);
            var secondItem = CreateNewItem(id);

            //  SEPARATE TESTS IN TWO
            var insertFirst = await Collection.InsertAsync(firstItem);
            var insertSecond = async () => await Collection.InsertAsync(secondItem);

            insertFirst.Should().NotBeNull();
            await insertSecond.Should().ThrowAsync<StorageException>();
        }

        public override void Dispose()
        {
            _databaseb.Dispose();
        }
    }
}

*/