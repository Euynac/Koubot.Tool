using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Koubot.Tool.Expand
{
    /// <summary>
    /// 正则表达式类扩展方法
    /// </summary>
    public static class RegexExpand
    {
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
        /// 搜索指定正则表达式的所有匹配项并返回捕获到的所有子字符串，不存在的将返回count=0的list
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <returns></returns>
        public static List<string> Matches([CanBeNull] this string s, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(s)) return list;
            list.AddRange(from Match item in Regex.Matches(s, pattern, regexOptions) select item.Value);
            return list;
        }
        /// <summary>
        /// 正则表达式替换
        /// </summary>
        /// <param name="s">要被替换的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="replacement">匹配的字符串被替换为</param>
        /// <param name="count">为null全部替换，否则替换指定次数</param>
        /// <returns></returns>
        public static string RegexReplace([CanBeNull] this string s, [RegexPattern] string pattern,
            string replacement, int? count = null)
        {
            if (s == null) return null;
            Regex regex = new Regex(pattern);
            return count != null ? regex.Replace(s, replacement, count.Value) : regex.Replace(s, replacement);
        }

        /// <summary>
        /// 找到字符串中符合正则表达式的给定命名捕获组名的所有匹配项。未找到返回Count=0的List
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="groupName">正则表达式中的命名捕获组中的名字</param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <returns></returns>
        public static List<string> MatchedGroupValues([CanBeNull] this string s,
            [RegexPattern] string pattern, string groupName,
            RegexOptions regexOptions = RegexOptions.None)
        {
            List<string> itemList = new List<string>();
            if (s == null || groupName.IsNullOrEmpty()) return itemList;
            Regex regex = new Regex(pattern);
            foreach (Match match in regex.Matches(s))
            {
                var matched = match.Groups[groupName].Value;
                if (matched != "") itemList.Add(matched);
            }

            return itemList;
        }

        /// <summary>
        /// 判断是否是有效的正则表达式模式
        /// </summary>
        /// <param name="s"></param>
        /// <param name="error">如果是无效正则表达式，输出错误原因</param>
        /// <returns></returns>
        public static bool IsValidRegexPattern([CanBeNull] this string s, out string error)
        {
            error = null;
            if (s == null) return false;
            try
            {
                Regex regex = new Regex(s);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }
    }
}