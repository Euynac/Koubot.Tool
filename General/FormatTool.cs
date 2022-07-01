using Koubot.Tool.Extensions;
using System.Text;

namespace Koubot.Tool.General
{
    /// <summary>
    /// General format style for some scenario
    /// </summary>
    public static class FormatTool
    {
        private static readonly string[] _sizeUnit = {
            "B",
            "KB",
            "MB",
            "GB",
            "TB",
            "PB"
        };

        /// <summary>
        /// Returns a formatted string representing the size, up to PB, especially of the file length defined in the FileInfo instance (i.e. 10 KB).
        /// </summary>
        /// <param name="bytes">The bytes value.</param>
        /// <from>CSharpLib</from>
        /// <returns>The equivalent formatted string of given bytes</returns>
        public static string FormatBytes(this long bytes)
        {
            double num = bytes;
            int i;
            for (i = 0; i < _sizeUnit.Length && bytes >= 1024L; bytes /= 1024L)
            {
                num = bytes / 1024.0;
                ++i;
            }
            return $"{num:0.###}{_sizeUnit[i]}";
        }
        /// <summary>
        /// Progress bar style format like "[|||||||||  ]"
        /// </summary>
        /// <param name="len">the whole bar's str length. (At least 3)</param>
        /// <param name="percentage">the progress percentage of whole bar</param>
        /// <returns></returns>
        public static string FormatProgressBar(int len, double percentage)
        {
            if (len <= 2) return null;
            int progressCount = (int)((len - 2) * percentage.LimitInRange(0, 1));
            int blankCount = len - 2 - progressCount;
            StringBuilder result = new StringBuilder();
            result.Append("[");
            for (int i = 0; i < progressCount; i++)
            {
                result.Append("|");
            }
            for (int i = 0; i < blankCount; i++)
            {
                result.Append(" ");
            }

            result.Append("]");
            return result.ToString();
        }
    }
}