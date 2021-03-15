namespace Koubot.Tool.General
{
    /// <summary>
    /// 通用字符串工具类
    /// </summary>
    public static class StringTool
    {
        #region 全角转换半角以及半角转换为全角
        ///字符串转换为全角(full-width)
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        public static string ToFullWidth(string input)
        {
            if (input == null)
            {
                return "";
            }
            // 半角转全角：
            char[] array = input.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == 32)
                {
                    array[i] = (char)12288;
                    continue;
                }
                if (array[i] < 127)
                {
                    array[i] = (char)(array[i] + 65248);
                }
            }
            return new string(array);
        }

        /// <summary>
        /// 字符串转换为半角(half-width)
        /// 全角空格为12288，半角空格为32;
        /// 其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToHalfWidth(string input)
        {
            if (input == null)
            {
                return "";
            }
            char[] array = input.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == 12288)
                {
                    array[i] = (char)32;
                    continue;
                }
                if (array[i] > 65280 && array[i] < 65375)
                {
                    array[i] = (char)(array[i] - 65248);
                }
            }
            return new string(array);
        }
        #endregion
    }
}
