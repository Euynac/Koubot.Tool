using Koubot.Tool.Expand;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Koubot.Tool.String
{
    /// <summary>
    /// 中文数字类
    /// </summary>
    public class ZhNumber
    {
        private static readonly Dictionary<string, string> ZhUpper2LowerList = new Dictionary<string, string>()
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
            return str.ReplaceAllFromPairSet(ZhUpper2LowerList, true);
        }
        /// <summary>
        /// 数字中文大写转中文小写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToZhLower(string str)
        {
            if (str.IsNullOrEmpty()) return str;
            return str.ReplaceAllFromPairSet(ZhUpper2LowerList, false);
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
            if (cnum.IsInStringSet(ZhUpper2LowerList.Keys)) cnum = ToZhLower(cnum);//若存在大写则转换为小写
            long firstUnit = 1;//一级单位                
            long secondUnit = 1;//二级单位 
            long result = 0;//结果
            for (int i = cnum.Length - 1; i >= 0; --i)//从低到高位依次处理
            {
                long tmpUnit = CharToUnit(cnum[i]);
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
        /// 将字符串中所有中文数字转成阿拉伯数字数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToArabicNumber(string str)
        {
            Regex regex = new Regex("[零壹一贰两二叁三肆四伍五陆六柒七捌八玖九拾十佰百仟千亿万]+");
            static string matchEvaluator(Match match) => ParseZnToInt(match.Value).ToString();//高级写法：本地函数
            return regex.Replace(str, matchEvaluator);
        }

        /// <summary>
        /// 判断字符串是否存在中文（大/小写）数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsContainZhNumber(string str)
        {
            if (str.IsNullOrWhiteSpace()) return false;
            return str.IsInStringSet(ZhUpper2LowerList.Keys) || str.IsInStringSet(ZhUpper2LowerList.Values);
        }
    }
}
