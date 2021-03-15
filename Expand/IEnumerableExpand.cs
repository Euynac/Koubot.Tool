using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Koubot.Tool.Expand
{
    /// <summary>
    /// 
    /// </summary>
    public static class IEnumerableExpand
    {
        /// <summary>
        /// 判断一个集合是否是 null 或空集合
        /// </summary>
        /// <param name="collection">指定的集合</param>
        /// <returns></returns>
        [ContractAnnotation("null => true")] //能够教会ReSharper空判断(传入的是null，返回true)
        public static bool IsNullOrEmptySet<T>([CanBeNull][NoEnumeration] this IEnumerable<T> collection) //指示不会对collection进行读写操作，但这里读了?
            => collection == null || !collection.Any();

        /// <summary>
        /// 判断一个集合是否是 null 或空集合
        /// </summary>
        [ContractAnnotation("null => true")] //能够教会ReSharper空判断(传入的是null，返回true)
        public static bool IsNullOrEmptySet([CanBeNull][NoEnumeration] this IEnumerable @this) => @this == null || !@this.GetEnumerator().MoveNext();

        /// <summary>
        /// 尝试获取与指定的键相关联的值
        /// </summary>
        /// <param name="dict">可为空</param>
        /// <param name="value">当本方法返回时，如果找到了指定的键，则返回与该键相关联的值；否则，返回值参数类型的默认值或设定的值。这个参数是在未初始化的情况下传递的。</param>
        /// <param name="key">要获取值的键，可为空，为空必返回false</param>
        /// <param name="defaultValue">失败时返回的默认值或设定的值</param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        [ContractAnnotation("dict:null => false; key:null => false")]
        public static bool TryGetValueOrDefault<TKey, TValue>([CanBeNull] this IDictionary<TKey, TValue> dict, [CanBeNull] TKey key, out TValue value, TValue defaultValue = default)
        {
            if (dict.IsNullOrEmptySet() || key == null)
            {
                value = defaultValue;
                return false;
            }
            if (dict.TryGetValue(key, out value)) return true;
            value = defaultValue;
            return false;
        }

        /// <summary>
        /// 尝试通过Value获取Key的值（多个value相同仅获取一个key，所以一般用于value和key一对一）
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="value">预测Dictionary中会有的值</param>
        /// <param name="key">若是存在value将返回key</param>
        /// <returns>成功返回true且返回key，不成功则返回false</returns>
        public static bool TryGetKey<TKey, TValue>([CanBeNull] this IEnumerable<KeyValuePair<TKey, TValue>> dict, TValue value, out TKey key)
        {
            key = default;
            if (dict.IsNullOrEmptySet()) return false;
            foreach (var pair in dict)
            {
                if (pair.Value.Equals(value))
                {
                    key = pair.Key;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 尝试获取所有指定Value对应的Key值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="value">预测集合中会有的值</param>
        /// <param name="key">value对应的所有Key</param>
        /// <returns>成功返回true且返回key的List，不成功则返回false</returns>
        public static bool TryGetAllKey<TKey, TValue>([CanBeNull] this IEnumerable<KeyValuePair<TKey, TValue>> dict, TValue value, out List<TKey> key)
        {
            key = new List<TKey>();
            if (dict.IsNullOrEmptySet()) return false;
            foreach (var keyValuePair in dict)
            {
                if (keyValuePair.Value.Equals(value))
                {
                    key.Add(keyValuePair.Key);
                }
            }
            return key.Count != 0;
        }
        /// <summary>
        /// 将可空类型的集合转换为不可空的<seealso cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> ConvertToNotNullable<T>(this IEnumerable<T?> list) where T : struct
        {
            return list.Where(i => i != null).Select(i => i.Value).ToList();
        }

        /// <summary>
        /// 往IEnumerable中增加元素（调用的是中之类型的方法，不同于Append）
        /// 注意不可使用IEnumerable&lt;dynamic&gt;然后调用，dynamic无法正确解析到T，会被当做object，需要使用反射自己做出泛型T
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="item">要加入的元素</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void Add<T>(this IEnumerable<T> enumerable, T item)
        {
            switch (enumerable)
            {
                case Stack<T> stack:
                    stack.Push(item);
                    break;
                case Queue<T> queue:
                    queue.Enqueue(item);
                    break;
                case ICollection<T> list:
                    list.Add(item);
                    break;
                default:
                    throw new Exception($"不支持{enumerable.GetType().FullName}类型的Add");
            }
        }
    }
}