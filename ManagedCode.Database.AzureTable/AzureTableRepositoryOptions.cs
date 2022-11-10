using System;
using Azure.Data.Tables;

namespace ManagedCode.Database.AzureTable;

public class AzureTableRepositoryOptions
{
    public string ConnectionString { get; set; }
    public TableSharedKeyCredential TableSharedKeyCredential { get; set; }
    public Uri TableStorageUri { get; set; }
    public bool AllowTableCreation { get; set; }
}