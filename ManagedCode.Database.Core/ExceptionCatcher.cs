using System;
using System.Threading.Tasks;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.Core
{
    public static class ExceptionCatcher
    {
        public static async Task<T> ExecuteAsync<T>(Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                throw new DatabaseException(exception.Message, exception);
            }
        }

        public static async Task ExecuteAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                throw new DatabaseException(exception.Message, exception);
            }
        }

        public static void Execute(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception exception)
            {
                throw new DatabaseException(exception.Message, exception);
            }
        }

        public static T Execute<T>(Func<T> action)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception exception)
            {
                throw new DatabaseException(exception.Message, exception);
            }
        }
    }
}