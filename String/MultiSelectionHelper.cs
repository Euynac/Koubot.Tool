using Koubot.Tool.Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Koubot.Tool.String
{
    /// <summary>
    /// Kou插件参数/功能多选帮助器
    /// </summary>
    public static class MultiSelectionHelper
    {
        /// <summary>
        /// 内容限制类型
        /// </summary>
        public enum ConstraintType
        {
            /// <summary>
            /// [\s\S]+无限制
            /// </summary>
            None,

            /// <summary>
            /// \d+ 仅数字
            /// </summary>
            Number,

            /// <summary>
            /// [a-zA-Z]+ 仅英文字母
            /// </summary>
            OnlyLetter,
        }

        /// <summary>
        /// 将字符串按指定逻辑分割为List返回（一般用于含“多选”特性的插件参数或功能）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="multiList">返回结果，单选也会返回一个Count=1的list</param>
        /// <param name="constraintType">自定义选择内容限制正则表达式（默认全字符）</param>
        /// <param name="allowDuplicate">允许重复项</param>
        /// <param name="splitStr">自定义分割字符</param>
        /// <param name="countConstraint">个数限制</param>
        /// <param name="regexOptions">正则表达式匹配设置</param>
        /// <returns>成功返回true且返回分割完后的list</returns>
        public static bool TryGetMultiSelections(this string str, out List<string> multiList,
            ConstraintType constraintType = ConstraintType.None, int countConstraint = 0, bool allowDuplicate = false,
            string splitStr = ",;；，、\\\\", RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            string constraintPattern;
            switch (constraintType)
            {
                case ConstraintType.Number:
                    constraintPattern = @"\d+";
                    break;
                case ConstraintType.OnlyLetter:
                    constraintPattern = @"[a-zA-Z]+";
                    break;
                default:
                    constraintPattern = @"[\s\S]+";
                    break;
            }

            return TryGetMultiSelections(str, out multiList, constraintPattern, countConstraint, allowDuplicate, splitStr, regexOptions);
        }

        /// <summary>
        /// 将字符串按指定逻辑分割为List返回（一般用于含“多选”特性的插件参数或功能）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="multiList">返回结果，单选也会返回一个Count=1的list</param>
        /// <param name="constraintPattern">自定义选择内容限制正则表达式（默认全字符）</param>
        /// <param name="allowDuplicate">允许重复项</param>
        /// <param name="splitStr">自定义分割字符</param>
        /// <param name="countConstraint">个数限制</param>
        /// <param name="regexOptions">正则表达式匹配设置</param>
        /// <param name="autoTrim">对分割后的先进行Trim</param>
        /// <returns>成功返回true且返回分割完后的list</returns>
        public static bool TryGetMultiSelections(string str, out List<string> multiList,
            string constraintPattern = @"[\s\S]+", int countConstraint = 0, bool allowDuplicate = false,
            string splitStr = ",;；，、\\\\", RegexOptions regexOptions = RegexOptions.IgnoreCase, bool autoTrim = false)
        {
            multiList = new List<string>();
            if (str.IsNullOrEmpty() || constraintPattern.IsNullOrEmpty() || splitStr.IsNullOrEmpty()) return false;
            Regex regex = new Regex("[" + splitStr + "]");
            if (!regex.IsMatch(str))
            {
                if (str.IsMatch(constraintPattern, regexOptions)) //不匹配说明只选择了一个选项
                {
                    multiList.Add(str);
                    return true;
                }

                return false;
            }

            var list = regex.Split(str);
            if (list.IsNullOrEmptySet()) return false;
            foreach (var item in list)
            {
                if (countConstraint > 0 && multiList.Count > countConstraint) break;
                if (!item.IsNullOrEmpty() && item.IsMatch(constraintPattern))
                {
                    if (!allowDuplicate && multiList.Contains(item)) continue;
                    multiList.Add(autoTrim ? item.Trim() : item);
                }
            }
            return multiList.Count > 0;
        }
    }
}
