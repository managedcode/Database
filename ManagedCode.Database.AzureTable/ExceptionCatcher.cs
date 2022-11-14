using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.AzureTable;

public static class ExceptionCatcher
{
    public static async Task<T> ExecuteAsync<T>(Task<T> task)
    {
        try
        {
            return await task;
        }
        catch (Exception e) when (e is RequestFailedException exception)
        {
            if (exception.ErrorCode == "EntityAlreadyExists")
            {
                throw new DatabaseException(exception.Message, exception,
                    ErrorCode.EntityAlreadyExist);
            }

            throw new DatabaseException(exception.Message, exception,
                ErrorCode.Unknown);
        }
        catch (Exception exception)
        {
            throw new DatabaseException(exception.Message, exception, ErrorCode.Unknown);
        }
    }
}