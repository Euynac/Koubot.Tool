using System.Collections.Generic;

namespace Koubot.Tool.KouData
{
    /// <summary>
    /// Kou静态数据库
    /// </summary>
    public static class KouStaticData
    {
        /// <summary>
        /// 中文标点与英文标点
        /// </summary>
        public static IReadOnlyDictionary<string, string> ZhToEnPunctuationDict { get; } =
            new Dictionary<string, string>
            {
                {"；", ";"}, {"。", "."}, {"：", ":"}, {"，", ","}, {"？", "?"}, {"！", "!"}, {"“", "\""}, {"”", "\""},
                {"（", "("}, {"）", ")"}, {"【", "["}, {"】", "]"}, {"《", "<"}, {"》", ">"}, {"…", "..."}, {"—", "-"},
                {"￥", "$"}, {"、", "\\"}, {"～", "~"}
            };
        /// <summary>
        /// bool值的转换
        /// </summary>
        public static IReadOnlyDictionary<string, bool> StringToBoolDict { get; } = new Dictionary<string, bool>
        {
            {"yes", true}, {"no", false}, {"1", true}, {"0", false}, {"是", true}, {"否", false}, {"对", true},
            {"错", false}, {"不对", false}, {"on", true}, {"off", false}, {"非", false},{"not", false},
            {"none", false},{"true", true},{"false", false},{"正确", true},{"错误", false},{"open", true},{"close", false},
            {"disable", false},{"enable", true},{"启动", true},{"关闭", false},{"启用", true},{"开", true},{"关", false},
            {"开启", true},{"打开", true},{"t", true},{"f", false}
        };
        /// <summary>
        /// 翻页使用的下一页的关键词
        /// </summary>
        public static readonly IReadOnlyList<string> PageNextList = new List<string>
        {
            "下","n","next","下一页","翻页","后","下一个"
        };
        /// <summary>
        /// 翻页使用的上一页的关键词
        /// </summary>
        public static readonly IReadOnlyList<string> PagePreviousList = new List<string>
        {
            "上","p","previous","上一页","前","上一个","earlier","preceding"
        };
    }
}