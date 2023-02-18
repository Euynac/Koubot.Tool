using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Koubot.Tool.Extensions;
using Koubot.Tool.String;

namespace Koubot.Tool.KouData;

internal class KouTimeType
{
    [Flags]
    enum Kind
    {
        None,
        ZhDescription1 = 1<<0,
        ZhDescription2 = 1<<1,
        Formal = 1<<2,
        AncientStr = 1 <<3,
        Normal = 1 << 4,
    }


    public static bool TryGetDateTimeFromZhDescription2(string str, out DateTime dateTime)
    {
        dateTime = default;
        var regex = new Regex(@"((?<day>大前|前|昨|今|当|明|后|大后)(?:日|天)(的)?(?<now>(这个时候|这时|现在))?)|(?<now2>(这个时候|这时|现在))");
        if (!regex.IsMatch(str)) return false;
        var match = regex.Match(str);
        var day = match.Groups["day"].Value;
        var isSameAsNow = match.Groups["now"].Value != "";
        var dayAwayFromNow = 0;//距离今天的天数
        var now = DateTime.Now;
        var hour = isSameAsNow ? now.Hour : 0;
        var minute = isSameAsNow ? now.Minute : 0;
        var second = isSameAsNow ? now.Second : 0;
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
        dateTime = new DateTime(now.Year, now.Month, now.Day + dayAwayFromNow, hour, minute, second); ;
        return true;
    }
    /// <summary>
    /// 获取中文描述的时间获取相对于现在的时间（例：明天上午13点15分）
    /// </summary>
    /// <param name="str"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static bool TryGetDateTimeFromZhDescription(string str, out DateTime dateTime)
    {
        dateTime = new DateTime();
        if (str.IsNullOrWhiteSpace()) return false;
        var now = DateTime.Now;
        var baseMonth =  now.Month;
        var baseDay = now.Day;
        
        if (str.MatchOnceThenReplace(@"月底", out str, out _))
        {
            baseDay = DateTime.DaysInMonth(baseMonth, baseDay);
        }
        else if (str.MatchOnceThenReplace(@"(\d+)[号日]", out str, out var group))
        {
            baseDay = int.Parse(group[1].Value);
        }


        var regex = new Regex(@"(?:(?<day>大前|前|昨|今|当|明|后|大后)(?:日|天))?(?<period>早上|上午|下午|晚上)?(?<hour>\d{1,2})[:点]过?(?<minute>\d{1,2})?[:分]?(?:(?<second>\d{1,7})秒?)?(?<period2>[pa]\.?m\.?)?");
        if (!regex.IsMatch(str)) return false;
        var match = regex.Match(str);
        bool? isAM = null;//为null是24小时制，否则true为AM，false为PM
        var dayAwayFromNow = 0;//距离今天的天数
        var day = match.Groups["day"].Value;
        var period = match.Groups["period"].Value;
        var hourStr = match.Groups["hour"].Value.BeIfNotEmpty("{0}", true) ?? "0";
        var minuteStr = match.Groups["minute"].Value.BeIfNotEmpty("{0}", true) ?? "0";
        var secondStr = match.Groups["second"].Value.BeIfNotEmpty("{0}", true) ?? "0";
        int minute = 0, second = 0;
        if (hourStr.IsNullOrEmpty() || !int.TryParse(hourStr, out var hour)) return false;
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
        
        dateTime = new DateTime(now.Year, baseMonth, baseDay + dayAwayFromNow, hour, minute, second); ;
        return true;
    }
    private static readonly List<string> _ancientTimePatternList = new()
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
    private static readonly List<string> _modernTimePatternList = new()
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
    /// <summary>
    /// 使用古代格式的字符尝试转换为TimeSpan格式的时间间隔
    /// </summary>
    /// <param name="str">支持格式为 旬10天[旬]；候5天[候]；须臾48分钟[须臾]；昼夜24小时[昼夜]；2小时[更|鼓|时辰]；30分钟[炷香|顿饭]；15分[刻|盏茶]；144秒[罗预]；7200毫秒[弹指]；360毫秒[瞬]；18毫秒[念|刹那]；</param>
    /// <param name="timeSpan"></param>
    /// <param name="parsedStr"></param>
    /// <returns></returns>
    public static bool TryGetTimeSpanFromAncientStr(string str, out TimeSpan timeSpan, out string parsedStr)
    {
        timeSpan = new TimeSpan();
        parsedStr = str;
        if (parsedStr.IsNullOrWhiteSpace()) return false;
        if (ZhNumber.IsContainZhNumber(parsedStr)) parsedStr = ZhNumber.ToArabicNumber(parsedStr);

        double day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;
        var success = false;//指示是否成功转换过一次
        for (var i = 0; i < _ancientTimePatternList.Count; i++)
        {
            var regex = new Regex(_ancientTimePatternList[i]);
            var timeStr = regex.Match(parsedStr).Value;
            if (timeStr.IsNullOrEmpty() || !double.TryParse(timeStr.Match(@"\d+(\.\d+)?"), out var num)) continue;
            parsedStr = regex.Replace(parsedStr, "", 1);
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
    /// <param name="parsedStr">支持格式为世纪(century)[c|世纪]；年数[y|年]；季[季]；月数[M|月]；周数[w|周]；天数[d|天|日]；小时数[h|小时|时]；分钟数[m|分]；秒数[s|秒]；毫秒数(millisecond)[ms|毫秒]；</param>
    /// <param name="timeSpan"></param>
    /// <param name="str">被处理后的str</param>
    /// <returns></returns>
    public static bool TryGetTimeSpanFromStr(string str, out TimeSpan timeSpan, out string parsedStr)
    {
        timeSpan = new TimeSpan();
        parsedStr = str;
        if (parsedStr.IsNullOrWhiteSpace()) return false;
        if (ZhNumber.IsContainZhNumber(parsedStr)) parsedStr = ZhNumber.ToArabicNumber(parsedStr);
        parsedStr = parsedStr.Replace("个", "");
        long day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;
        var success = false;//指示是否成功转换过一次
        for (var i = 0; i < _modernTimePatternList.Count; i++)
        {
            var regex = new Regex(_modernTimePatternList[i]);
            var timeStr = regex.Match(parsedStr).Value;
            if (timeStr.IsNullOrEmpty() || !double.TryParse(timeStr.Match(@"\d+(\.\d+)?"), out var num)) continue;
            parsedStr = regex.Replace(parsedStr, "", 1);
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
        var regex = new Regex(@"(?<time>(?:(?:(?<day>\d{1,7})\.)?(?:(?<hour>\d{1,7}):)?(?:(?<minute>\d{1,7}):))(?<second>\d{1,7})(?:(?:\.)(?<millisecond>\d{1,7}))?)");
        if (regex.IsMatch(str))
        {
            var groups = regex.Match(str).Groups;
            int.TryParse(groups["day"]?.Value, out var day);
            int.TryParse(groups["hour"]?.Value, out var hour);
            int.TryParse(groups["minute"]?.Value, out var minute);
            int.TryParse(groups["second"]?.Value, out var second);
            int.TryParse(groups["millisecond"]?.Value, out var millisecond);
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
    public static bool TryGetTimeSpan(string str, out TimeSpan timeSpan, bool isStrict = true)
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
            int.TryParse(groups["day"]?.Value, out var day);
            int.TryParse(groups["hour"]?.Value, out var hour);
            int.TryParse(groups["minute"]?.Value, out var minute);
            int.TryParse(groups["second"]?.Value, out var second);
            int.TryParse(groups["millisecond"]?.Value, out var millisecond);
            timeSpan = new TimeSpan(day, hour, minute, second, millisecond);
            return true;
        }
        return false;
    }
}