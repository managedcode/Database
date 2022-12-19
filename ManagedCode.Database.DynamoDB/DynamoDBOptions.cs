using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedCode.Database.DynamoDB;

public class DynamoDBOptions
{
    public string ConnectionString { get; set; }
    public string DataBaseName { get; set; }
    //public bool AllowTableCreation { get; set; }
}
