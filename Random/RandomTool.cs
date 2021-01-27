using Koubot.Tool.Expand;
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
        public static System.Random randomSeed = new System.Random();//只初始化一次比较好？

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
                s.Append(randomPool.Substring(randomSeed.Next(0, randomPool.Length), 1));
            }
            return s.ToString();
        }

        #endregion

        #region IList的类拓展
        /// <summary>
        /// 从<see cref="IList&lt;T&gt;"/>中随机获取一个item，失败返回default(T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T RandomGetOne<T>(this IList<T> list)
        {
            if (list == null || !list.Any()) return default(T);
            var items = RandomGetItems<T>(list, 1);
            if (items != null && items.Any()) return items.First();
            return default(T);
        }
        /// <summary>
        /// 从<see cref="IList&lt;T&gt;"/>中随机获取规定数量的items，返回list，失败返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IList<T> RandomGetItems<T>(this IList<T> list, int count)
        {
            if (list == null || !list.Any()) return null;
            if (count > list.Count())
            {
                return RandomList<T>(list);
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
            return list.OrderBy(o => randomSeed.Next(0, list.Count())).ToList();
        }
        #endregion

        #region Enum类拓展
        /// <summary>
        /// 从Enum中随机选取一个（需要Enum类是0-n连续的）
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static T EnumRandomGetOne<T>() where T : Enum
        {
            Type enumType = typeof(T);
            int length = Enum.GetNames(enumType).Length;
            return (T)Enum.Parse(enumType, randomSeed.Next(length).ToString());
        }
        #endregion
    }
}
