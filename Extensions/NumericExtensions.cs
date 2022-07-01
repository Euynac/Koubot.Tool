namespace Koubot.Tool.Extensions
{
    /// <summary>
    /// Extension methods of Numeric type
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="max">最大值，超过则取这个最大值</param>
        /// <returns></returns>
        public static int LimitInRange(this int num, int max) => num > max ? max : num;

        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min">最小值，小于则取这个最小值</param>
        /// <param name="max">最大值，大于则取这个最大值</param>
        /// <returns></returns>
        public static int LimitInRange(this int num, int min, int max) => (num > max) ? max : (num < min) ? min : num;

        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="max">最大值，超过则取这个最大值</param>
        /// <returns></returns>
        public static long LimitInRange(this long num, long max) => num > max ? max : num;

        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min">最小值，小于则取这个最小值</param>
        /// <param name="max">最大值，大于则取这个最大值</param>
        /// <returns></returns>
        public static long LimitInRange(this long num, long min, long max) => num > max ? max : num < min ? min : num;

        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min">最小值，小于则取这个最小值</param>
        /// <param name="max">最大值，大于则取这个最大值</param>
        /// <returns></returns>
        public static long LimitInRange(this long num, long? min, long? max)
        {
            if (min == null)
            {
                return max == null ? num : num.LimitInRange(max.Value);
            }

            return max == null ? num < min ? min.Value : num : num.LimitInRange(min.Value, max.Value);
        }
        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min">最小值，小于则取这个最小值</param>
        /// <param name="max">最大值，大于则取这个最大值</param>
        /// <returns></returns>
        public static double LimitInRange(this double num, double min, double max) => (num > max) ? max : (num < min) ? min : num;

        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min">最小值，小于则取这个最小值</param>
        /// <param name="max">最大值，大于则取这个最大值</param>
        /// <returns></returns>
        public static double LimitInRange(this double num, double? min, double? max)
        {
            if (min == null)
            {
                return max == null ? num : num.LimitInRange(max.Value);
            }

            return max == null ? num < min ? min.Value : num : num.LimitInRange(min.Value, max.Value);
        }
        
        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min">最小值，小于则取这个最小值</param>
        /// <param name="max">最大值，大于则取这个最大值</param>
        /// <returns></returns>
        public static int LimitInRange(this int num, double? min, double? max)
        {
            if (min == null)
            {
                return max == null ? num : num > max ? (int)max.Value : num;
            }

            return max == null ? num < min ? (int)min.Value : num : num.LimitInRange((int)min.Value, (int)max.Value);
        }
        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min">最小值，小于则取这个最小值</param>
        /// <param name="max">最大值，大于则取这个最大值</param>
        /// <returns></returns>
        public static long LimitInRange(this long num, double? min, double? max)
        {
            if (min == null)
            {
                return max == null ? num : num > max ? (long)max.Value : num;
            }

            return max == null ? num < min ? (long)min.Value : num : num.LimitInRange((long)min.Value, (long)max.Value);
        }
        /// <summary>
        /// 将指定数字限定在特定范围内
        /// </summary>
        /// <param name="num"></param>
        /// <param name="max">最大值，超过则取这个最大值</param>
        /// <returns></returns>
        public static double LimitInRange(this double num, double max) => num > max ? max : num;
        /// <summary>
        /// Judge whether a double number is actually a integer.
        /// </summary>
        /// <param name="num"></param>
        /// <returns>Epsilon maybe up to 1-16e.</returns>
        public static bool IsInteger(this double num) => num % 1 == 0;
        /// <summary>
        /// 返回大于或等于指定double的最小整数值。
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int Ceiling(this double num) => (int)System.Math.Ceiling(num);
        /// <summary>
        /// 返回大于或等于指定float的最小整数值。
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int Ceiling(this float num) => (int)System.Math.Ceiling(num);
        /// <summary>
        /// Not same as P in string format which always retains 2 decimal places. It only retain decimal max to given <paramref name="maxRetain"/>. 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="maxRetain"></param>
        /// <returns></returns>
        public static string FormatPercentage(this double num, int maxRetain = 2) => (num * 100).ToString("0." + '#'.Repeat(maxRetain)) + '%';

        /// <summary>
        /// Not same as P in string format which always retains 2 decimal places. It only retain decimal max to given <paramref name="maxRetain"/>. 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="maxRetain"></param>
        /// <returns></returns>
        public static string FormatPercentage(this float num, int maxRetain = 2) =>
            FormatPercentage((double) num, maxRetain);
    }
}