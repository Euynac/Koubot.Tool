using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Koubot.Tool.General
{
    /// <summary>
    /// Kou定时、延时器
    /// </summary>
    public static class KouTaskDelayer
    {
        private static readonly SortedList<DateTime, Task> _sleepTaskList = new(new DuplicateKeyComparer<DateTime>());
        private static readonly object _listLock = new();

        /// <summary>
        /// 单次线程等待时间（ms）
        /// </summary>
        private const int _sleepTime = 10;

        static KouTaskDelayer()
        {
            StartTick();
        }
        /// <summary>
        /// 开启定时器
        /// </summary>
        private static void StartTick()
        {
            Task.Factory.StartNew(() =>
            {
                bool canStartTask = false;
                while (true)
                {
                    if (_sleepTaskList.Count <= 0) Thread.Sleep(1000);
                    KeyValuePair<DateTime, Task> pair;
                    lock (_listLock)
                    {
                        if (_sleepTaskList.Count <= 0) continue;
                        pair = _sleepTaskList.ElementAt(0);
                        if (pair.Key <= DateTime.Now)
                        {
                            _sleepTaskList.RemoveAt(0);
                            canStartTask = true;
                        }
                    }
                    if (canStartTask)//任务在当前线程完成
                    {
                        pair.Value.Start();
                        canStartTask = false;
                    }
                    Thread.Sleep(_sleepTime);
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 向定时池增加需要执行的任务
        /// </summary>
        /// <param name="executeTime"></param>
        /// <param name="task">要执行的任务</param>
        public static void AddTask(DateTime executeTime, Task task)
        {
            lock (_listLock)
            {
                _sleepTaskList.Add(executeTime, task);//允许重复的执行时间，但注意Remove之类的方法失效
            }
        }

    }

    /// <summary>
    /// Comparer for comparing two keys, handling equality as beeing greater
    /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal class DuplicateKeyComparer<TKey>
        :
            IComparer<TKey> where TKey : IComparable
    {
        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1; // Handle equality as being greater. Note: this will break Remove(key) or
            else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                return result;
        }

        #endregion
    }
}
