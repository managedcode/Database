using System;
using Azure.Data.Tables;

namespace ManagedCode.Database.AzureTables;

public class AzureTablesOptions
{
    public string ConnectionString { get; set; }
    public TableSharedKeyCredential TableSharedKeyCredential { get; set; }
    public Uri TableStorageUri { get; set; }
    public bool AllowTableCreation { get; set; }
    public string TableName { get; set; }

}