using System;

namespace ManagedCode.Database.Core.Common;

public class RepositoryNotInitializedException : Exception
{
    public RepositoryNotInitializedException(Type repositoryType) : base($"Repository {repositoryType.Name} is not Initialized")
    {
    }
}