using System;

namespace ManagedCode.Database.Core.Exceptions;

public class DatabaseException : Exception
{
    public ErrorCode ErrorCode { get; }

    public DatabaseException()
    {
    }

    public DatabaseException(string message, ErrorCode errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DatabaseException(string message, Exception inner, ErrorCode errorCode)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}

public enum ErrorCode
{
    Unknown,
    EntityAlreadyExist,
    ItemNotFound,
}