namespace ManagedCode.Database.EntityFramework.PostgreSQL.Models;

public class PostgresConnectionOptions
{
    public string ConnectionString { get; set; }
    public bool UseTracking { get; set; }
}