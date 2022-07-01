using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Koubot.Tool.Extensions
{
    /// <summary>
    /// Extension Methods of Regex related
    /// </summary>
    public static class RegexExtensions
    {
        /// <summary>
        /// 判断字符串是否能够匹配正则表达式
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <returns></returns>
        public static bool IsMatch(this string? s, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.None)
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
        public static string? Match(this string? s, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.None, bool ifNotExistReturnNull = false)
        {
            if (s == null) return ifNotExistReturnNull ? null : "";
            var result = Regex.Match(s, pattern, regexOptions);
            return result.Success ? result.Value : ifNotExistReturnNull ? null : "";
        }

        /// <summary>
        /// 搜索指定正则表达式的第一个匹配项并得到捕获的匹配组，并获取替换匹配项后的字符串结果
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="replaced">替换后的字符串，没有匹配上则返回原来的字符串</param>
        /// <param name="groupResult">匹配到则获取匹配组结果</param>
        /// <param name="replaceTo"></param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <returns>false则没有匹配上</returns>
        public static bool MatchOnceThenReplace(this string s,
            [RegexPattern] string pattern, out string replaced,
            out GroupCollection? groupResult,
            RegexOptions regexOptions = RegexOptions.None, string replaceTo = "")
        {
            replaced = s;
            groupResult = null;
            var regex = new Regex(pattern, regexOptions);
            var result = regex.Match(s);
            if (!result.Success) return false;
            groupResult = result.Groups;
            replaced = regex.Replace(s, replaceTo, 1);
            return true;
        }
       
        /// <summary>
        /// 搜索指定正则表达式的所有匹配项并返回捕获到的所有子字符串，不存在的将返回count=0的list
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <returns></returns>
        public static List<string> Matches(this string? s, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(s)) return list;
            list.AddRange(from Match item in Regex.Matches(s, pattern, regexOptions) select item.Value);
            return list;
        }

        /// <summary>
        /// 使用指定的替换字符串替换与正则表达式匹配的指定数量的字符串
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="replacement">指定的替换字符串</param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <param name="count">为0默认全部替换</param>
        /// <returns></returns>
        public static string RegexReplace(this string s, [RegexPattern] string pattern, string replacement = "",
            RegexOptions regexOptions = RegexOptions.None, int count = 0)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var regex = new Regex(pattern, regexOptions);
            return count > 0 ? regex.Replace(s, replacement, count) : regex.Replace(s, replacement);
        }
        /// <summary>
        /// 找到字符串中符合正则表达式的给定命名捕获组名的所有匹配项。未找到返回Count=0的List
        /// </summary>
        /// <param name="s">要测试的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="groupName">正则表达式中的命名捕获组中的名字</param>
        /// <param name="regexOptions">使用指定的选项进行匹配，可按位组合</param>
        /// <returns></returns>
        public static List<string> MatchedGroupValues(this string? s,
            [RegexPattern] string pattern, string groupName,
            RegexOptions regexOptions = RegexOptions.None)
        {
            var itemList = new List<string>();
            if (s == null || groupName.IsNullOrEmpty()) return itemList;
            var regex = new Regex(pattern);
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
        public static bool IsValidRegexPattern(this string? s, out string? error)
        {
            error = null;
            if (s == null)
            {
                error = "pattern is empty!";
                return false;
            }
            try
            {
                var regex = new Regex(s);
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