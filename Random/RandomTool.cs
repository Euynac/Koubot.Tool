using Koubot.Tool.Expand;
using Koubot.Tool.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koubot.Tool.Random
{
    /// <summary>
    /// 随机工具类
    /// </summary>
    public static class RandomTool
    {
        /// <summary>
        /// 随机数种子
        /// </summary>
        private static readonly System.Random _randomSeed = new();//只初始化一次比较好？

        #region 随机生成
        /// <summary>
        /// 获取强随机byte数组
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public static byte[] GetSecurityRandomByte(int byteCount = 1)
        {
            var count = byteCount.LimitInRange(1, 62);
            byte[] b = new byte[count];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            return b;
        }
        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，默认包含</param>
        ///<param name="useLow">是否包含小写字母</param>
        ///<param name="useUpp">是否包含大写字母</param>
        ///<param name="useSpecial">是否包含特殊字符</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length, bool useNum = true, bool useLow = false, bool useUpp = false, bool useSpecial = false, string custom = null)
        {
            StringBuilder s = new StringBuilder();
            string randomPool = custom ?? "";
            if (useNum) { randomPool += "0123456789"; }
            if (useLow) { randomPool += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp) { randomPool += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpecial) { randomPool += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            while (length-- > 0)
            {
                s.Append(randomPool.Substring(_randomSeed.Next(0, randomPool.Length), 1));
            }
            return s.ToString();
        }

        #endregion

        #region IList的类拓展
        /// <summary>
        /// 从数组中随机获取一个item，失败返回default(T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T RandomGetOne<T>(this T[] list)
        {
            if (list == null || !list.Any()) return default;
            return list[_randomSeed.Next(list.Length)];
        }

        /// <summary>
        /// 从<see cref="IList&lt;T&gt;"/>中随机获取一个item，失败返回default(T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T RandomGetOne<T>(this IList<T> list)
        {
            if (list == null || !list.Any()) return default;
            return list.Count == 1 ? list[0] : list[_randomSeed.Next(list.Count)];
        }
        /// <summary>
        /// 从<see cref="IList&lt;T&gt;"/>中随机获取规定数量的items（不会重复），返回list，当集合为null时返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count">实现已经自动限制count在1-list.Count之间了</param>
        /// <returns></returns>
        public static IList<T> RandomGetItems<T>(this IList<T> list, int count)
        {
            if (list == null || !list.Any()) return null;
            if (count <= 1) return new List<T> { RandomGetOne(list) };
            if (count > list.Count())
            {
                return RandomList(list);
            }
            return list.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        }
        /// <summary>
        /// 将<see cref="IList&lt;T&gt;"/>打乱顺序返回，失败则返回原来的list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IList<T> RandomList<T>(this IList<T> list)
        {
            if (list == null || !list.Any()) return list;
            return list.OrderBy(o => _randomSeed.Next(0, list.Count())).ToList();
        }
        #endregion

        #region Enum类拓展
        /// <summary>
        /// 从Enum中随机选取一个（需要Enum类是0-n连续的）
        /// </summary>
        /// <returns></returns>
        public static T EnumRandomGetOne<T>() where T : Enum
        {
            Type enumType = typeof(T);
            int length = Enum.GetNames(enumType).Length;
            return (T)Enum.Parse(enumType, _randomSeed.Next(length).ToString());
        }
        #endregion

        #region 数字类拓展

        /// <summary>
        /// 产生区间范围中的随机浮点数
        /// </summary>
        /// <param name="intervalDoublePair">区间</param>
        /// <returns></returns>
        public static double GenerateRandomDouble(this IntervalDoublePair intervalDoublePair)
        {
            var rightInterval = intervalDoublePair.RightInterval;
            var leftInterval = intervalDoublePair.LeftInterval;
            double maxValue = rightInterval.NumType == NumberType.Infinity
                ? double.MaxValue
                : rightInterval.Value;
            double minValue = leftInterval.NumType == NumberType.Infinitesimal ? double.MinValue : leftInterval.Value;
            if (rightInterval.NumType == NumberType.Infinity || leftInterval.NumType == NumberType.Infinitesimal)
            {
                var randomFactor = _randomSeed.NextDouble();
                return minValue + randomFactor * maxValue - randomFactor * minValue;//如不这样可能会溢出
            }

            return _randomSeed.NextDouble() * (maxValue - minValue) + minValue;
        }
        /// <summary>
        /// 产生区间范围中的随机浮点数
        /// </summary>
        /// <returns></returns>
        public static double GenerateRandomDouble(double minValue, double maxValue) => _randomSeed.NextDouble() * (maxValue - minValue) + minValue;

        /// <summary>
        /// 产生区间范围中的随机整数
        /// </summary>
        /// <param name="intervalDoublePair">区间</param>
        /// <returns></returns>
        public static int GenerateRandomInt(this IntervalDoublePair intervalDoublePair)
        {
            int maxValue = intervalDoublePair.GetRightIntervalNearestNumber();
            int minValue = intervalDoublePair.GetLeftIntervalNearestNumber();
            if (minValue > maxValue) return minValue;//区间相同就麻烦了
            if (maxValue == int.MaxValue) maxValue--;
            return _randomSeed.Next(minValue, maxValue + 1);
        }
        /// <summary>
        /// 产生区间范围中的随机整数
        /// </summary>
        /// <returns></returns>
        public static int GenerateRandomInt(int minValue, int maxValue)
        {
            if (minValue > maxValue) return minValue;
            if (maxValue == int.MaxValue) maxValue--;
            return _randomSeed.Next(minValue, maxValue + 1);
        }
        #endregion

        #region object类拓展

        /// <summary>
        /// 有x%可能性不返回null，就是有x%可能性会做（链式上?截断用于概率执行）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="probability">概率基础值，不返回null的可能性[0-1]</param>
        /// <param name="influenceValue">影响值，与原始基础概率相加，可为负</param>
        /// <param name="maxProbability">最大可能性，影响值+基础值的最大值</param>
        /// <param name="minProbability">最小可能性，影响值+基础值的最小值</param>
        /// <returns></returns>
        public static T ProbablyDo<T>(this T obj, double probability, double influenceValue = 0, double maxProbability = 1, double minProbability = 0) where T : class
            => ProbablyTrue(probability, influenceValue, maxProbability, minProbability) ? obj : null;

        /// <summary>
        /// 有x%可能性会成为给定的对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="become">x%可能会返回的对象</param>
        /// <param name="probability">概率基础值，不返回null的可能性[0-1]</param>
        /// <param name="influenceValue">影响值，与原始基础概率相加，可为负</param>
        /// <param name="maxProbability">最大可能性，影响值+基础值的最大值</param>
        /// <param name="minProbability">最小可能性，影响值+基础值的最小值</param>
        /// <returns></returns>
        public static T ProbablyBe<T>(this T obj, T become, double probability, double influenceValue = 0, double maxProbability = 1, double minProbability = 0)
            => ProbablyTrue(probability, influenceValue, maxProbability, minProbability) ? become : obj;

        /// <summary>
        /// 有x%可能性返回true
        /// </summary>
        /// <param name="probability">概率基础值，返回true的可能性[0-1]</param>
        /// <param name="influenceValue">影响值，与原始基础概率相加，可为负</param>
        /// <param name="maxProbability">0-1之间 最大可能性，影响值+基础值的最大值</param>
        /// <param name="minProbability">0-1之间 最小可能性，影响值+基础值的最小值</param>
        /// <returns></returns>
        public static bool ProbablyTrue(this double probability, double influenceValue = 0, double maxProbability = 1, double minProbability = 0)
            => (probability + influenceValue).LimitInRange(minProbability, maxProbability) - _randomSeed.NextDouble() > 0;
        #endregion
    }
}
