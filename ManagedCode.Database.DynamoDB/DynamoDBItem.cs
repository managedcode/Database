using Amazon.DynamoDBv2.DocumentModel;
using ManagedCode.Database.Core;
using System.Security.Cryptography;

namespace ManagedCode.Database.DynamoDB
{
    public class DynamoDBItem : IItem<Primitive>
    {
        private Primitive _id;


        public DynamoDBItem()
        {
            _id = new Primitive();
        }

        public Primitive Id 
        { 
            get => _id;
            set 
            {
                _id = value;
            } 
        }
    }
}