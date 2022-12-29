using System;

namespace ManagedCode.Database.DynamoDB;

public class DynamoDBOptions
{
    public string ServiceURL { get; set; }
    public string AuthenticationRegion { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string DataBaseName { get; set; }
    public string CollectionName { get; set; }
}
