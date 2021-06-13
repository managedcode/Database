using System;
using ManagedCode.Repository.CosmosDB;
using ManagedCode.Repository.Tests.Common;

namespace ManagedCode.Repository.Tests
{
    public class CosmosDbRepositoryTests : BaseRepositoryTests<string, TestCosmosDbItem>
    {
        public const string ConnectionString =
            "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;";

        public CosmosDbRepositoryTests() : base(new CosmosDbRepository<TestCosmosDbItem>(
            new CosmosDbRepositoryOptions
            {
                ConnectionString = ConnectionString
            }))
        {
            Repository.InitializeAsync().Wait();
        }

        protected override string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}