using Koubot.Tool.Extensions;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Koubot.Tool.String
{
    /// <summary>
    /// 常用的数字转换器
    /// </summary>
    public static class NumberConvertor
    {
        /// <summary>
        /// 使用网络、英文缩写等常用数字单位缩写表达处理字符串，将其全部替换为数字
        /// </summary>
        /// <param name="str">支持格式为1k、1w、1kw</param>
        /// <param name="parsedStr">处理成数字的的字符串</param>
        /// <returns></returns>
        public static bool WebUnitDouble(string str, out string parsedStr)
        {

            parsedStr = str;
            if (str.IsNullOrWhiteSpace()) return false;
            List<string> patternList = new List<string>()
            {
                @"(?<k>\d+(\.\d+)?)(?:k(?!w))(?<tail>\d+)?",//0 匹配10k、1k500之类
                @"(?<w>\d+(?:\.\d+)?)(?:w)((?<k>\d+(\.\d+)?)(?:k(?!w)))?(?<tail>\d+)?",//1  匹配6w1k、6w1k500之类
                @"(?<kw>\d+(\.\d+)?)(?:kw)((?<w>\d+(?:\.\d+)?)(?:w))?((?<k>\d+(\.\d+)?)(?:k(?!w)))?(?<tail>\d+)?",//2 匹配6kw6w
            };
            var success = false;//指示是否成功转换过一次

            for (var i = patternList.Count - 1; i >= 0; i--)
            {
                var regex = new Regex(patternList[i]);
                if (regex.IsMatch(str))
                {
                    success = true;
                    foreach (Match match in regex.Matches(str))
                    {
                        var kwStr = match.Groups["kw"].Value;
                        var wStr = match.Groups["w"].Value;
                        var kStr = match.Groups["k"].Value;
                        var tailStr = match.Groups["tail"].Value;
                        double.TryParse(kwStr, out var kw);
                        double.TryParse(wStr, out var w);
                        double.TryParse(kStr, out var k);
                        double.TryParse(tailStr, out var tail);
                        var total = kw * 10000000 + w * 10000 + k * 1000 + tail;
                        parsedStr = regex.Replace(parsedStr, total.ToString(CultureInfo.InvariantCulture), 1);
                    }
                }
            }

            if (success)
            {
                parsedStr = parsedStr.Replace("k", "");
                parsedStr = parsedStr.Replace("w", "");
            }
            return success;
        }
    }
}
