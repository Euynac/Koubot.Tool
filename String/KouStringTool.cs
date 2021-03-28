using JetBrains.Annotations;
using Koubot.Tool.Expand;
using Koubot.Tool.KouData;
using Koubot.Tool.Math;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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

        #region 格式化
        /// <summary>
        /// 如果列表中含有指定的元素时则返回给定的customFormat否则返回null。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="customFormat"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string ContainsReturnCustomOrNull<T>(this IEnumerable<T> list, T item,
            string customFormat)
            => list != null && list.Contains(item) ? customFormat : null;

        /// <summary>
        /// 使用特定的分割字符串合并一个Enumerable中的所有元素的字符串形式
        /// （本质是string.Join方法）
        /// </summary>
        /// <param name="values"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ToIListString<T>([CanBeNull] this IEnumerable<T> values, string separator) =>
            values == null ? string.Empty : string.Join(separator, values);
        /// <summary>
        /// 智能拼接
        /// </summary>
        /// <param name="mainStr">其中有$0的将会自动将secondStr拼接进去，否则是mainStr在前secondStr在后拼接</param>
        /// <param name="secondStr">要拼接到主字符串中$0或后面的字符串</param>
        /// <returns></returns>
        public static string SmartConcat(this string mainStr, string secondStr)
        {
            if (mainStr == null || secondStr == null || !mainStr.Contains("$0")) return mainStr + secondStr;
            return mainStr.Replace("$0", secondStr);
        }

        /// <summary>
        /// 智能拼接（必定按照$num来拼接，不会自动向后拼接。$num如果要跳过一个可以赋值null）
        /// </summary>
        /// <param name="mainStr">$0的将会自动将整个secondStr拼接进去，$1拼接第一个，$2拼接第二个，最多九个以此类推</param>
        /// <param name="notParse">不处理第几号元素，一般用于嵌套使用</param>
        /// <param name="numStr">要拼接到主字符串中$num的字符串，按顺序，最多9个</param>
        /// <returns></returns>
        public static string SmartConcat(this string mainStr, int? notParse = null, params string[] numStr)
        {
            Regex regex = new Regex(@"\$(?<index>\d)");
            if (mainStr == null || numStr == null || !regex.IsMatch(mainStr)) return mainStr;
            foreach (Match match in regex.Matches(mainStr))
            {
                string replaceStr = "";
                int secondStrCount = numStr.Length;
                if (int.TryParse(match.Groups["index"].Value, out int index))
                {
                    if (index == notParse) continue;
                    switch (index)
                    {
                        case < 0:
                            continue;
                        case 0:
                            replaceStr = numStr.ToIListString("");
                            break;
                        default:
                            {
                                if (index <= secondStrCount)
                                {
                                    replaceStr = numStr[index - 1];
                                }

                                break;
                            }
                    }
                }

                mainStr = regex.Replace(mainStr, replaceStr, 1);
            }

            return mainStr;
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
                if (NumberConvertor.WebUnitDouble(str, out string parsedStr))
                {
                    str = parsedStr;
                }

                ExpressionCalculator calculator = new ExpressionCalculator();//BUG 没有处理abc等非表达式情况，仍然会认为输入了正确的表达式
                if (str.Contains("上")) str = str.Replace("上", "");//乘上、加上等
                if (str.Contains("去")) str = str.Replace("去", "");
                str = str.ReplaceAllFromPairSet(KouStaticData.ZhMathToSymbolMath);
                var result = calculator.Calculate(str)?.ToString();
                if (result != null && result != double.NaN.ToString(CultureInfo.InvariantCulture))
                {
                    return double.TryParse(result, out doubleResult);
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
        /// 将字符串类型的数字转换为enum类型，支持KouEnumName标签特性别名枚举
        /// </summary>
        /// <param name="str"></param>
        /// <param name="enumResult"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryToEnum<T>(string str, out T enumResult) where T : struct, Enum
        {
            return KouEnumTool.TryGetKouEnum(str, out enumResult) ||
                   Enum.TryParse(str, true, out enumResult);
        }
        /// <summary>
        /// 将字符串类型的数字转换为enum类型，支持KouEnumName标签特性别名枚举
        /// </summary>
        /// <param name="str"></param>
        /// <param name="enumType"></param>
        /// <param name="enumResult"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryToEnum(string str, Type enumType, out object enumResult)
        {
            if (KouEnumTool.TryGetKouEnum(enumType, str, out enumResult)) return true;
            try
            {
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
        public static string ToEnPunctuation([CanBeNull] this string str)
        {
            if (str.IsNullOrWhiteSpace()) return str;
            return str.IsInStringSet(KouStaticData.ZhToEnPunctuationDict.Keys)
                ? str.ReplaceAllFromPairSet(KouStaticData.ZhToEnPunctuationDict)
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
            Regex regex = new Regex(@"^(?<lefttime>(?:(?:(?<lday>\d{1,7})\.)?(?:(?<lhour>2[0-3]|[0-1]\d|\d):)?(?:(?<lminute>[0-5]\d|\d):))?(?<lsecond>[0-5]\d|\d)(?:(?:\.)?(?<lmillisecond>\d{1,7}))?)[^.:\d]+?(?<righttime>(?:(?:(?<rday>[0-5]\d|\d)\.)?(?:(?<rhour>2[0-3]|[0-1]\d|\d):)?(?:(?<rminute>[0-5]\d|\d):))?(?<rsecond>[0-5]\d|\d))(?:(?:\.)?(?<rmillisecond>\d{1,7}))?$");
            if (regex.IsMatch(str))
            {
                var groups = regex.Match(str).Groups;
                int.TryParse(groups["lday"]?.Value, out int lday);
                int.TryParse(groups["lhour"]?.Value, out int lhour);
                int.TryParse(groups["lminute"]?.Value, out int lminute);
                int.TryParse(groups["lsecond"]?.Value, out int lsecond);
                int.TryParse(groups["lmillisecond"]?.Value, out int lmillisecond);
                int.TryParse(groups["rday"]?.Value, out int rday);
                int.TryParse(groups["rhour"]?.Value, out int rhour);
                int.TryParse(groups["rminute"]?.Value, out int rminute);
                int.TryParse(groups["rsecond"]?.Value, out int rsecond);
                int.TryParse(groups["rmillisecond"]?.Value, out int rmillisecond);
                left = new TimeSpan(lday, lhour, lminute, lsecond, lmillisecond);
                right = new TimeSpan(rday, rhour, rminute, rsecond, rmillisecond);
                if (left <= right) return true;

            }
            return false;
        }

        #endregion

        #region 时间转换
        /// <summary>
        /// 时间间隔转换为中文格式
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string ToZhFormatString(this TimeSpan timeSpan)
        {
            int days = timeSpan.Days;
            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;
            int milliseconds = timeSpan.Milliseconds;
            return days.BeIfNotDefault($"{days}天")
                   + hours.BeIfNotDefault($"{hours}小时")
                   + minutes.BeIfNotDefault($"{minutes}分钟")
                   + (milliseconds.BeIfNotDefault($"{seconds + milliseconds / 1000.0}秒") ?? seconds.BeIfNotDefault($"{seconds}秒"));
        }

        /// <summary>
        /// 使用所有Kou支持的单位获取时间间隔；（支持中文）（纯数字默认为s）(若是日期会自动转换为距离当前时间的时间间隔)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="timeSpan"></param>
        /// <param name="kouType"></param>
        /// <returns></returns>
        public static bool TryToTimeSpan(string str, out TimeSpan timeSpan, bool kouType = true)
        {
            if (str.Length > 1000) return false;//这么大基本是来找麻烦的
            bool success = false;
            timeSpan = new TimeSpan();
            if (str.IsNullOrWhiteSpace()) return false;
            if (!kouType) return TimeSpan.TryParse(str, out timeSpan);
            if (ZhNumber.IsContainZhNumber(str)) str = ZhNumber.ToArabicNumber(str);
            if (str.TryGetTimeSpan(out TimeSpan timeSpanFormal, false))//2:00这种是指2分钟
            {
                timeSpan += timeSpanFormal;
                return true;
            }
            if (TryGetTimeSpanFromZhDescription(str, out timeSpan)) return true;
            if (DateTime.TryParse(str, out DateTime dateTime)) //使用日期格式尝试转换，相对于当前日期
            {
                timeSpan = dateTime - DateTime.Now;
                return true;
            }
            if (str.IsMatch(@"^\d+$") && int.TryParse(str, out int second))
            {
                timeSpan = new TimeSpan(0, 0, second);
                return true;
            }
            if (TryGetTimeSpanFromStr(str, out TimeSpan timeSpanModern))
            {
                timeSpan += timeSpanModern;
                success = true;
            }

            if (TryGetTimeSpanFromAncientStr(str, out TimeSpan timeSpanAncient))
            {
                timeSpan += timeSpanAncient;
                success = true;
            }
            return success;
        }

        /// <summary>
        /// 获取中文描述的时间获取相对于现在的时间间隔（例：明天上午13点15分）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static bool TryGetTimeSpanFromZhDescription(string str, out TimeSpan timeSpan)
        {
            if (str.IsNullOrWhiteSpace()) return false;
            Regex regex = new Regex(@"(?:(?<day>大前|前|昨|今|当|明|后|大后)(?:日|天))?(?<period>早上|上午|下午|晚上)?(?<hour>\d{1,2})[:点]过?(?<minute>\d{1,2})?[:分]?(?:(?<second>\d{1,7})秒?)?(?<period2>[pa]\.?m\.?)?");
            if (!regex.IsMatch(str)) return false;
            var match = regex.Match(str);
            bool? isAM = null;//为null是24小时制，否则true为AM，false为PM
            int dayAwayFromNow = 0;//距离今天的天数
            var day = match.Groups["day"]?.Value;
            var period = match.Groups["period"]?.Value;
            var hourStr = match.Groups["hour"]?.Value;
            var minuteStr = match.Groups["minute"]?.Value;
            var secondStr = match.Groups["second"]?.Value;
            int minute = 0, second = 0;
            if (hourStr.IsNullOrEmpty() || !int.TryParse(hourStr, out int hour)) return false;
            if (!minuteStr.IsNullOrEmpty() && !int.TryParse(minuteStr, out minute)) return false;
            if (!secondStr.IsNullOrEmpty() && !int.TryParse(secondStr, out second)) return false;
            var period2 = match.Groups["period2"]?.Value;
            if (!day.IsNullOrEmpty())
            {
                dayAwayFromNow = day switch
                {
                    "大前" => -3,
                    "前" => -2,
                    "昨" => -1,
                    "今" => 0,
                    "当" => 0,
                    "明" => 1,
                    "后" => 2,
                    "大后" => 3,
                    _ => dayAwayFromNow
                };
            }

            if (!period.IsNullOrEmpty())
            {
                isAM = period switch
                {
                    "早上" => true,
                    "上午" => true,
                    "下午" => false,
                    "晚上" => false,
                    _ => false
                };
            }

            if (!period2.IsNullOrEmpty())
            {
                isAM = period2.StartsWith("a");
            }

            if (isAM == false)//是下午
            {
                hour += 12;
            }
            var after = DateTime.Now;
            after = after.AddDays(dayAwayFromNow);
            after = after.AddHours(0 - after.Hour + hour);
            after = after.AddMinutes(0 - after.Minute + minute);
            after = after.AddSeconds(0 - after.Second + second);
            timeSpan = after - DateTime.Now;
            return true;
        }

        /// <summary>
        /// 使用古代格式的字符尝试转换为TimeSpan格式的时间间隔
        /// </summary>
        /// <param name="str">支持格式为 旬10天[旬]；候5天[候]；须臾48分钟[须臾]；昼夜24小时[昼夜]；2小时[更|鼓|时辰]；30分钟[炷香|顿饭]；15分[刻|盏茶]；144秒[罗预]；7200毫秒[弹指]；360毫秒[瞬]；18毫秒[念|刹那]；</param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static bool TryGetTimeSpanFromAncientStr(string str, out TimeSpan timeSpan)
        {
            timeSpan = new TimeSpan();
            if (str.IsNullOrWhiteSpace()) return false;
            if (ZhNumber.IsContainZhNumber(str)) str = ZhNumber.ToArabicNumber(str);
            List<string> patternList = new List<string>()
            {
                @"\d+(\.\d+)?(旬)",//0
                @"\d+(\.\d+)?(候)",//1
                @"\d+(\.\d+)?(昼夜)",//2
                @"\d+(\.\d+)?(须臾)",//3
                @"\d+(\.\d+)?(更|鼓|时辰)",//4
                @"\d+(\.\d+)?(柱香|炷香|顿饭)",//5 
                @"\d+(\.\d+)?(刻|盏茶)",//6
                @"\d+(\.\d+)?(罗预)",//7
                @"\d+(\.\d+)?(弹指)",//8
                @"\d+(\.\d+)?(瞬)",//9
                @"\d+(\.\d+)?(念|刹那)",//10
            };
            double day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;
            bool success = false;//指示是否成功转换过一次
            for (int i = 0; i < patternList.Count; i++)
            {
                var timeStr = str.Match(patternList[i]);
                if (timeStr.IsNullOrEmpty() || !double.TryParse(timeStr.Match(@"\d+(\.\d+)?"), out double num)) continue;
                success = true;
                switch (i)
                {
                    //转化为天
                    case 0://旬
                        day += (num *= 10) > long.MaxValue ? 0 : Convert.ToInt64(num);//Convert.ToInt64可以四舍六入五取偶
                        break;
                    case 1://候
                        day += (num *= 5) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 2://昼夜
                        hour += (num *= 24) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    //转化为分钟
                    case 3://须臾
                        minute += (num *= 48) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 4://更|鼓|时辰
                        minute += (num *= 120) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 5://炷香|顿饭
                        minute += (num *= 30) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 6://刻|盏茶
                        minute += (num *= 15) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    //转化为秒：
                    case 7://罗预
                        second += (num *= 144) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 8://弹指
                        millisecond += (num *= 7200) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 9://瞬
                        millisecond += (num *= 360) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 10://念|刹那
                        millisecond += (num *= 18) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    default:
                        break;
                }
            }
            if (!success) return false;

            try
            {
                timeSpan = new TimeSpan((int)day, (int)hour, (int)minute, (int)second, (int)millisecond);
            }
            catch (Exception)
            {
                timeSpan = TimeSpan.MaxValue;//10675199.02:48:05.4775807
            }
            return true;
        }

        /// <summary>
        /// 使用现代时间计数格式尝试转换为TimeSpan格式的时间间隔
        /// </summary>
        /// <param name="str">支持格式为世纪(century)[c|世纪]；年数[y|年|]；季[季]；月数[M|月]；周数[w|周]；天数[d|天|日]；小时数[h|小时|时]；分钟数[m|分]；秒数[s|秒]；毫秒数(millisecond)[ms|毫秒]；</param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static bool TryGetTimeSpanFromStr(string str, out TimeSpan timeSpan)
        {
            timeSpan = new TimeSpan();
            if (str.IsNullOrWhiteSpace()) return false;
            if (ZhNumber.IsContainZhNumber(str)) str = ZhNumber.ToArabicNumber(str);
            str = str.Replace("个", "");
            List<string> patternList = new List<string>()
            {
                @"\d+(\.\d+)?(c|世纪)",//0
                @"\d+(\.\d+)?(y|年)",//1
                @"\d+(\.\d+)?(季)",//2
                @"\d+(\.\d+)?(M|月)",//3
                @"\d+(\.\d+)?(w|周)",//4
                @"\d+(\.\d+)?(d|天|日)",//5 
                @"\d+(\.\d+)?(h|小时|时(?!辰))",//6
                @"\d+(\.\d+)?(m(?!s)|分)",//7
                @"\d+(\.\d+)?(s|秒)",//8
                @"\d+(\.\d+)?(ms|毫秒)",//9
            };
            long day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;
            bool success = false;//指示是否成功转换过一次
            for (int i = 0; i < patternList.Count; i++)
            {
                var timeStr = str.Match(patternList[i]);
                if (timeStr.IsNullOrEmpty() || !double.TryParse(timeStr.Match(@"\d+(\.\d+)?"), out double num)) continue;
                success = true;
                switch (i)
                {
                    //转化为天
                    case 0://世纪
                        day += (num *= 36500) > long.MaxValue ? 0 : Convert.ToInt64(num);//Convert.ToInt32可以四舍六入五取偶
                        break;
                    case 1://年
                        day += (num *= 365) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 2://季
                        day += (num *= 91.25) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    //转化为小时
                    case 3://月
                        hour += (num *= 730) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 4://周
                        hour += (num *= 168) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    //转化为分钟
                    case 5://天
                        minute += (num *= 1440) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    //转化为秒
                    case 6://时
                        second += (num *= 3600) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 7://分
                        second += (num *= 60) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    //转化为毫秒：
                    case 8://秒
                        millisecond += (num *= 1000) > long.MaxValue ? 0 : Convert.ToInt64(num);
                        break;
                    case 9://毫秒
                        millisecond += Convert.ToInt64(num);
                        break;
                    default:
                        break;
                }
            }
            //继续突破最高限制
            if (millisecond > int.MaxValue)
            {
                second += (millisecond - int.MaxValue) / 1000;
                millisecond = int.MaxValue;
            }
            if (second > int.MaxValue)
            {
                minute += (second - int.MaxValue) / 60;
                second = int.MaxValue;
            }
            if (minute > int.MaxValue)
            {
                hour += (minute - int.MaxValue) / 60;
                minute = int.MaxValue;
            }
            if (hour > int.MaxValue)
            {
                day += (hour - int.MaxValue) / 24;
                hour = int.MaxValue;
            }
            if (!success) return false;
            try
            {
                timeSpan = new TimeSpan((int)day, (int)hour, (int)minute, (int)second, (int)millisecond);
            }
            catch (Exception)
            {
                timeSpan = TimeSpan.MaxValue;
            }
            return true;
        }

        /// <summary>
        /// 使用正式时间计数格式尝试转换为TimeSpan格式的时间间隔
        /// </summary>
        /// <param name="str">格式为[7位天数.][7位小时:]7位分钟:7位秒[.7位毫秒数]</param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static bool TryGetTimeSpanFormal(string str, out TimeSpan timeSpan)
        {
            timeSpan = new TimeSpan();
            if (str.IsNullOrWhiteSpace()) return false;
            Regex regex = new Regex(@"(?<time>(?:(?:(?<day>\d{1,7})\.)?(?:(?<hour>\d{1,7}):)?(?:(?<minute>\d{1,7}):))(?<second>\d{1,7})(?:(?:\.)(?<millisecond>\d{1,7}))?)");
            if (regex.IsMatch(str))
            {
                var groups = regex.Match(str).Groups;
                int.TryParse(groups["day"]?.Value, out int day);
                int.TryParse(groups["hour"]?.Value, out int hour);
                int.TryParse(groups["minute"]?.Value, out int minute);
                int.TryParse(groups["second"]?.Value, out int second);
                int.TryParse(groups["millisecond"]?.Value, out int millisecond);
                timeSpan = new TimeSpan(day, hour, minute, second, millisecond);
                return true;
            }
            return false;
        }


        /// <summary>
        /// 尝试转换为TimeSpan格式的时间间隔
        /// </summary>
        /// <param name="str">格式为[7位天数.][7位小时:][7位分钟:]7位秒[.7位毫秒数]</param>
        /// <param name="timeSpan"></param>
        /// <param name="isStrict">是否严格匹配（即开头结尾不能有其他字符）</param>
        /// <returns></returns>
        public static bool TryGetTimeSpan(this string str, out TimeSpan timeSpan, bool isStrict = true)
        {
            timeSpan = new TimeSpan();
            if (str.IsNullOrWhiteSpace()) return false;
            Regex regex;
            if (isStrict)
            {
                str = str.Trim();
                regex = new Regex(@"^(?<time>(?:(?:(?<day>\d{1,7})\.)?(?:(?<hour>\d{1,7}):)?(?:(?<minute>\d{1,7}):))(?<second>\d{1,7})(?:(?:\.)(?<millisecond>\d{1,7}))?)$");
                //regex = new Regex(@"^(?<time>(?:(?:(?<day>\d{1,7})\.)?(?:(?<hour>2[0-3]|[0-1]\d|\d):)?(?:(?<minute>[0-5]\d|\d):))?(?<second>[0-5]\d|\d)(?:(?:\.)?(?<millisecond>\d{1,7}))?)$");//[7位天数.][00-23小时:][00-59分钟:]00-59秒[.7位毫秒数]
            }
            else
            {
                regex = new Regex(@"(?<time>(?:(?:(?<day>\d{1,7})\.)?(?:(?<hour>\d{1,7}):)?(?:(?<minute>\d{1,7}):))(?<second>\d{1,7})(?:(?:\.)(?<millisecond>\d{1,7}))?)");
            }
            if (regex.IsMatch(str))
            {
                var groups = regex.Match(str).Groups;
                int.TryParse(groups["day"]?.Value, out int day);
                int.TryParse(groups["hour"]?.Value, out int hour);
                int.TryParse(groups["minute"]?.Value, out int minute);
                int.TryParse(groups["second"]?.Value, out int second);
                int.TryParse(groups["millisecond"]?.Value, out int millisecond);
                timeSpan = new TimeSpan(day, hour, minute, second, millisecond);
                return true;
            }
            return false;
        }

        #endregion

    }
}
