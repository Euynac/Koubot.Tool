using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Koubot.Tool.Expand
{
    /// <summary>
    /// 时间戳（格林威治时间1970年01月01日00时00分00秒）类型
    /// </summary>
    public enum TimeStampType
    {
        /// <summary>
        /// 总秒数
        /// </summary>
        Unix,
        /// <summary>
        /// 总毫秒数
        /// </summary>
        Javascript
    }
    /// <summary>
    /// 系统类拓展方法集
    /// </summary>
    public static class SystemExpand
    {
        #region 时间类拓展
        /// <summary>
        /// 字符串形式时间戳转DateTime
        /// </summary>
        /// <returns>转换失败返回<see cref="DateTime"/>的default</returns>
        public static DateTime ToDateTime(this string timestampStr, TimeStampType timeStampType = TimeStampType.Unix)
        {
            return long.TryParse(timestampStr, out long timestamp) ? ToDateTime(timestamp, timeStampType) : default;
        }
        /// <summary>
        /// 获取指定类型的时间戳的 <see cref="DateTime"/> 表示形式
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="timeStampType">指定类型，默认Unix（秒为单位）</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timestamp, TimeStampType timeStampType = TimeStampType.Unix)
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            DateTime daTime = new DateTime();
            switch (timeStampType)
            {
                case TimeStampType.Unix:
                    daTime = startTime.AddSeconds(timestamp);
                    break;
                case TimeStampType.Javascript:
                    daTime = startTime.AddMilliseconds(timestamp);
                    break;
            }
            return daTime;
        }
        /// <summary>
        /// DateTime转时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeStampType"></param>
        /// <returns></returns>
        public static long ToTimeStamp(this DateTime dateTime, TimeStampType timeStampType = TimeStampType.Unix)
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            long timestamp = 0;
            switch (timeStampType)
            {
                case TimeStampType.Unix:
                    timestamp = (long)(dateTime - startTime).TotalSeconds;
                    break;
                case TimeStampType.Javascript:
                    timestamp = (long)(dateTime - startTime).TotalMilliseconds;
                    break;
            }
            return timestamp;
        }

        /// <summary>
        /// 字符串形式DateTime转时间戳
        /// </summary>
        /// <param name="dateTimeStr"></param>
        /// <param name="timeStampType"></param>
        /// <returns>转换失败返回0</returns>
        public static long ToUnixTimeStamp(this string dateTimeStr, TimeStampType timeStampType = TimeStampType.Unix)
        {
            return  DateTime.TryParse(dateTimeStr, out DateTime dateTime) ? ToTimeStamp(dateTime, timeStampType) : 0;
        }

        
        #endregion

        #region Attribute类拓展
        /// <summary>
        /// 获取该类型上指定类型的CustomAttribute，包括其接口上的（仅支持类，不支持方法或属性等）
        /// </summary>
        /// <typeparam name="T">指定的Attribute类型</typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomAttributesIncludingBaseInterfaces<T>(this Type type)
        {
            var attributeType = typeof(T);
            return type.GetCustomAttributes(attributeType, true).
                Union(type.GetInterfaces().
                    SelectMany(interfaceType => interfaceType.GetCustomAttributes(attributeType, true))).
                Distinct().Cast<T>();
        }
        #endregion

        #region Enum类拓展
        /// <summary>
        /// 读取 <see cref="System.Enum"/> 标记 <see cref="System.ComponentModel.DescriptionAttribute"/> 的值
        /// </summary>
        /// <param name="value">原始 <see cref="System.Enum"/> 值</param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            DescriptionAttribute attribute = value?.GetType().GetField(value.ToString())?.GetCustomAttribute<DescriptionAttribute>(false);
            return attribute?.Description;
        }
        /// <summary>
        /// 判断按位枚举是否存在指定的任意一个选项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool HasAnyFlag<T>(this T value, params T[] flags) where T : Enum
        {
            return flags != null && flags.Any(flag => value.HasFlag(flag));
        }
        /// <summary>
        /// 判断按位枚举是否存在指定的所有选项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool HasAllFlag<T>(this T value, params T[] flags) where T : Enum
        {
            return flags != null && flags.All(flag => value.HasFlag(flag));
        }
        /// <summary>
        /// 判断按位枚举是否存在指定的选项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool HasTheFlag<T>(this T value,T flag) where T : Enum
            =>   value.HasFlag(flag);
        
        #endregion

        #region Object类拓展
        /// <summary>
        /// 如果引用类型对象为空则返回null字符串，否则返回要成为的那个字符串（在拼接字符串时使用，若是be要嵌套的话记得加?否则会null引用）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="be"></param>
        /// <param name="useSmartConcat">是否启用自动拼接obj.ToString()，使用 $0 指定自动位置，有冲突时注意关闭</param>
        /// <returns></returns>
        [ContractAnnotation("obj:null => null")]
        public static string BeNullOr<T>([CanBeNull] this T obj, string be, bool useSmartConcat = false) where T : class //引用类型约束
        {
            return obj == null ? null : !useSmartConcat ? be : be?.Replace("$0", obj.ToString());
        }

        /// <summary>
        /// 如果值类型为初始默认值则返回null字符串，否则返回要成为的那个字符串（在拼接字符串时使用）（注意特殊情况比如int=0若也有效的话）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="be"></param>
        /// <returns></returns>
        public static string BeDefaultOr<T>(this T obj, string be) where T : struct //值类型约束
        {
            return obj.Equals(default(T)) ? null : be;
        }

        /// <summary>
        /// 如果可空值类型对象为空则返回null字符串，否则返回要成为的那个字符串（在拼接字符串时使用，若是be要嵌套的话记得加?否则会null引用）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="be"></param>
        /// <returns></returns>
        public static string BeNullOr<T>(this T? obj, string be) where T : struct //可空值类型
        {
            return obj == null ? null : be;
        }
        /// <summary>
        /// 判断是否存在一个元素给定的元素与之相等
        /// </summary>
        /// <typeparam name="T">可以为空</typeparam>
        /// <param name="this"></param>
        /// <param name="objects">给定的元素</param>
        /// <returns></returns>
        public static bool EqualsAny<T>([CanBeNull] this T @this, params T[] objects) where T : class
        {
            if (@this == null && objects.Any(obj => obj == null)) return true;
            return objects != null && @this != null && objects.Any(@this.Equals);
        }

        /// <summary>
        /// 判断是否存在一个元素给定的元素与之相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="placeHolder">给定的元素，占位符，只是给编译器通过</param>
        /// <param name="objects">给定的元素</param>
        /// <returns></returns>
        public static bool EqualsAny<T>(this T @this, T placeHolder, params T[] objects) where T : struct
        {
            return placeHolder.Equals(@this) || objects != null && objects.Any(obj => obj.Equals(@this));
        }
        /// <summary>
        /// 判断是否存在一个元素给定的元素与之相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="objects">给定的元素</param>
        /// <returns></returns>
        public static bool EqualsAny<T>(this T? @this, params T?[] objects) where T : struct
        {
            if (@this == null && objects.Any(obj => obj == null)) return true;
            return objects != null && objects.Any(obj => obj.Equals(@this));
        }

        /// <summary>
        /// 判断元素是否满足任意一个方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool SatisfyAny<T>(this T obj, params Func<T, bool>[] predicates) where T : class
        {
            return predicates != null && predicates.Any(p => p.Invoke(obj));
        }
        /// <summary>
        /// 判断元素是否满足所有方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool SatisfyAll<T>(this T obj, params Func<T, bool>[] predicates) where T : class
        {
            return predicates != null && predicates.All(p => p.Invoke(obj));
        }
        #endregion

        #region 委托类拓展
        /// <summary>
        /// 判断是否存在一个元素不满足此方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static bool Any<T>(this Func<T, bool> predicate, params T[] objects) where T : class
        {
            return objects != null && objects.Any(predicate);
        }
        /// <summary>
        /// 判断是否所有元素都满足此方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static bool All<T>(this Func<T, bool> predicate, params T[] objects) where T : class
        {
            return objects != null && objects.All(predicate);
        }


        #endregion

        #region String类拓展
        /// <summary>
        /// 指示指定的字符串是 null 还是空字符串("")
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <returns></returns>
        [ContractAnnotation("null => true")] //能够教会ReSharper空判断(传入的是null，返回true)https://www.jetbrains.com/help/resharper/Contract_Annotations.html#syntax
        public static bool IsNullOrEmpty([CanBeNull] this string s)
        {
            return string.IsNullOrEmpty(s);
        }
        /// <summary>
        /// 指示指定的字符串是 null 还是空字符串("")还是仅由空白字符组成
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <returns></returns>
        [ContractAnnotation("null => true")] //能够教会ReSharper空判断(传入的是null，返回true)
        public static bool IsNullOrWhiteSpace([CanBeNull] this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }
        /// <summary>
        /// 判断是否能够被转换为int型
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsInt([CanBeNull] this string s)
        {
            return int.TryParse(s, out _);
        }
        /// <summary>
        /// 判断字符串是否能够匹配正则表达式
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <returns></returns>
        public static bool IsMatch([CanBeNull] this string s, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            return s != null && Regex.IsMatch(s, pattern, regexOptions);
        }

        /// <summary>
        /// 搜索指定正则表达式的第一个匹配项并得到捕获的子字符串，不存在的默认返回("")
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <param name="ifNotExistReturnNull">如果不存在返回 null，而不是("")</param>
        /// <returns></returns>
        public static string Match([CanBeNull] this string s, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.None, bool ifNotExistReturnNull = false)
        {
            if (s == null) return ifNotExistReturnNull ? null : "";
            var result = Regex.Match(s, pattern, regexOptions);
            return result.Success ? result.Value : ifNotExistReturnNull ? null : "";
        }
        /// <summary>
        /// 使用指定的替换字符串替换与正则表达式匹配的指定数量的字符串
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <param name="replacement">指定的替换字符串</param>
        /// <param name="count">为0默认全部替换</param>
        /// <returns></returns>
        public static string RegexReplace([CanBeNull] this string s, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.None, string replacement = "", int count = 0)
        {
            if (string.IsNullOrEmpty(s)) return s;
            Regex regex = new Regex(pattern, regexOptions);
            return count > 0 ? regex.Replace(s, replacement, count) : regex.Replace(s, replacement);
        }
        /// <summary>
        /// 搜索指定正则表达式的所有匹配项并返回捕获到的所有子字符串，不存在的将返回null
        /// </summary>
        /// <param name="s"></param>
        /// <param name="pattern"></param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <returns></returns>
        public static string[] Matches([CanBeNull] this string s, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var collection = Regex.Matches(s, pattern, regexOptions);
            string[] result = new string[collection.Count];
            int i = 0;
            foreach (Match item in collection)
            {
                result[i] = item.Value;
                i++;
            }
            return result;
        }

        /// <summary>
        /// 将某段字符串按照某指定字符串分割成数组后，查找一个字符串是否在数组中
        /// </summary>
        /// <param name="source">要分割的字符串</param>
        /// <param name="comparisonType">指定搜索规则的枚举值之一</param>
        /// <param name="searchStr">要查找的字符串</param>
        /// <param name="splitArray">分割字符串，输入多个就将其都作为分割符</param>
        /// <returns></returns>
        public static bool SplitContain([CanBeNull] this string source, StringComparison comparisonType, string searchStr,
            params string[] splitArray)
        {
            if (string.IsNullOrEmpty(source) || splitArray.Contains("") || splitArray.Contains(null)) return false;
            return source.Split(splitArray, StringSplitOptions.RemoveEmptyEntries).Any(item => item.Equals(searchStr, comparisonType));
        }

        /// <summary>
        /// 指定字符串在某字符串中出现的所有Index集合，没有则返回null
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="value">要搜索的字符串</param>
        /// <returns></returns>
        public static List<int> AllIndexOf(this string source, string value)
        {
            if (source.Contains(value))
            {
                List<int> list = new List<int>();
                int i = 0;
                while (i >= 0 && i < source.Length)
                {
                    i = source.IndexOf(value, i, StringComparison.Ordinal);
                    if (i >= 0)
                    {
                        list.Add(i);
                        i++;
                    }
                }
                if (list.Count != 0) return list;
            }
            return null;
        }
        /// <summary>
        /// 返回一个值，该值指示指定的子串是否出现在此字符串中。
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="value">要搜寻的字符串。</param>
        /// <param name="comparisonType">指定搜索规则的枚举值之一</param>
        /// <returns>如果 true 参数出现在此字符串中，或者 value 为空字符串 ("")，则为 value；否则为 false。</returns>
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }
        /// <summary>
        /// 检测一个字符串集合中是否存在string的子字符串
        /// </summary>
        /// <param name="source"></param>
        /// <param name="strList">要检测的字符串集合</param>
        /// <param name="comparisonType">指定搜索规则的枚举值之一</param>
        /// <returns></returns>
        public static bool IsInStringSet(this string source, IEnumerable<string> strList, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(source) || strList.IsNullOrEmptySet()) return false;
            return strList.Any(s => source.Contains(s, comparisonType));
        }


        /// <summary>
        /// 将一个string中与提供的键值对集合中的键相同的全部替换为对应的值（注意若是提供的集合中值与键相包含会造成重复替换）
        /// </summary>
        /// <param name="source">原文</param>
        /// <param name="pairList">提供的键值对集合</param>
        /// <param name="reverse">默认正向Key替换为Value，为true则Value替换Key</param>
        /// <returns></returns>
        public static string ReplaceAllFromPairSet(this string source, IEnumerable<KeyValuePair<string, string>> pairList, bool reverse = false)
        {
            StringBuilder stringBuilder = new StringBuilder(source);
            foreach (var pair in pairList)
            {
                if (reverse) stringBuilder.Replace(pair.Value, pair.Key);
                else stringBuilder.Replace(pair.Key, pair.Value);
            }
            return stringBuilder.ToString();
        }
        #endregion

        #region int类拓展
        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="max">最大值，超过则取这个最大值</param>
        /// <returns></returns>
        public static int LimitInRange(this int num, int max)
        {
            return num > max ? max : num;
        }
        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min">最小值，小于则取这个最小值</param>
        /// <param name="max">最大值，大于则取这个最大值</param>
        /// <returns></returns>
        public static int LimitInRange(this int num, int min, int max)
        {
            return (num > max) ? max : (num < min) ? min : num;
        }
        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min">最小值，小于则取这个最小值</param>
        /// <param name="max">最大值，大于则取这个最大值</param>
        /// <returns></returns>
        public static double LimitInRange(this double num, double min, double max)
        {
            return (num > max) ? max : (num < min) ? min : num;
        }
        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="max">最大值，超过则取这个最大值</param>
        /// <returns></returns>
        public static double LimitInRange(this double num, double max)
        {
            return num > max ? max : num;
        }
        #endregion

        #region IDictionary类的拓展
        /// <summary>
        /// 尝试将键和值添加到字典中，如果不存在才添加，存在则不添加且不抛异常
        /// </summary>
        public static IDictionary<TKey, TValue> TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
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
        public static TValue GetValueOrCustom<TKey, TValue>([CanBeNull] this IDictionary<TKey, TValue> dict, [CanBeNull] TKey key, TValue defaultValue = default)
        {
            if (key == null || dict == null) return defaultValue;
            if (dict.TryGetValue(key, out TValue result) == false)
                result = defaultValue;
            return result;
        }

        /// <summary>
        /// 检查指定字典中是否存在任意一个给定的元素
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        [ContractAnnotation("dict:null => false; keys:null => false")]
        public static bool ContainsAny<TKey, TValue>([CanBeNull] this IDictionary<TKey, TValue> dict, params TKey[] keys)
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
        public static bool ContainsAll<TKey, TValue>([CanBeNull] this IDictionary<TKey, TValue> dict, params TKey[] keys)
        {
            if (keys == null || dict.IsNullOrEmptySet()) return false;
            return keys.All(dict.ContainsKey);
        }
        #endregion

        #region IEnumerable类拓展
        /// <summary>
        /// 判断一个集合是否是 null 或空集合
        /// </summary>
        /// <param name="collection">指定的集合</param>
        /// <returns></returns>
        [ContractAnnotation("null => true")] //能够教会ReSharper空判断(传入的是null，返回true)
        public static bool IsNullOrEmptySet<T>([CanBeNull][NoEnumeration] this IEnumerable<T> collection) //指示不会对collection进行读写操作，但这里读了?
        {
            return collection == null || !collection.Any();
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
            if (key.Count != 0)
                return true;
            return false;
        }
        /// <summary>
        /// 将可空类型的Enumerable集合转换为不可空的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> ConvertToNotNullable<T>(this IEnumerable<T?> list) where T : struct
        {
            return list.Where(i => i != null).Select(i => i.Value).ToList();
        }
        #endregion
    }
}
