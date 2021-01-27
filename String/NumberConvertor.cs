using Koubot.Tool.Expand;
using System.Collections.Generic;

namespace Koubot.Tool.String
{
    /// <summary>
    /// 常用的数字转换器
    /// </summary>
    public static class NumberConvertor
    {
        /// <summary>
        /// 使用网络、英文缩写等常用数字单位缩写表达处理字符串为double
        /// </summary>
        /// <param name="str">支持格式为1k、1w、1kw</param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool WebUnitDouble(string str, out double result)
        {
            result = 0;
            if (str.IsNullOrWhiteSpace()) return false;
            List<string> patternList = new List<string>()
            {
                @"\d+(\.\d+)?(k(?!w))",//0
                @"\d+(\.\d+)?(w)",//1
                @"\d+(\.\d+)?(kw)",//2
            };
            bool success = false;//指示是否成功转换过一次
            for (int i = 0; i < patternList.Count; i++)
            {
                var numStr = str.Match(patternList[i]);
                if (numStr.IsNullOrEmpty() || !double.TryParse(numStr.Match(@"\d+(\.\d+)?"), out double num)) continue;
                success = true;
                switch (i)
                {
                    case 0:
                        result += num * 1000;
                        break;
                    case 1:
                        result += num * 10000;
                        break;
                    case 2:
                        result += num * 10000000;
                        break;
                }
            }
            return success;
        }
    }
}
