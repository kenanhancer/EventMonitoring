using System;
using System.Threading.Tasks;

namespace MonitoringLib
{
    public static class AsyncExtensions
    {
        public static async Task ThrowsAsync(this Task task, Action<Exception> exceptionCallback = null)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                if (exceptionCallback != null)
                {
                    exceptionCallback(ex);

                    if (ex is AggregateException)
                    {
                        AggregateException aggExc = (AggregateException)ex;

                        foreach (var item in aggExc.InnerExceptions)
                        {
                            exceptionCallback(item);
                        }
                    }
                }
            }
        }
        public static async Task<TResult> ThrowsAsync<TResult>(this Task<TResult> task, Action<Exception> exceptionCallback = null)
        {
            try
            {
                await task;

                return task.Result;
            }
            catch (Exception ex)
            {
                if (exceptionCallback != null)
                {
                    exceptionCallback(ex);

                    if (ex is AggregateException)
                    {
                        AggregateException aggExc = (AggregateException)ex;

                        foreach (var item in aggExc.InnerExceptions)
                        {
                            exceptionCallback(item);
                        }
                    }
                }
            }

            return default(TResult);
        }
    }
}