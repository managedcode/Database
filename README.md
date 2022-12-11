![img|300x200](https://raw.githubusercontent.com/managed-code-hub/Database/main/logo.png)
# Database
[![.NET](https://github.com/managed-code-hub/Database/actions/workflows/dotnet.yml/badge.svg)](https://github.com/managed-code-hub/Database/actions/workflows/dotnet.yml)
[![Coverage Status](https://coveralls.io/repos/github/managed-code-hub/Database/badge.svg?branch=main)](https://coveralls.io/github/managed-code-hub/Database?branch=main)
[![nuget](https://github.com/managed-code-hub/Database/actions/workflows/nuget.yml/badge.svg?branch=main)](https://github.com/managed-code-hub/Database/actions/workflows/nuget.yml)
[![CodeQL](https://github.com/managed-code-hub/Database/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/managed-code-hub/Database/actions/workflows/codeql-analysis.yml)

| Version | Package | Description |
| ------- | ------- | ----------- |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Database.Core.svg)](https://www.nuget.org/packages/ManagedCode.Database.Core) | [ManagedCode.Database.Core](https://www.nuget.org/packages/ManagedCode.Database.Core) | Core |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Database.AzureTable.svg)](https://www.nuget.org/packages/ManagedCode.Database.AzureTable) | [ManagedCode.Database.AzureTable](https://www.nuget.org/packages/ManagedCode.Database.AzureTable) | AzureTable |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Database.CosmosDB.svg)](https://www.nuget.org/packages/ManagedCode.Database.CosmosDB) | [ManagedCode.Database.CosmosDB](https://www.nuget.org/packages/ManagedCode.Database.CosmosDB) | CosmosDB |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Database.LiteDB.svg)](https://www.nuget.org/packages/ManagedCode.Database.LiteDB) | [ManagedCode.Database.LiteDB](https://www.nuget.org/packages/ManagedCode.Database.LiteDB) | LiteDB |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Database.MongoDB.svg)](https://www.nuget.org/packages/ManagedCode.Database.MongoDB) | [ManagedCode.Database.MongoDB](https://www.nuget.org/packages/ManagedCode.Database.MongoDB) | MongoDB |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Database.SQLite.svg)](https://www.nuget.org/packages/ManagedCode.Database.SQLite) | [ManagedCode.Database.SQLite](https://www.nuget.org/packages/ManagedCode.Database.SQLite) | SQLite |





---
# OUTDATED--
## Repository pattern implementation for C#.
A universal repository for working with multiple databases:
- InMemory 
- Azure Tables
- CosmosDB
- LiteDB
- SQLite
- MSSQL
- PostgreSQL

## General concept

We don't think you can hide the real database completely behind abstractions, so I recommend using your interfaces that will lend IReposity of the right type.
And do the same with the direct implementation.

```cs
// declare the model as a descendant of the base type.
public class SomeModel : IItem<T>
{

}
```

```cs 
// then create an interface
public interface ISomeRepository : IRepository<TId, TItem> where TItem : IItem<TId>
{
}
```

```cs 
// create a class inherited from a repository of the desired type
public class SomeRepository : BaseRepository<TId, TItem> where TItem : class, IItem<TId>, new()
{
}
```

And then add your interface and dependency configuration class.
``` cs
services
        .AddTransient<ISomeRepository, SomeRepository>()
```
This is to define the id type, and the object itself.

---
## Azure Table
```cs
// declare the model as a descendant of the base type.
public class SomeModel : AzureTableItem
{

}


// then create an interface
public interface ISomeRepository : IAzureTableRepository<SomeModel>
{
}


// create a class inherited from a repository of the desired type
public class SomeRepository : AzureTableRepository<SomeModel>, ISomeRepository
{
    public SomeRepository(ILogger<SomeRepository> logger, IConfiguration config) : base(logger, 
        new AzureTableRepositoryOptions
        {
            ConnectionString = "connectionString"
        })
    {
    }
}
```
---
## CosmosDB
```cs
// declare the model as a descendant of the base type.
public class SomeModel : CosmosDbItem
{

}


// then create an interface
public interface ISomeRepository : ICosmosDbRepository<SomeModel>
{
}


// create a class inherited from a repository of the desired type
public class SomeRepository : CosmosDbRepository<SomeModel>, ISomeRepository
{
    public SomeRepository(ILogger<SomeRepository> logger, IConfiguration config) : base(logger, 
        new CosmosDbRepositoryOptions
        {
            ConnectionString = "connectionString"
        })
    {
    }
}
```
---
## LiteDB
```cs
// declare the model as a descendant of the base type.
public class SomeModel : LiteDbItem<string>
{

}


// then create an interface
public interface ISomeRepository : ILiteDbRepository<string, SomeModel>
{
}


// create a class inherited from a repository of the desired type
public class SomeRepository : LiteDbRepository<SomeModel>, ISomeRepository
{
    public SomeRepository(ILogger<SomeRepository> logger, IConfiguration config) : base(logger, 
        new LiteDbRepositoryOptions
        {
            ConnectionString = "connectionString"
        })
    {
    }
}
```
---
## MongoDB
```cs
// declare the model as a descendant of the base type.
public class SomeModel : MongoDbItem<string>
{

}


// then create an interface
public interface ISomeRepository : IMongoDbRepository<SomeModel>
{
}


// create a class inherited from a repository of the desired type
public class SomeRepository : MongoDbRepository<SomeModel>, ISomeRepository
{
    public SomeRepository(ILogger<SomeRepository> logger, IConfiguration config) : base(logger, 
        new LiteDbRepositoryOptions
        {
            ConnectionString = "connectionString"
        })
    {
    }
}
```
---
## SQLite
```cs
// declare the model as a descendant of the base type.
public class SomeModel : SQLiteItem
{

}


// then create an interface
public interface ISomeRepository : ISQLiteRepository<string, SomeModel>
{
}


// create a class inherited from a repository of the desired type
public class SomeRepository : SQLiteRepository<SomeModel>, ISomeRepository
{
    public SomeRepository(ILogger<SomeRepository> logger, IConfiguration config) : base(logger, 
        new SQLiteRepositoryOptions
        {
            ConnectionString = "connectionString"
        })
    {
    }
}
```
