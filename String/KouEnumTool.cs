using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Koubot.Tool.String
{
    /// <summary>
    /// 指定某枚举是KouEnum的Name，可以设定该Enum的转换名
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class KouEnumName : Attribute
    {
        /// <summary>
        /// 使用此类字符串来转换成该Enum，默认都是toLower后对比的
        /// </summary>
        public string[] Names { get; }
        /// <summary>
        /// 指示该枚举拥有的名字
        /// </summary>
        /// <param name="names"></param>
        public KouEnumName(params string[] names)
        {
            Names = names;
        }
    }
    /// <summary>
    /// KouEnum工具，可将string转换为对应的Enum
    /// </summary>
    public static class KouEnumTool
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Enum>>
            _enumCache = new();
        /// <summary>
        /// 尝试通过字符串获取对应KouEnum标签枚举
        /// </summary>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryGetKouEnum<T>(string str, out T result) where T : struct, Enum
        {
            result = default;
            if (!TryGetKouEnum(typeof(T), str, out object resultEnum)) return false;
            result = (T)resultEnum;
            return true;
        }

        /// <summary>
        /// 尝试通过字符串获取对应KouEnum标签枚举
        /// </summary>
        /// <param name="enumType">枚举的类型</param>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryGetKouEnum(Type enumType, string str, out object result)
        {
            result = default;
            if (!_enumCache.TryGetValue(enumType, out Dictionary<string, Enum> enumDict))
            {
                CreateCache(enumType);//如果无缓存，自动创建该Enum缓存
                _enumCache.TryGetValue(enumType, out enumDict);
            }
            if (enumDict == null || !enumDict.TryGetValue(str, out Enum enumResult)) return false;
            result = enumResult;
            return true;
        }
        /// <summary>
        /// 创建该enum的枚举name缓存
        /// </summary>
        /// <param name="enumType"></param>
        private static void CreateCache(Type enumType)
        {
            var enumValues = Enum.GetValues(enumType);
            Dictionary<string, Enum> enumsNameDict = new Dictionary<string, Enum>();
            foreach (var value in enumValues)
            {
                var nameEnum = enumType.GetField(value.ToString())?.GetCustomAttribute<KouEnumName>(false);
                if (nameEnum == null) continue;
                foreach (var name in nameEnum.Names)
                {
                    if (string.IsNullOrEmpty(name)) continue;
                    enumsNameDict[name] = (Enum)value;
                }
            }
            _enumCache.TryAdd(enumType, enumsNameDict.Count == 0 ? null : enumsNameDict);
        }
    }
}