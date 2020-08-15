using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Koubot.Tool.Expand;
using Koubot.Tool.KouData;
using Koubot.Tool.Math;

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
        /// <returns>错误输入返回false，能够处理页数返回true</returns>
        public static bool TryParsePage(int currentPage, string userInput, out int parsedPage)
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

            return TryToInt(userInput, out parsedPage);
        }

        #endregion

        #region 格式化

        public static string ToIListString(this object obj, string splitedStr = ",")
        {
            if (obj is IList list)
            {
                string result = "";
                foreach (var item in list)
                {
                    result += item.BeNullOr(item + splitedStr);
                }

                return result;
            }

            return obj?.ToString();
        }
        #endregion

        #region KouType类型适配
        /// <summary>
        /// 转换为指定类型的List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryToIList<T>(string str, out List<T> resultList,
            string constraintPattern = @"[\s\S]+", int countConstraint = 0, bool allowDuplicate = false,
            string splitStr = ",;；，、\\\\", RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            resultList = null;
            if (TryToIList(str, out IList list, typeof(T), constraintPattern, countConstraint, allowDuplicate,
                splitStr, regexOptions))
            {
                resultList = (List<T>)list;
                return true;
            }

            return false;
        }
        /// <summary>
        /// 尝试将string转换为指定类型的IList
        /// </summary>
        /// <param name="str"></param>
        /// <param name="resultList"></param>
        /// <param name="listType"></param>
        public static bool TryToIList(string str, out IList resultList, Type listType,
            string constraintPattern = @"[\s\S]+", int countConstraint = 0, bool allowDuplicate = false,
            string splitStr = ",;；，、\\\\", RegexOptions regexOptions = RegexOptions.IgnoreCase, bool useKouType = true,
            bool onceFailReplyError = false)
        {
            resultList = null;
            if (!MultiSelectionHelper.TryGetMultiSelections(str, out List<string> list, constraintPattern,
                countConstraint,
                allowDuplicate
                , splitStr, regexOptions))
            {
                return false;
            }

            //是字符串类型的
            if (typeof(IList<string>).IsAssignableFrom(listType))
            {
                resultList = list;
                return true;
            }

            IList<int?> intList = null;
            IList<double?> doubleList = null;
            IList<bool?> boolList = null;
            IList<TimeSpan?> timeSpanList = null;
            bool nullable = false;
            //判断是哪个类型列表
            if (listType.SatisfyAny(typeof(IList<int>).IsAssignableFrom
                , typeof(IList<int?>).IsAssignableFrom))
                intList = new List<int?>();
            else if (listType.SatisfyAny(typeof(IList<double>).IsAssignableFrom
                , typeof(IList<double>).IsAssignableFrom))
                doubleList = new List<double?>();
            else if (listType.SatisfyAny(typeof(IList<bool>).IsAssignableFrom
                , typeof(IList<bool?>).IsAssignableFrom))
                boolList = new List<bool?>();
            else if (listType.SatisfyAny(typeof(IList<TimeSpan>).IsAssignableFrom
                , typeof(IList<TimeSpan?>).IsAssignableFrom))
                timeSpanList = new List<TimeSpan?>();
            //如果是可空类型
            if (listType.SatisfyAny(typeof(IList<int?>).IsAssignableFrom,
                typeof(IList<double?>).IsAssignableFrom, typeof(IList<bool?>).IsAssignableFrom,
                typeof(IList<TimeSpan?>).IsAssignableFrom))
                nullable = true;
            //进行转换
            foreach (var item in list)
            {
                if (intList != null)
                {
                    if (TryToInt(item, out int intResult,
                        useKouType))
                        intList.Add(intResult);
                    continue;
                }

                if (doubleList != null)
                {
                    if (TryToDouble(item, out double doubleResult,
                        useKouType))
                        doubleList.Add(doubleResult);
                    continue;
                }

                if (boolList != null)
                {
                    if (TryToBool(item, out bool boolResult,
                        useKouType))
                        boolList.Add(boolResult);
                    continue;
                }

                if (timeSpanList != null)
                {
                    if (TryToTimeSpan(item, out TimeSpan timeSpanResult,
                        useKouType))
                        timeSpanList.Add(timeSpanResult);
                    continue;
                }
            }

            if (intList != null)
            {
                resultList = nullable ? (IList)intList : intList.ConvertToNotNullable().ToList();
                return true;
            }

            if (doubleList != null)
            {
                resultList = nullable ? (IList)doubleList : doubleList.ConvertToNotNullable().ToList();
                return true;
            }

            if (boolList != null)
            {
                resultList = nullable ? (IList)boolList : boolList.ConvertToNotNullable().ToList();
                return true;
            }

            if (timeSpanList != null)
            {
                resultList = nullable ? (IList)timeSpanList : timeSpanList.ConvertToNotNullable().ToList();
                return true;
            }

            return false;
        }

        
        /// <summary>
        /// 将字符串类型的数字转换为bool类型，支持中文以及英文、数字
        /// </summary>
        /// <param name="str"></param>
        /// <param name="doubleResult"></param>
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
        /// <returns></returns>
        public static bool TryToDouble(string str, out double doubleResult, bool kouType = true)
        {
            doubleResult = 0;
            if (str.IsNullOrWhiteSpace()) return false;
            if (!kouType) return double.TryParse(str, out doubleResult);
            //如果有中文的数字，转为阿拉伯数字
            if (ZhNumber.IsContainZhNumber(str)) str = ZhNumber.ToArabicNumber(str);
            if (!Double.TryParse(str, NumberStyles.Float, null, out doubleResult))
            {
                if (NumberConvertor.WebUnitDouble(str, out doubleResult))
                {
                    // 将末尾无单位的那个数字取出来（比如10k500，则结果为500）
                    Double.TryParse(str.Match(@"\d+(\.\d+)?$"), NumberStyles.Float, null, out double tailNumber);
                    doubleResult += tailNumber;
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
        /// <returns></returns>
        public static bool TryToInt(string str, out int intResult, bool kouType = true)
        {
            var result = TryToDouble(str, out var doubleResult, kouType);
            intResult = (int)doubleResult;
            return result;
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
                Int32.TryParse(groups["lday"]?.Value, out int lday);
                Int32.TryParse(groups["lhour"]?.Value, out int lhour);
                Int32.TryParse(groups["lminute"]?.Value, out int lminute);
                Int32.TryParse(groups["lsecond"]?.Value, out int lsecond);
                Int32.TryParse(groups["lmillisecond"]?.Value, out int lmillisecond);
                Int32.TryParse(groups["rday"]?.Value, out int rday);
                Int32.TryParse(groups["rhour"]?.Value, out int rhour);
                Int32.TryParse(groups["rminute"]?.Value, out int rminute);
                Int32.TryParse(groups["rsecond"]?.Value, out int rsecond);
                Int32.TryParse(groups["rmillisecond"]?.Value, out int rmillisecond);
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
            return days.BeDefaultOr($"{days}天")
                   + hours.BeDefaultOr($"{hours}小时")
                   + minutes.BeDefaultOr($"{minutes}分钟")
                   + (milliseconds.BeDefaultOr($"{seconds + milliseconds / 1000.0}秒") ?? seconds.BeDefaultOr($"{seconds}秒"));
        }

        /// <summary>
        /// 使用所有Kou支持的单位获取时间间隔；（支持中文）（纯数字默认为s）(若是日期会自动转换为距离当前时间的时间间隔)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static bool TryToTimeSpan(string str, out TimeSpan timeSpan, bool kouType = true)
        {
            bool success = false;
            timeSpan = new TimeSpan();
            if (str.IsNullOrWhiteSpace()) return false;
            if (!kouType) return TimeSpan.TryParse(str, out timeSpan);
            if (ZhNumber.IsContainZhNumber(str)) str = ZhNumber.ToArabicNumber(str);
            if (DateTime.TryParse(str, out DateTime dateTime)) //使用日期格式尝试转换
            {
                timeSpan = dateTime - DateTime.Now;
                return true;
            }
            if (str.IsMatch(@"^\d+$") && int.TryParse(str, out int second)) { timeSpan = new TimeSpan(0, 0, second); return true; }
            if (str.TryGetTimeSpan(out TimeSpan timeSpanFormal, false))
            {
                timeSpan += timeSpanFormal;
                return true;
            }
            if (TryGetTimeSpanFromStr(str, out TimeSpan timeSpanModern)) { timeSpan += timeSpanModern; success = true; }
            if (TryGetTimeSpanFromAncientStr(str, out TimeSpan timeSpanAncient)) { timeSpan += timeSpanAncient; success = true; }
            return success;
        }

        /// <summary>
        /// 使用古代格式的字符尝试转换为TimeSpan格式的时间间隔
        /// </summary>
        /// <param name="str">支持格式为 旬10天[旬]；候5天[候]；须臾48分钟[须臾]；昼夜24小时[昼夜]；2小时[更|鼓|时辰]；30分钟[炷香|顿饭]；24分钟[点]；15分[刻|盏茶]；144秒[罗预]；7200毫秒[弹指]；360毫秒[瞬]；18毫秒[念|刹那]；</param>
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
                @"\d+(\.\d+)?(点)",//6
                @"\d+(\.\d+)?(刻|盏茶)",//7
                @"\d+(\.\d+)?(罗预)",//8
                @"\d+(\.\d+)?(弹指)",//9
                @"\d+(\.\d+)?(瞬)",//10
                @"\d+(\.\d+)?(念|刹那)",//11
            };
            int day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;
            bool success = false;//指示是否成功转换过一次
            for (int i = 0; i < patternList.Count; i++)
            {
                var timeStr = str.Match(patternList[i]);
                if (timeStr.IsNullOrEmpty() || !Double.TryParse(timeStr.Match(@"\d+(\.\d+)?"), out double num)) continue;
                success = true;
                switch (i)
                {
                    //转化为天
                    case 0://旬
                        day += (num *= 10) > 1000000 ? 0 : Convert.ToInt32(num);//Convert.ToInt32可以四舍六入五取偶
                        break;
                    case 1://候
                        day += (num *= 5) > 1000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 2://昼夜
                        hour += (num *= 24) > 1000000 ? 0 : Convert.ToInt32(num);
                        break;
                    //转化为分钟
                    case 3://须臾
                        minute += (num *= 48) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 4://更|鼓|时辰
                        minute += (num *= 120) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 5://炷香|顿饭
                        minute += (num *= 30) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 6://点
                        minute += (num *= 24) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 7://刻|盏茶
                        minute += (num *= 15) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    //转化为秒：
                    case 8://罗预
                        second += (num *= 144) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 9://弹指
                        millisecond += (num *= 7200) > 2100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 10://瞬
                        millisecond += (num *= 360) > 2100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 11://念|刹那
                        millisecond += (num *= 18) > 2100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    default:
                        break;
                }
            }
            if (success)
            {
                timeSpan = new TimeSpan(day, hour, minute, second, millisecond);
                return true;
            }
            return false;
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
            int day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;
            bool success = false;//指示是否成功转换过一次
            for (int i = 0; i < patternList.Count; i++)
            {
                var timeStr = str.Match(patternList[i]);
                if (timeStr.IsNullOrEmpty() || !Double.TryParse(timeStr.Match(@"\d+(\.\d+)?"), out double num)) continue;
                success = true;
                switch (i)
                {
                    //转化为天
                    case 0://世纪
                        day += (num *= 36500) > 1000000 ? 0 : Convert.ToInt32(num);//Convert.ToInt32可以四舍六入五取偶
                        break;
                    case 1://年
                        day += (num *= 365) > 1000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 2://季
                        day += (num *= 91.25) > 1000000 ? 0 : Convert.ToInt32(num);
                        break;
                    //转化为小时
                    case 3://月
                        hour += (num *= 730) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 4://周
                        hour += (num *= 168) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    //转化为分钟
                    case 5://天
                        minute += (num *= 1440) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    //转化为秒
                    case 6://时
                        second += (num *= 3600) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 7://分
                        second += (num *= 60) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    //转化为毫秒：
                    case 8://秒
                        millisecond += (num *= 1000) > 100000000 ? 0 : Convert.ToInt32(num);
                        break;
                    case 9://毫秒
                        millisecond += Convert.ToInt32(num);
                        break;
                    default:
                        break;
                }
            }

            if (!success) return false;
            timeSpan = new TimeSpan(day, hour, minute, second, millisecond);
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
                Int32.TryParse(groups["day"]?.Value, out int day);
                Int32.TryParse(groups["hour"]?.Value, out int hour);
                Int32.TryParse(groups["minute"]?.Value, out int minute);
                Int32.TryParse(groups["second"]?.Value, out int second);
                Int32.TryParse(groups["millisecond"]?.Value, out int millisecond);
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
                Int32.TryParse(groups["day"]?.Value, out int day);
                Int32.TryParse(groups["hour"]?.Value, out int hour);
                Int32.TryParse(groups["minute"]?.Value, out int minute);
                Int32.TryParse(groups["second"]?.Value, out int second);
                Int32.TryParse(groups["millisecond"]?.Value, out int millisecond);
                timeSpan = new TimeSpan(day, hour, minute, second, millisecond);
                return true;
            }
            return false;
        }

        #endregion
    }
}
