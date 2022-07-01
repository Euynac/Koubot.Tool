using Koubot.Tool.Extensions;
using Koubot.Tool.KouData;
using Koubot.Tool.Math;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using static Koubot.Tool.KouData.KouTimeType;
namespace Koubot.Tool.String
{
    /// <summary>
    /// Kou开发常用字符串工具类
    /// </summary>
    public static class KouStringTool
    {

        #region 页数处理

        /// <summary>
        /// 根据用户的输入处理页数
        /// </summary>
        /// <param name="currentPage">当前页面</param>
        /// <param name="userInput">用户输入处理为页数</param>
        /// <param name="parsedPage">处理后的页数</param>
        /// <param name="parseInt">处理Int型页数</param>
        /// <returns>错误输入返回false，能够处理页数返回true</returns>
        public static bool TryParsePage(int currentPage, string userInput, out int parsedPage, bool parseInt = true)
        {
            parsedPage = 0;
            if (userInput.IsNullOrWhiteSpace()) return false;
            if (KouStaticData.PageNextList.Contains(userInput.ToLower()))
            {
                parsedPage = currentPage + 1;
                return true;
            }

            if (KouStaticData.PagePreviousList.Contains(userInput.ToLower()))
            {
                parsedPage = currentPage - 1;
                return true;
            }

            return parseInt && TryToInt(userInput, out parsedPage);
        }

        #endregion


        #region KouType类型适配
        /// <summary>
        /// 将字符串类型的数字转换为bool类型，支持中文以及英文、数字
        /// </summary>
        /// <param name="str"></param>
        /// <param name="boolResult"></param>
        /// <param name="kouType"></param>
        /// <returns></returns>
        public static bool TryToBool(string str, out bool boolResult, bool kouType = true)
        {
            boolResult = false;
            if (str.IsNullOrWhiteSpace()) return false;
            return !kouType ? bool.TryParse(str, out boolResult) :
                KouStaticData.StringToBoolDict.TryGetValue(str, out boolResult);
        }

        /// <summary>
        /// 将字符串类型的数字转换为double类型，支持中文以及带单位
        /// </summary>
        /// <param name="str"></param>
        /// <param name="doubleResult"></param>
        /// <param name="kouType"></param>
        /// <returns></returns>
        public static bool TryToDouble(string str, out double doubleResult, bool kouType = true)
        {
            doubleResult = 0;
            if (str.IsNullOrWhiteSpace()) return false;
            if (!kouType) return double.TryParse(str, out doubleResult);
            //如果有中文的数字，转为阿拉伯数字
            if (ZhNumber.IsContainZhNumber(str)) str = ZhNumber.ToArabicNumber(str);
            if (!double.TryParse(str, NumberStyles.Float, null, out doubleResult))
            {
                if (NumberConvertor.WebUnitDouble(str, out var parsedStr))
                {
                    str = parsedStr;
                }

                var isPercentage = false;
                if (str.EndsWith("%"))
                {
                    isPercentage = true;
                    str = str.TrimEnd('%');
                }
                var calculator = new ExpressionCalculator();//BUG 没有处理abc等非表达式情况，仍然会认为输入了正确的表达式
                if (str.Contains("上")) str = str.Replace("上", "");//乘上、加上等
                if (str.Contains("去")) str = str.Replace("去", "");
                str = str.ReplaceBasedOnDict(KouStaticData.ZhMathToSymbolMath);
                var result = calculator.Calculate(str)?.ToString();
                if (result != null && result != double.NaN.ToString(CultureInfo.InvariantCulture))
                {
                    if (!double.TryParse(result, out doubleResult)) return false;
                    if (isPercentage) doubleResult /= 100;
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 将字符串类型的数字转换为int类型，支持中文以及带单位
        /// </summary>
        /// <param name="str"></param>
        /// <param name="intResult"></param>
        /// <param name="kouType"></param>
        /// <returns></returns>
        public static bool TryToInt(string str, out int intResult, bool kouType = true)
        {
            var result = TryToDouble(str, out var doubleResult, kouType);
            intResult = (int)doubleResult;
            return result;
        }
        /// <summary>
        /// 将字符串类型的数字转换为long类型，支持中文以及带单位
        /// </summary>
        /// <param name="str"></param>
        /// <param name="longResult"></param>
        /// <param name="kouType"></param>
        /// <returns></returns>
        public static bool TryToLong(string str, out long longResult, bool kouType = true)
        {
            var result = TryToDouble(str, out var doubleResult, kouType);
            longResult = (long)doubleResult;
            return result;
        }

        /// <summary>
        /// 将字符串类型的数字转换为enum类型，支持KouEnumName标签特性别名枚举
        /// </summary>
        /// <param name="str"></param>
        /// <param name="enumResult"></param>
        /// <param name="supportNumeric"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryToEnum<T>(string str, out T enumResult, bool supportNumeric = false) where T : struct, Enum
        {
            enumResult = default;
            var success = TryToEnum(str, typeof(T), out var result);
            if (success)
            {
                enumResult = (T)result;
            }

            return success;
        }

        /// <summary>
        /// 将字符串类型的数字转换为enum类型，支持KouEnumName标签特性别名枚举
        /// </summary>
        /// <param name="str"></param>
        /// <param name="enumType"></param>
        /// <param name="enumResult"></param>
        /// <param name="supportNumeric"></param>
        /// <returns></returns>
        public static bool TryToEnum(string str, Type enumType, out object enumResult, bool supportNumeric = false)
        {
            enumResult = null;
            if (str.IsNullOrEmpty()) return false;
            if (KouEnumTool.TryToKouEnum(enumType, str, out enumResult)) return true;
            try
            {
                if (!supportNumeric && str.IsInt()) return false;
                enumResult = Enum.Parse(enumType, str, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        #endregion

        #region 插件参数处理常用

        /// <summary>
        /// 转换为英文标点符号
        /// </summary>
        /// <returns></returns>
        public static string ToEnPunctuation(this string? str)
        {
            if (str.IsNullOrWhiteSpace()) return str;
            return str.ContainsAny(KouStaticData.ZhToEnPunctuationDict.Keys)
                ? str.ReplaceBasedOnDict(KouStaticData.ZhToEnPunctuationDict)
                : str;
        }

        #endregion

        #region 区间格式转区间

        /// <summary>
        /// 获取TimeSpan型区间值（格式为[7位天数.][00-23小时:][00-59分钟:]00-59秒[.7位毫秒数]）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool TryGetTimeSpanInterval(this string str, out TimeSpan left, out TimeSpan right)
        {
            left = new TimeSpan();
            right = new TimeSpan();
            if (str.IsNullOrWhiteSpace()) return false;
            str = str.Trim();
            //该正则表达式匹配[leftime]lday(7位天数).lhour(0-23):lminute(0-23):lsecond(0-59).lmillisecond(7位毫秒数) 分隔符 [righttime]rday(7位天数).rhour(0-23):rminute(0-23):rsecond(0-59).rmillisecond(7位毫秒数)
            var regex = new Regex(@"^(?<lefttime>(?:(?:(?<lday>\d{1,7})\.)?(?:(?<lhour>2[0-3]|[0-1]\d|\d):)?(?:(?<lminute>[0-5]\d|\d):))?(?<lsecond>[0-5]\d|\d)(?:(?:\.)?(?<lmillisecond>\d{1,7}))?)[^.:\d]+?(?<righttime>(?:(?:(?<rday>[0-5]\d|\d)\.)?(?:(?<rhour>2[0-3]|[0-1]\d|\d):)?(?:(?<rminute>[0-5]\d|\d):))?(?<rsecond>[0-5]\d|\d))(?:(?:\.)?(?<rmillisecond>\d{1,7}))?$");
            if (regex.IsMatch(str))
            {
                var groups = regex.Match(str).Groups;
                int.TryParse(groups["lday"]?.Value, out var lday);
                int.TryParse(groups["lhour"]?.Value, out var lhour);
                int.TryParse(groups["lminute"]?.Value, out var lminute);
                int.TryParse(groups["lsecond"]?.Value, out var lsecond);
                int.TryParse(groups["lmillisecond"]?.Value, out var lmillisecond);
                int.TryParse(groups["rday"]?.Value, out var rday);
                int.TryParse(groups["rhour"]?.Value, out var rhour);
                int.TryParse(groups["rminute"]?.Value, out var rminute);
                int.TryParse(groups["rsecond"]?.Value, out var rsecond);
                int.TryParse(groups["rmillisecond"]?.Value, out var rmillisecond);
                left = new TimeSpan(lday, lhour, lminute, lsecond, lmillisecond);
                right = new TimeSpan(rday, rhour, rminute, rsecond, rmillisecond);
                if (left <= right) return true;

            }
            return false;
        }

        #endregion

        #region 时间转换
        /// <summary>
        /// 时间间隔转换为中文格式 <paramref name="duration"/>.Days 天 <paramref name="duration"/>.Hours 小时 <paramref name="duration"/>.Minutes 分 <paramref name="duration"/>.Seconds 秒
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static string ToZhFormatString(this TimeSpan duration)
        {
            var days = duration.Days;
            var hours = duration.Hours;
            var minutes = duration.Minutes;
            var seconds = duration.Seconds;
            var milliseconds = duration.Milliseconds;
            return days.BeIfNotDefault($"{days}天")
                   + hours.BeIfNotDefault($"{hours}小时")
                   + minutes.BeIfNotDefault($"{minutes}分钟")
                   + (milliseconds.BeIfNotDefault($"{seconds + milliseconds / 1000.0}秒") ?? seconds.BeIfNotDefault($"{seconds}秒"));
        }
        /// <summary>
        /// 使用所有Kou支持的单位获取时间；（支持中文）（纯数字默认为s）(若是时间间隔会自动转换为当前时间+时间间隔)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="dateTime"></param>
        /// <param name="kouType"></param>
        /// <returns></returns>
        public static bool TryToDateTime(string str, out DateTime dateTime, bool kouType = true)
        {
            dateTime = DateTime.Now;
            if (str.Length > 1000) return false;//这么大基本是来找麻烦的
            var success = false;
            if (str.IsNullOrWhiteSpace()) return false;

            if (!kouType && DateTime.TryParse(str, out var officialDateTime))
            {
                dateTime = officialDateTime;
                return true;
            }
            if (DateTime.TryParse(str, out var officialDateTime2)) //使用日期格式尝试转换，相对于当前日期
            {
                dateTime = officialDateTime2;
                return true;
            }
            if (TryToTimeSpan(str, out var timeSpan, kouType))
            {
                dateTime = DateTime.Now + timeSpan;
                return true;
            }
            
            return success;
        }

        /// <summary>
        /// 使用所有Kou支持的单位获取时间间隔；（支持中文）（纯数字默认为秒）(若是日期会自动转换为距离当前时间的时间间隔)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="timeSpan"></param>
        /// <param name="kouType"></param>
        /// <returns></returns>
        public static bool TryToTimeSpan(string str, out TimeSpan timeSpan, bool kouType = true)
        {
            timeSpan = default;
            if (str.Length > 1000) return false;//这么大基本是来找麻烦的
            var success = false;
            timeSpan = new TimeSpan();
            if (str.IsNullOrWhiteSpace()) return false;
            if (!kouType) return TimeSpan.TryParse(str, out timeSpan);
            if (ZhNumber.IsContainZhNumber(str)) str = ZhNumber.ToArabicNumber(str);
            
            if (TryGetDateTimeFromZhDescription2(str, out var date2))
            {
                timeSpan = date2 - DateTime.Now;
                return true;
            }
            if (TryGetDateTimeFromZhDescription(str, out var date))
            {
                timeSpan = date - DateTime.Now;
                return true;
            }
            if (str.IsMatch(@"^\d+$") && int.TryParse(str, out var second))
            {
                timeSpan = new TimeSpan(0, 0, second);
                return true;
            }
            if (TryGetTimeSpan(str, out var timeSpanFormal, false))//2:00这种是指2分钟
            {
                timeSpan += timeSpanFormal;
                return true;
            }
            if (TryGetTimeSpanFromStr(str, out var timeSpanModern, out var parsedStr))
            {
                str = parsedStr;
                timeSpan += timeSpanModern;
                success = true;
            }

            if (TryGetTimeSpanFromAncientStr(str, out var timeSpanAncient, out var parsedStr2))
            {
                str = parsedStr2;
                timeSpan += timeSpanAncient;
                success = true;
            }
            if (DateTime.TryParse(str, out var dateTime)) //使用日期格式尝试转换，相对于当前日期
            {
                timeSpan = dateTime - DateTime.Now;
                return true;
            }
            return success;
        }

        

        #endregion

    }
}
