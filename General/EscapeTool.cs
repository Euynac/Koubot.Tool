namespace Koubot.Tool.General
{
    /// <summary>
    /// 转义工具类 用于转义一些在某些场合可能引发问题的字符串
    /// </summary>
    public static class EscapeTool
    {
        /// <summary>
        /// 正则表达式模式字符串转义 (不支持\的转义)
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string ToRegexPattern(this string pattern)
        {
            char[] escapeList = { '(', ')', '[', ']', '{', '}', '!', '?', '^', '|', '.', '+', '*', '$', '#' };//Regular Expression中的特殊字符转义
            foreach (char chr in escapeList)
            {
                if (pattern.Contains(chr.ToString()))
                {
                    pattern = pattern.Replace(chr.ToString(), $"\\{chr}");
                }
            }
            return pattern;
        }

        //'(', ')', '[', ']', '{', '}', '!', '?', '^', '|', ',', ':','.', '+', '*', '=', '$', '#', '<', '>', '@', '%', '&', '-', '_', '\'', '"'
    }
}
