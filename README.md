
<img alt="managed code repository" src="https://github.com/managed-code-hub/Repository/raw/main/logo.png" width="300px" />

# Repository
[![.NET](https://github.com/managed-code-hub/Repository/actions/workflows/dotnet.yml/badge.svg)](https://github.com/managed-code-hub/Repository/actions/workflows/dotnet.yml) 
[![nuget](https://github.com/managed-code-hub/Repository/actions/workflows/nuget.yml/badge.svg?branch=main)](https://github.com/managed-code-hub/Repository/actions/workflows/nuget.yml)
[![CodeQL](https://github.com/managed-code-hub/Repository/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/managed-code-hub/Repository/actions/workflows/codeql-analysis.yml)


| Version | Package | Description |
| ------- | ------- | ----------- |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Repository.Core.svg)](https://www.nuget.org/packages/ManagedCode.Repository.Core) | [ManagedCode.Repository.Core](https://www.nuget.org/packages/ManagedCode.Repository.Core) | Core |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Repository.AzureTable.svg)](https://www.nuget.org/packages/ManagedCode.Repository.AzureTable) | [ManagedCode.Repository.AzureTable](https://www.nuget.org/packages/ManagedCode.Repository.AzureTable) | AzureTable |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Repository.CosmosDB.svg)](https://www.nuget.org/packages/ManagedCode.Repository.CosmosDB) | [ManagedCode.Repository.CosmosDB](https://www.nuget.org/packages/ManagedCode.Repository.CosmosDB) | CosmosDB |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Repository.LiteDB.svg)](https://www.nuget.org/packages/ManagedCode.Repository.LiteDB) | [ManagedCode.Repository.LiteDB](https://www.nuget.org/packages/ManagedCode.Repository.LiteDB) | LiteDB |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Repository.SQLite.svg)](https://www.nuget.org/packages/ManagedCode.Repository.MongoDB) | [ManagedCode.Repository.MongoDB](https://www.nuget.org/packages/ManagedCode.Repository.MongoDB) | MongoDB |
|[![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Repository.SQLite.svg)](https://www.nuget.org/packages/ManagedCode.Repository.SQLite) | [ManagedCode.Repository.SQLite](https://www.nuget.org/packages/ManagedCode.Repository.SQLite) | SQLite |




# Repository



---

## Repository pattern implementation for C#.
A universal repository for working with multiple databases:
- InMemory 
- Azure Tables
- CosmosDB
- LiteDB
- SQLite

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
