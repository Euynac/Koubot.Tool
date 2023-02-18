using System;
using Koubot.Tool.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Koubot.Tool.String;

/// <summary>
/// 中文数字类
/// </summary>
public class ZhNumber
{
    private static readonly Dictionary<string, string> ZhUpper2LowerList = new()
    {
        {"壹", "一"},
        {"贰", "二"},
        {"叁", "三"},
        {"肆", "四"},
        {"伍", "五"},
        {"陆", "六"},
        {"柒", "七"},
        {"捌", "八"},
        {"玖", "九"},
        {"拾", "十"},
        {"佰", "百"},
        {"仟", "千"},
        {"两", "二"},
    };
    /// <summary>
    /// 数字中文小写转中文大写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToZhUpper(string str)
    {
        if (str.IsNullOrEmpty()) return str;
        return str.ReplaceBasedOnDict(ZhUpper2LowerList, true);
    }
    /// <summary>
    /// 数字中文大写转中文小写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToZhLower(string str)
    {
        if (str.IsNullOrEmpty()) return str;
        return str.ReplaceBasedOnDict(ZhUpper2LowerList, false);
    }

    /// <summary>
    /// 转换数字
    /// </summary>
    protected static long CharToNumber(char c)
    {
        switch (c)
        {
            case '一': return 1;
            case '二':
            case '两':
                return 2;
            case '三': return 3;
            case '四': return 4;
            case '五': return 5;
            case '六': return 6;
            case '七': return 7;
            case '八': return 8;
            case '九': return 9;
            case '零': return 0;
            default: return 0;
        }
    }

    /// <summary>
    /// 转换单位
    /// </summary>
    protected static long CharToUnit(char c)
    {
        return c switch
        {
            '十' => 10,
            '百' => 100,
            '千' => 1000,
            '万' => 10000,
            '亿' => 100000000,
            _ => 1,
        };
    }
    /// <summary>
    /// 将中文（大写/小写）数字转换阿拉伯数字（未完善 比如：十一万、一千五）
    /// </summary>
    /// <param name="cnum">汉字数字</param>
    /// <returns>长整型阿拉伯数字</returns>
    /// 原始来源 https://www.xuebuyuan.com/691129.html 
    public static long ParseZnToInt(string cnum)
    {
        if (cnum.ContainsAny(ZhUpper2LowerList.Keys)) cnum = ToZhLower(cnum);//若存在大写则转换为小写
        long firstUnit = 1;//一级单位                
        long secondUnit = 1;//二级单位 
        long result = 0;//结果
        for (var i = cnum.Length - 1; i >= 0; --i)//从低到高位依次处理
        {
            var tmpUnit = CharToUnit(cnum[i]);
            if (tmpUnit > firstUnit)//判断此位是数字还是单位
            {
                firstUnit = tmpUnit;//是的话就赋值,以备下次循环使用
                secondUnit = 1;
                if (i == 0)//处理如果是"十","十一"这样的开头的
                {
                    result += firstUnit * secondUnit;
                }
                continue;//结束本次循环
            }
            else if (tmpUnit > secondUnit)
            {
                secondUnit = tmpUnit;
                continue;
            }
            result += firstUnit * secondUnit * CharToNumber(cnum[i]);//如果是数字,则和单位相乘然后存到结果里
        }
        if (result == 0) result = firstUnit * secondUnit;
        return result;
    }

    /// <summary>
    /// 将字符串中所有阿拉伯数字转成中文数字
    /// </summary>
    /// <param name="str"></param>
    /// <param name="upper">是否转成大写中文数字</param>
    /// <returns></returns>
    public static string FromArabicNumber(string str, bool upper = false)
    {
        return ArabicRegex.Replace(str, match =>
            ChineseNumber.GetString(decimal.Parse(match.Value), upper));
    }

    public static Regex ArabicRegex = new("[0-9]+");
    public static Regex ChineseRegex = new("[零壹一贰两二叁三肆四伍五陆六柒七捌八玖九拾十佰百仟千亿万]+");
    /// <summary>
    /// 将字符串中所有中文数字转成阿拉伯数字
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToArabicNumber(string str)
    {
        static string matchEvaluator(Match match) => ParseZnToInt(match.Value).ToString();//高级写法：本地函数
        return ChineseRegex.Replace(str, matchEvaluator);
    }

    /// <summary>
    /// 判断字符串是否存在中文（大/小写）数字
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsContainZhNumber(string str)
    {
        if (str.IsNullOrWhiteSpace()) return false;
        return str.ContainsAny(ZhUpper2LowerList.Keys) || str.ContainsAny(ZhUpper2LowerList.Values);
    }
}


/// <summary>
/// From https://github.com/zmjack/Chinese
/// </summary>
internal static class ChineseNumber
    {
       
        static ChineseNumber()
        {
            SuperiorLevels = new[] { "", "万", "亿", "兆", "京", "垓", "秭", "穰" };
        }

        public static readonly string[] UpperNumberValues = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        public static readonly string[] LowerNumberValues = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

        private const int PART_COUNT = 4;
        private const int SUPERIOR_LEVELS_COUNT = 8;
        private static string[] superiorLevels;

        /// <summary>
        /// 自定义分级读法（简体中文，个位为空，从低到高设置八级），默认为 ["", "万", "亿", "兆", "京", "垓", "秭", "穰"]。
        /// </summary>
        public static string[] SuperiorLevels
        {
            get => superiorLevels;
            set
            {
                if (value.Length != SUPERIOR_LEVELS_COUNT) throw new ArgumentException("自定义分级读法必须设置八级。");
                superiorLevels = value;
            }
        }

        public static readonly string[] UpperLevels = { "", "拾", "佰", "仟" };
        public static readonly string[] LowerLevels = { "", "十", "百", "千" };
    
        /// <summary>
        /// 获取数值的数值读法。
        /// </summary>
        /// <param name="number"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static string GetString(decimal number, bool upper = false)
        {
            number = decimal.Floor(number);

            string[] numberValues;
            string[] levels;
            if (upper)
            {
                numberValues = UpperNumberValues;
                levels = UpperLevels;
            }
            else
            {
                numberValues = LowerNumberValues;
                levels = LowerLevels;
            }

            string? GetPartString(char[] singles, string level, bool prevZero)
            {
                if (!singles.Any()) return string.Empty;

                var sb = new StringBuilder();
                var zero = prevZero;
                foreach (var (k, v) in singles.WithIndex())
                {
                    if (v != '0')
                    {
                        var value = numberValues[v - '0'];
                        var singleNumberUnit = levels[singles.Length - 1 - k];

                        if (zero) sb.Append(numberValues[0]);
                        sb.Append($"{value}{singleNumberUnit}");

                        zero = false;
                    }
                    else zero = true;
                }

                if (sb.Length == 0) return null;
                else
                {
                    sb.Append(level);
                    return sb.ToString();
                }
            }

            var s_number = number.ToString();
            var enumerator_s_number = s_number.GetEnumerator();

            var levelParts = new char[(s_number.Length - 1) / 4 + 1][];
            var enumerator_levelParts = levelParts.GetEnumerator();
            if (enumerator_levelParts.MoveNext())
            {
                var mod = s_number.Length % PART_COUNT;
                levelParts[0] = new char[mod > 0 ? mod : 4];
                for (var j = 0; j < levelParts[0].Length; j++)
                {
                    enumerator_s_number.MoveNext();
                    levelParts[0][j] = enumerator_s_number.Current;
                }

                var i = 1;
                while (enumerator_levelParts.MoveNext())
                {
                    levelParts[i] = new char[4];
                    for (var j = 0; j < PART_COUNT; j++)
                    {
                        enumerator_s_number.MoveNext();
                        levelParts[i][j] = enumerator_s_number.Current;
                    }
                    i++;
                }
            }

            var sb = new StringBuilder();
            var part_i = 0;
            var prevZero = false;
            foreach (var part in levelParts)
            {
                var partString = GetPartString(part, SuperiorLevels[levelParts.Length - 1 - part_i], prevZero);
                if (partString is not null)
                {
                    sb.Append(partString);
                    prevZero = false;
                }
                else prevZero = true;
                part_i++;
            }

            if (sb.Length < 2) return sb.ToString();
            if ((sb[0] == '一' && sb[1] == '十') || (sb[0] == '壹' && sb[1] == '拾'))
            {
                sb.Remove(0, 1);
            }


            return sb.ToString();
        }

    }
