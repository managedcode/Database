using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ManagedCode.Database.Core;
using System.Security.Cryptography;

namespace ManagedCode.Database.DynamoDB
{
    public class DynamoDBItem<TId> : IItem<TId>
    {
        [DynamoDBHashKey]
        public TId Id { get; set; }
    }
}