using Koubot.Tool.Expand;
using System;
using System.Diagnostics;

namespace Koubot.Tool.General
{
    /// <summary>
    /// Kou计时表类，方便测试效率
    /// </summary>
    public class KouWatch
    {
        /// <summary>
        /// 开始计时，并自动结束，输出执行时间
        /// </summary>
        /// <param name="action"></param>
        /// <param name="actionName">动作名字，区分执行时间</param>
        public static void Start(string actionName, Action action)
        {
            Console.WriteLine(actionName + "执行中...");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            action.Invoke();
            watch.Stop();
            Console.WriteLine("执行时间：" + watch.ElapsedMilliseconds);
        }

        /// <summary>
        /// 两个动作测试优劣。自动开始计时、结束，输出执行时间、两者比较的结果
        /// </summary>
        /// <param name="action1Name"></param>
        /// <param name="action1"></param>
        /// <param name="action2Name"></param>
        /// <param name="action2"></param>
        /// <param name="testTimes">执行次数</param>
        public static void Start(string action1Name, Action action1, string action2Name, Action action2, int testTimes = 1)
        {
            Stopwatch watch = new Stopwatch();
            Console.WriteLine($"“{action1Name}”动作执行{testTimes}次中...");
            watch.Start();
            for (int i = 0; i < testTimes; i++)
            {
                action1.Invoke();
            }
            watch.Stop();
            Console.WriteLine($"{action1Name}执行时间：{watch.ElapsedMilliseconds}ms");
            Console.WriteLine($"“{action2Name}”动作执行{testTimes}中...");
            Stopwatch watch2 = new Stopwatch();
            watch2.Start();
            for (int i = 0; i < testTimes; i++)
            {
                action2.Invoke();
            }
            watch2.Stop();
            Console.WriteLine($"{action2Name}执行时间：{watch2.ElapsedMilliseconds}ms");

            long watch2Time = watch2.ElapsedMilliseconds.LimitInRange(1, null);
            long watch1Time = watch.ElapsedMilliseconds.LimitInRange(1, null);
            Console.WriteLine(watch2Time > watch1Time
                ? $"效率比较：“{action1Name}”动作比{action2Name}快{(watch2Time / (double)watch1Time - 1):P3}倍"
                : $"效率比较：“{action2Name}”动作比{action1Name}快{(watch1Time / (double)watch2Time - 1):P3}倍");
        }
    }
}