using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace Koubot.Tool.Extensions
{
    /// <summary>
    /// Extension methods of IDictionary type.
    /// </summary>
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// 尝试将键和值添加到字典中，如果不存在才添加，存在则不添加且不抛异常
        /// </summary>
        public static IDictionary<TKey, TValue> AddOrIgnore<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key)) dict.Add(key, value);
            return dict;
        }

        /// <summary>
        /// 将键和值添加到字典中，存在的会被替换 其实是dict[key] = value的更直白的写法
        /// </summary>
        public static IDictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            dict[key] = value;
            return dict;
        }

        /// <summary>
        /// 向字典中批量添加键值对
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="values"></param>
        /// <param name="replaceExisted">如果已存在，是否替换</param>
        public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> values, bool replaceExisted = false)
        {
            foreach (var pair in values)
            {
                if (dict.ContainsKey(pair.Key) == false || replaceExisted)
                    dict[pair.Key] = pair.Value;
            }
            return dict;
        }

        /// <summary>
        /// 获取与指定的键相关联的值，如果没有则返回指定的默认值或Value类的默认值（引用类型默认值null，值类型返回0或false或'\0'等）（好处：不用写ContainsKey）
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key">如果是null也会返回默认值</param>
        /// <param name="defaultValue">指定默认值</param>
        /// <returns></returns>
        public static TValue? GetValueOrCustom<TKey, TValue>(this IDictionary<TKey, TValue>? dict, TKey? key, TValue? defaultValue = default)
        {
            if (key == null || dict == null) return defaultValue;
            return dict.TryGetValue(key, out TValue result) == false ? defaultValue : result;
        }

        /// <summary>
        /// 检查指定字典中是否存在任意一个给定的元素
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        [ContractAnnotation("dict:null => false; keys:null => false")]
        public static bool ContainsAny<TKey, TValue>(this IDictionary<TKey, TValue>? dict, params TKey[]? keys)
        {
            if (keys == null || dict.IsNullOrEmptySet()) return false;
            return keys.Any(dict.ContainsKey);
        }
        /// <summary>
        /// 检查指定字典中是否存在任意一个给定的元素
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        [ContractAnnotation("dict:null => false; keys:null => false")]
        public static bool ContainsAll<TKey, TValue>(this IDictionary<TKey, TValue>? dict, params TKey[]? keys)
        {
            if (keys == null || dict.IsNullOrEmptySet()) return false;
            return keys.All(dict.ContainsKey);
        }
    }
}