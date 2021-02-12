using System.Text;

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

        private enum RemoveEscapeCharsStates
        {
            Reset,
            FoundEscapeChar
        }
        /// <summary>
        /// Remove escape chars from text.
        /// </summary>
        /// <param name="originalText"></param>
        /// <param name="escapeChar"></param>
        /// <returns></returns>
        public static string RemoveEscapeChars(string originalText, char escapeChar = '\\')
        {
            StringBuilder result = new StringBuilder();
            RemoveEscapeCharsStates state = RemoveEscapeCharsStates.Reset;
            foreach (char chr in originalText)
            {
                switch (state)
                {
                    case RemoveEscapeCharsStates.Reset:
                        if (chr == escapeChar)
                        {
                            state = RemoveEscapeCharsStates.FoundEscapeChar;
                        }
                        else
                        {
                            result.Append(chr);
                            state = RemoveEscapeCharsStates.Reset;
                        }
                        break;
                    case RemoveEscapeCharsStates.FoundEscapeChar:
                        result.Append(chr);
                        state = RemoveEscapeCharsStates.Reset;
                        break;
                    default:
                        throw new System.Exception("Unknown state");
                }
            }
            if (state != RemoveEscapeCharsStates.Reset)
            {
                throw new System.Exception($"{state} is not an accept state!");
            }
            return result.ToString();
        }
    }
}
