using System;
using System.Threading;
using System.Threading.Tasks;

namespace Koubot.Tool.Extensions;

public static class ThreadingExtensions
{
    /// <summary>
    /// Sync over async method, use it carefully.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <returns></returns>
    public static T GetResult<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Doing read action in a read lock.
    /// </summary>
    /// <param name="rwLock"></param>
    /// <param name="action"></param>
    public static T DoReadAction<T>(this ReaderWriterLockSlim rwLock, Func<T> action)
    {
        rwLock.EnterReadLock();
        try
        {
            return action();
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }
    /// <summary>
    /// Doing read action in a read lock.
    /// </summary>
    /// <param name="rwLock"></param>
    /// <param name="action"></param>
    public static void DoReadAction(this ReaderWriterLockSlim rwLock, Action action)
    {
        rwLock.EnterReadLock();
        try
        {
            action();
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Doing write action in a write lock.
    /// </summary>
    /// <param name="rwLock"></param>
    /// <param name="action"></param>
    public static void DoWriteAction(this ReaderWriterLockSlim rwLock, Action action)
    {
        rwLock.EnterWriteLock();
        try
        {
            action();
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Doing write action in a write lock.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rwLock"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static T DoWriteAction<T>(this ReaderWriterLockSlim rwLock, Func<T> action)
    {
        rwLock.EnterWriteLock();
        try
        {
            return action();
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }
}