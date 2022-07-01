using System;
using Koubot.Tool.String;

namespace Koubot.Tool.Extensions
{
    /// <summary>
    /// Extension methods of Time related type
    /// </summary>
    public static class TimeExtensions
    {
        /// <summary>
        /// 时间戳（格林威治时间1970年01月01日00时00分00秒）类型
        /// </summary>
        public enum TimeStampType
        {
            /// <summary>
            /// 总秒数（10位）
            /// </summary>
            [KouEnumName("总秒数","unix","10位","秒")]
            Unix,
            /// <summary>
            /// 总毫秒数（13位）
            /// </summary>
            [KouEnumName("总毫秒数","javascript","js","13位","毫秒")]
            Javascript
        }
        #region 时间类拓展
        /// <summary>
        /// Get the time span of given date time to that next minute.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static TimeSpan NextMinuteSpan(this DateTime dateTime) =>
            dateTime - NextMinute(dateTime);
        /// <summary>
        /// Get the time span of given date time to that next hour.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static TimeSpan NextHourSpan(this DateTime dateTime) =>
            dateTime - NextMinute(dateTime);
        /// <summary>
        /// Get the time span of given date time to that next day 00:00.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static TimeSpan NextDaySpan(this DateTime dateTime) =>
            dateTime - NextDay(dateTime);
        /// <summary>
        /// Get the date time of given date time to that next minute.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime NextMinute(this DateTime dateTime)
        {
            var timeBase = dateTime.AddMinutes(1);
            return new DateTime(timeBase.Year, timeBase.Month, timeBase.Day, timeBase.Hour, timeBase.Minute, 0);
        }
        /// <summary>
        /// Get the date time of given date time to that next hour.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime NextHour(this DateTime dateTime)
        {
            var timeBase = dateTime.AddHours(1);
            return new DateTime(timeBase.Year, timeBase.Month, timeBase.Day, timeBase.Hour, 0, 0);
        }
        /// <summary>
        /// Get the date time of given date time to that next day 00:00.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime NextDay(this DateTime dateTime)
        {
            var timeBase = dateTime.AddDays(1);
            return new DateTime(timeBase.Year, timeBase.Month, timeBase.Day, 0, 0, 0);
        }

        /// <summary>
        /// 获取指定类型的时间戳的 <see cref="DateTime"/> 表示形式
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="timeStampType">指定类型，默认Unix（秒为单位）</param>
        /// <returns>注意是以本地时区为准的</returns>
        public static DateTime ToDateTime(this long timestamp, TimeStampType timeStampType = TimeStampType.Unix)
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            DateTime daTime = new DateTime();
            switch (timeStampType)
            {
                case TimeStampType.Unix:
                    daTime = startTime.AddSeconds(timestamp);
                    break;
                case TimeStampType.Javascript:
                    daTime = startTime.AddMilliseconds(timestamp);
                    break;
            }
            return daTime;
        }
        /// <summary>
        /// DateTime转时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeStampType"></param>
        /// <returns>注意是以本地时区为准的</returns>
        public static long ToTimeStamp(this DateTime dateTime, TimeStampType timeStampType = TimeStampType.Unix)
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            long timestamp = 0;
            switch (timeStampType)
            {
                case TimeStampType.Unix:
                    timestamp = (long)(dateTime - startTime).TotalSeconds;
                    break;
                case TimeStampType.Javascript:
                    timestamp = (long)(dateTime - startTime).TotalMilliseconds;
                    break;
            }
            return timestamp;
        }
        /// <summary>
        /// 转换为中国式星期几的表述（星期天为第七天）
        /// </summary>
        /// <param name="week"></param>
        /// <returns>1-7对应星期一到星期天</returns>
        public static ChineseWeeks ToChineseWeek(this DayOfWeek week) => week == DayOfWeek.Sunday ? ChineseWeeks.星期日 : (ChineseWeeks)week;

        #endregion
        /// <summary>
        /// Chinese week.
        /// </summary>
        public enum ChineseWeeks
        {
            星期一 = 1,
            星期二 = 2,
            星期三 = 3,
            星期四 = 4,
            星期五 = 5,
            星期六 = 6,
            星期日 = 7,
        }

        /// <summary>
        /// Round given time to second. (discard millisecond)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime RoundToSecond(this DateTime time) =>
            new(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);

        /// <summary>
        /// Combine given and time from given DateTime.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime CombineDateAndTime(this DateTime date, DateTime time) => 
            new(date.Year, date.Month , date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
    }
}