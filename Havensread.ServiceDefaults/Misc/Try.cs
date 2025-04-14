using Havensread.IngestionService.Workers.Book;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havensread.ServiceDefaults.Misc;

public static class Try
{
    public static async Task<TryResult<T>> Execute<T>(Func<Task<T>> func, ILogger? logger = null)
    {
        try
        {
            var result = await func();
            return new() { Value = result };
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while executing function {Name}.", func.Method.Name);
            return new() { Error = true };
        }
    }

    public static async Task<TryResult<T>> Execute<T>(Func<Task<T>> func, Action<Exception> log)
    {
        try
        {
            var result = await func();
            return new() { Value = result };
        }
        catch (Exception ex)
        {
            log(ex);
            return new() { Error = true };
        }
    }

    public static TryResult<T> Execute<T>(Func<T> func, Action<Exception> log)
    {
        try
        {
            var result = func();
            return new() { Value = result };
        }
        catch (Exception ex)
        {
            log(ex);
            return new() { Error = true };
        }
    }
}
