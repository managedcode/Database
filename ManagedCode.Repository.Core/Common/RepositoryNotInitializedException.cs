using System;

namespace ManagedCode.Repository.Core.Common
{
    public class RepositoryNotInitializedException : Exception
    {
        public RepositoryNotInitializedException(Type repositoryType) : base($"Repository {repositoryType.Name} is not Initialized")
        {
        }
    }
}