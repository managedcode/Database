namespace ManagedCode.Repository.EntityFramework.PostgreSQL.Models;

public class PostgresConnectionOptions
{
    public string ConnectionString { get; set; }
    public bool UseTracking { get; set; }
}