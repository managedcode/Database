using System;

namespace ManagedCode.Database.Core.Exceptions
{
    public class DatabaseException : Exception
    {
        public DatabaseException()
        {
        }

        public DatabaseException(string message)
            : base(message)
        {
        }

        public DatabaseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}