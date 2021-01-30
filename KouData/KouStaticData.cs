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
        /// 中文数学运算转对应符号
        /// </summary>
        public static IReadOnlyDictionary<string, string> ZhMathToSymbolMath { get; } = new Dictionary<string, string>()
        {
            {"加", "+"},{"减", "-"},{"乘", "*"},{"除","/"}
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

        /// <summary>
        /// 数据库增加的关键词
        /// </summary>
        public static readonly IReadOnlyList<string> AutoModelAddAction = new List<string>
        {
            "add", "create", "增", "增加", "学", "学习", "添加", "加", "新增", "learn","append","a","c"
        };
        /// <summary>
        /// 数据库删除的关键词
        /// </summary>
        public static readonly IReadOnlyList<string> AutoModelDeleteAction = new List<string>
        {
            "delete", "d","remove","删除","丢弃","删","删掉","忘记","forget",
        };
        /// <summary>
        /// 数据库修改的关键词
        /// </summary>
        public static readonly IReadOnlyList<string> AutoModelModifyAction = new List<string>
        {
            "modify", "update", "修改","更改","更新","u","alter","改"
        };
        /// <summary>
        /// 数据库查询的关键词
        /// </summary>
        public static readonly IReadOnlyList<string> AutoModelSearchAction = new List<string>
        {
            "search","retrieve","检索","搜索","查询","query","r","查"
        };
    }
}