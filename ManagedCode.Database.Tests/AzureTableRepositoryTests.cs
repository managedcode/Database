using System;
using ManagedCode.Database.AzureTable;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests
{
    /*
    public class AzureTableRepositoryTests : BaseRepositoryTests<TableId, TestAzureTableItem>
    {
        public const string ConnectionString =
            "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;QueueEndpoint=http://localhost:10001/devstoreaccount1;TableEndpoint=http://localhost:10002/devstoreaccount1;";

        public AzureTableRepositoryTests() : base(new AzureTableRepository<TestAzureTableItem>(
            new AzureTableRepositoryOptions
            {
                ConnectionString = ConnectionString
            }))
        {
            Repository.InitializeAsync().Wait();
        }

        protected override TableId GenerateId()
        {
            return new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }
    }*/
}

