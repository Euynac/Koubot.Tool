using Koubot.Tool.Expand;
using System;
using System.Text.RegularExpressions;

namespace Koubot.Tool.Math
{
    /// <summary>
    /// 数的类型
    /// </summary>
    public enum NumberType
    {
        /// <summary>
        /// 实数
        /// </summary>
        RealNumber,
        /// <summary>
        /// 无穷小
        /// </summary>
        Infinitesimal,
        /// <summary>
        /// 无穷大
        /// </summary>
        Infinity
    }
    /// <summary>
    /// 区间支持型Double，默认闭区间，使用区间用小于等于或大于等于（当是开区间的时候实际上不会取到等于）
    /// </summary>
    public class IntervalDouble
    {

        /// <summary>
        /// 数的类型
        /// </summary>
        public NumberType NumType { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// 是否是开区间
        /// </summary>
        public bool IsOpen { get; set; }

        public IntervalDouble(double value, bool isOpen = false, NumberType numberType = NumberType.RealNumber)
        {
            Value = value;
            IsOpen = isOpen;
            NumType = numberType;
        }

        public IntervalDouble(NumberType numberType)
        {
            NumType = numberType;
        }

        /// <summary>
        /// 指示是否是无穷数值（无穷大或无穷小）
        /// </summary>
        /// <param name="intervalDouble"></param>
        /// <returns></returns>
        public static bool IsInfinite(IntervalDouble intervalDouble)
        {
            return intervalDouble != null && intervalDouble.NumType != NumberType.RealNumber;
        }

        #region 公共方法
        /// <summary>
        /// 获取区间值（左到右）返回IntervalDouble类型，默认闭区间
        /// </summary>
        /// <param name="str"></param>
        /// <param name="intervalLeft">左区间 (、[</param>
        /// <param name="intervalRight">右区间 )、]</param>
        /// /// <param name="force">若是能转为double但无法转为区间，强制转换为区间（即left=right值）</param>
        /// <returns></returns>
        public static bool GetInterval(string str, out IntervalDouble intervalLeft, out IntervalDouble intervalRight, bool force = false)
        {
            intervalLeft = new IntervalDouble(0);
            intervalRight = new IntervalDouble(0);
            if (str.IsNullOrWhiteSpace()) return false;
            Regex regex = new Regex(@"[\[\(](-?\d*\.?\d+)?.+?(-?\d*\.?\d+)?[\]\)]");//匹配区间（浮点数以及负数支持）
            if (regex.IsMatch(str))
            {
                var groups = regex.Match(str).Groups;

                if (groups.Count == 3 && !groups[0].Value.IsNullOrWhiteSpace()) //第一组捕获组是全部
                {
                    if (double.TryParse(groups[1].Value, out double left))
                    {
                        if (groups[0].Value.StartsWith("(")) intervalLeft.IsOpen = true;
                        intervalLeft.Value = left;
                    }
                    else intervalLeft.NumType = NumberType.Infinitesimal;//无法判断或空则是认为是无穷小

                    if (double.TryParse(groups[2].Value, out double right))
                    {
                        if (groups[0].Value.EndsWith(")")) intervalRight.IsOpen = true;
                        intervalRight.Value = right;
                    }
                    else intervalRight.NumType = NumberType.Infinity;//无法判断或空则是认为是无穷大
                }

                if (intervalLeft > intervalRight) return false;
                return true;
            }
            else if (GetInterval(str, out double left, out double right))
            {
                intervalLeft.Value = left;
                intervalRight.Value = right;
                return true;
            }
            else if (force && double.TryParse(str, out double num))
            {
                intervalLeft.Value = intervalRight.Value = num;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取区间值（左到右）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="left">左值</param>
        /// <param name="right">右值</param>
        /// <param name="force">若是能转为double但无法转为区间，强制转换为区间（即left=right值）</param>
        /// <returns></returns>
        public static bool GetInterval(string str, out double left, out double right, bool force = false)
        {
            var result = str.Matches(@"(?<!\d)-?\d*\.?\d+");//支持匹配浮点数以及负数
            left = default;
            right = default;
            if (result == null || result.Length < 2)
            {
                if (force && Double.TryParse(str, out double num))
                {
                    left = right = num;
                    return true;
                }
                return false;
            }
            for (int i = 0; i < result.Length; i++)
            {
                if (i == 0)
                {
                    Double.TryParse(result[0], out left);
                }
                else if (i == 1)
                {
                    Double.TryParse(result[1], out right);
                }
                else break;
            }
            if (left > right) return false;
            return true;
        }
        public override string ToString()
        {
            return this.NumType switch
            {
                NumberType.Infinity => "+∞",
                NumberType.Infinitesimal => "-∞",
                _ => Value.ToString()
            };
        }
        #endregion

        #region 运算符重载 传入空值会报错
        public override bool Equals(object obj)
        {
            if (obj is IntervalDouble intervalDouble)
                return NumType == intervalDouble.NumType
                    && Value == intervalDouble.Value
                    && IsOpen == intervalDouble.IsOpen;
            return false;
        }
        public override int GetHashCode() //这里实验一下若是使用按照值计算hashcode是不是hashSet中就不能放多个一样的；若是按照地址计算又是不是可以放多个，虽然这违背了初衷
        {
            return base.GetHashCode();
        }
        public static bool operator >(IntervalDouble left, double right)
        {
            if (left.NumType == NumberType.Infinitesimal) return false;
            if (left.NumType == NumberType.Infinity) return true;
            return left.Value > right;
        }
        public static bool operator <(IntervalDouble left, double right)
        {
            if (left.NumType == NumberType.Infinity) return false;
            if (left.NumType == NumberType.Infinitesimal) return true;
            return left.Value < right;
        }
        public static bool operator <(double left, IntervalDouble right)
        {
            if (right.NumType == NumberType.Infinitesimal) return false;
            if (right.NumType == NumberType.Infinity) return true;
            return left < right.Value;
        }
        public static bool operator >(double left, IntervalDouble right)
        {
            if (right.NumType == NumberType.Infinity) return false;
            if (right.NumType == NumberType.Infinitesimal) return true;
            return left > right.Value;
        }

        public static bool operator >=(IntervalDouble left, double right)
        {
            if (left.NumType == NumberType.Infinitesimal) return false;
            if (left.NumType == NumberType.Infinity) return true;
            return left.IsOpen ? left.Value > right : left.Value >= right;
        }
        public static bool operator <=(IntervalDouble left, double right)
        {
            if (left.NumType == NumberType.Infinity) return false;
            if (left.NumType == NumberType.Infinitesimal) return true;
            return left.IsOpen ? left.Value < right : left.Value <= right;
        }
        public static bool operator <=(double left, IntervalDouble right)
        {
            if (right.NumType == NumberType.Infinitesimal) return false;
            if (right.NumType == NumberType.Infinity) return true;
            return right.IsOpen ? left < right.Value : left <= right.Value;
        }
        public static bool operator >=(double left, IntervalDouble right)
        {
            if (right.NumType == NumberType.Infinity) return false;
            if (right.NumType == NumberType.Infinitesimal) return true;
            return right.IsOpen ? left > right.Value : left >= right.Value;
        }


        public static bool operator >(IntervalDouble left, IntervalDouble right)
        {
            if (left.NumType == NumberType.Infinitesimal || right.NumType == NumberType.Infinity) return false;
            if ((left.NumType == NumberType.Infinity && right.NumType != NumberType.Infinity) || (right.NumType == NumberType.Infinitesimal && left.NumType != NumberType.Infinitesimal)) return true;
            return left.Value > right.Value;
        }

        public static bool operator <(IntervalDouble left, IntervalDouble right)
        {
            if (left.NumType == NumberType.Infinity || right.NumType == NumberType.Infinitesimal) return false;
            if ((right.NumType == NumberType.Infinity && left.NumType != NumberType.Infinity) || (left.NumType == NumberType.Infinitesimal && right.NumType != NumberType.Infinitesimal)) return true;
            return left.Value < right.Value;
        }
        public static bool operator ==(IntervalDouble left, IntervalDouble right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(IntervalDouble left, IntervalDouble right)
        {
            return !left.Equals(right);
        }
        public static bool operator >=(IntervalDouble left, IntervalDouble right)
        {
            return left.IsOpen ? (left.Value > right.Value) : (left.Value > right.Value || left.Value == right.Value);
        }
        public static bool operator <=(IntervalDouble left, IntervalDouble right)
        {
            return left.IsOpen ? (left.Value < right.Value) : (left.Value < right.Value || left.Value == right.Value);
        }
        #endregion
    }
    /// <summary>
    /// 区间Double型
    /// </summary>
    public static class IntervalDoubleTool
    {
        /// <summary>
        /// 获取区间值（左到右）返回IntervalDouble类型，默认闭区间
        /// </summary>
        /// <param name="str"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool TryGetInterval(this string str, out IntervalDouble intervalLeft, out IntervalDouble intervalRight, bool force = false)
        {
            return IntervalDouble.GetInterval(str, out intervalLeft, out intervalRight, force);
        }

        /// <summary>
        /// 获取区间值（左到右）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="left">左值</param>
        /// <param name="right">右值</param>
        /// <returns></returns>
        public static bool TryGetInterval(this string str, out double left, out double right, bool force = false)
        {
            return IntervalDouble.GetInterval(str, out left, out right, force);
        }
    }
}
