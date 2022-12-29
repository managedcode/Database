using Amazon.DynamoDBv2.DataModel;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.DynamoDB
{
    public class DynamoDBItem<TId> : IItem<TId>
    {
        [DynamoDBHashKey]
        public TId Id { get; set; }
    }
}