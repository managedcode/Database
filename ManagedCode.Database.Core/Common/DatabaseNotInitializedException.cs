using System;

namespace ManagedCode.Database.Core.Common;

public class DatabaseNotInitializedException : Exception
{
    public DatabaseNotInitializedException(Type type) : base($"Database {type} is not Initialized")
    {
    }
}