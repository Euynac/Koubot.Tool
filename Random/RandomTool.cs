using Koubot.Tool.Extensions;
using Koubot.Tool.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Koubot.Tool.Random
{
    /// <summary>
    /// Random Tool (Thread-safe)
    /// </summary>
    public static class RandomTool
    {
        private static int _seed = Environment.TickCount;
        private static readonly ThreadLocal<System.Random> _randomSeed =
            new(() => new System.Random(Interlocked.Increment(ref _seed)));

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
                s.Append(randomPool.Substring(_randomSeed.Value.Next(0, randomPool.Length), 1));
            }
            return s.ToString();
        }

        #endregion

        #region IList的类拓展

        /// <summary>
        /// Get a random item from the array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="hashString">if use hash, then the result depends on it</param>
        /// <returns>return default(T) if fails</returns>
        public static T? RandomGetOne<T>(this T[]? list, string? hashString = null)
        {
            if (list == null || !list.Any()) return default;
            return hashString != null ?
                list[GenerateRandomInt(0, list.Length - 1, hashString)]
                : list[_randomSeed.Value.Next(list.Length)];
        }

        /// <summary>
        /// Get a random item from the <see cref="IList&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="hashString">if use hash, then the result depends on it</param>
        /// <returns>return default(T) if fails</returns>
        public static T? RandomGetOne<T>(this IList<T>? list, string? hashString = null)
        {
            if (list == null || !list.Any()) return default;
            return hashString != null ? list[GenerateRandomInt(0, list.Count - 1, hashString)] :
                list[_randomSeed.Value.Next(list.Count)];
        }

        /// <summary>
        /// Get a number of random items from <see cref="IList&lt;T&gt;"/> (no duplicates).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count">Count is already automatically limited to 1-list.Count</param>
        /// <param name="hashString">if use hash, then the result depends on it</param>
        /// <returns>return null when the set is null</returns>
        public static IList<T>? RandomGet<T>(this IList<T>? list, int count, string? hashString = null)
        {
            if (list == null || !list.Any()) return null;
            if (count == 1) return new List<T> { RandomGetOne(list)! };
            if (count > list.Count())
            {
                return RandomList(list, hashString);
            }
            return hashString != null ? list.OrderBy(_ => hashString.GetHashCode()).Take(count).ToList() :
                list.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        }

        /// <summary>
        /// Return <see cref="IList&lt;T&gt;"/> in disordered order, or return the original data if it fails
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="hashString">if use hash, then the result depends on it</param>
        /// <returns></returns>
        public static IList<T>? RandomList<T>(this IList<T>? list, string? hashString = null)
        {
            if (list == null || !list.Any()) return list;
            return hashString != null ? list.OrderBy(o => hashString.GetHashCode()).ToList() :
                list.OrderBy(o => Guid.NewGuid()).ToList();
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
            return (T)Enum.Parse(enumType, _randomSeed.Value.Next(length).ToString());
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
                var randomFactor = _randomSeed.Value.NextDouble();
                return minValue + randomFactor * maxValue - randomFactor * minValue;//如不这样可能会溢出
            }

            return _randomSeed.Value.NextDouble() * (maxValue - minValue) + minValue;
        }
        /// <summary>
        /// Generate a random double between minValue and maxValue (include min but not max value)
        /// </summary>
        /// <returns></returns>
        public static double GenerateRandomDouble(double minValue, double maxValue) => _randomSeed.Value.NextDouble() * (maxValue - minValue) + minValue;
        /// <summary>
        /// Generate a random double between minValue and maxValue base on hash code (include min but not max value)
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="objectToHash"></param>
        /// <returns></returns>
        public static double GenerateRandomDouble(double minValue, double maxValue, object objectToHash) =>
            ((objectToHash.GetHashCode() + System.Math.Abs((long)int.MinValue)) /
             (double)(int.MaxValue + System.Math.Abs((long)int.MinValue))) * (maxValue - minValue) + minValue;
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
            return _randomSeed.Value.Next(minValue, maxValue + 1);
        }
        /// <summary>
        /// Generate a random integer between minValue and maxValue (include min and max value)
        /// </summary>
        /// <returns></returns>
        public static int GenerateRandomInt(int minValue, int maxValue)
        {
            if (minValue >= maxValue) return minValue;
            if (maxValue == int.MaxValue) maxValue--;
            return _randomSeed.Value.Next(minValue, maxValue + 1);
        }
        /// <summary>
        /// Generate a random integer between minValue and maxValue base on hash code (include min and max value)
        /// </summary>
        /// <returns></returns>
        public static int GenerateRandomInt(int minValue, int maxValue, object objectToHash)
        {
            if (minValue >= maxValue) return minValue;
            long modular = maxValue - minValue + 1;
            var hashCode = System.Math.Abs(objectToHash.GetHashCode());
            var rest = (int)(hashCode % modular);
            return minValue + rest;
        }
        #endregion

        #region object类拓展

        /// <summary>
        /// 有x%可能性返回null（用于 ?? 来随机选择，并较其他效率较高）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="probability">概率基础值，不返回null的可能性[0-1]</param>
        /// <param name="influenceValue">影响值，与原始基础概率相加，可为负</param>
        /// <param name="maxProbability">最大可能性，影响值+基础值的最大值</param>
        /// <param name="minProbability">最小可能性，影响值+基础值的最小值</param>
        /// <returns></returns>
        public static T? ProbablyNull<T>(this T obj, double probability, double influenceValue = 0, double maxProbability = 1, double minProbability = 0) where T : class
            => ProbablyTrue(probability, influenceValue, maxProbability, minProbability) ? null : obj;

        /// <summary>
        /// 有x%可能性不返回null，就是有x%可能性会做（链式上?截断用于概率执行）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="probability">概率基础值，不返回null的可能性[0-1]</param>
        /// <param name="influenceValue">影响值，与原始基础概率相加，可为负</param>
        /// <param name="maxProbability">最大可能性，影响值+基础值的最大值</param>
        /// <param name="minProbability">最小可能性，影响值+基础值的最小值</param>
        /// <returns></returns>
        public static T? ProbablyDo<T>(this T obj, double probability, double influenceValue = 0, double maxProbability = 1, double minProbability = 0) where T : class
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
        /// <returns>没有成为就返回原先的对象</returns>
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
            => (probability + influenceValue).LimitInRange(minProbability, maxProbability) - _randomSeed.Value.NextDouble() > 0;

        /// <summary>
        /// 结果基于给定的哈希，有x%可能性返回true
        /// </summary>
        /// <param name="probability">概率基础值，返回true的可能性[0-1]</param>
        /// <param name="objectToHash"></param>
        /// <param name="influenceValue">影响值，与原始基础概率相加，可为负</param>
        /// <param name="maxProbability">0-1之间 最大可能性，影响值+基础值的最大值</param>
        /// <param name="minProbability">0-1之间 最小可能性，影响值+基础值的最小值</param>
        /// <returns></returns>
        public static bool ProbablyTrue(this double probability, object objectToHash, double influenceValue = 0, double maxProbability = 1,
            double minProbability = 0) =>
            (probability + influenceValue).LimitInRange(minProbability, maxProbability) - GenerateRandomDouble(0,1,objectToHash) > 0;

        #endregion

        public static double DistributeUsePowerFunction(int value, double distributeMaxValue, double power, int maxRange = 100)
        {
            var factor = System.Math.Pow(distributeMaxValue, 1 / power) / maxRange;
            return System.Math.Pow(factor * value, power);
        }


    }
}
