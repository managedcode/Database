# Repository

[![.NET](https://github.com/managed-code-hub/Repository/actions/workflows/dotnet.yml/badge.svg)](https://github.com/managed-code-hub/Repository/actions/workflows/dotnet.yml) [![nuget](https://github.com/managed-code-hub/Repository/actions/workflows/nuget.yml/badge.svg?branch=main)](https://github.com/managed-code-hub/Repository/actions/workflows/nuget.yml)

---

## Repository pattern implementation for C#.
A universal repository for working with multiple databases.
We currently support:

- InMemory 
- Azure Tables
- CosmosDB
- LiteDB

## General concept

The main interface of the repository is defined as: 
```cs 
interface IRepository<in TId, TItem> where TItem : IRepositoryItem<TId>
```
This is to define the id type, and the object itself.
