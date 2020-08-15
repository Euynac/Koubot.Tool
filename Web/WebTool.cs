using System;
using System.Security.Cryptography;
using System.Text;

namespace Koubot.Tool.Web
{
    /// <summary>
    /// 网络/安全方向工具
    /// </summary>
    public class WebTool
    {
        /// <summary>
        /// Base64加密，出错将返回原文，默认按UTF8加密
        /// </summary>
        /// <param name="source">原文</param>
        /// <param name="code_type">编码类型</param>
        /// <returns></returns>
        public static string EncodeBase64(string source, Encoding code_type = null)
        {
            if (string.IsNullOrEmpty(source)) return "";
            if (code_type == null)
            {
                code_type = Encoding.UTF8;
            }
            string encode;
            try
            {
                byte[] bytes = code_type.GetBytes(source);
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = source;
            }
            return encode;
        }

        /// <summary>
        /// Base64解密，出错将返回原文，默认按UTF8解密
        /// </summary>
        /// <param name="source">原文</param>
        /// <param name="code_type">编码类型，为空默认是UTF8</param>
        /// <returns></returns>
        public static string DecodeBase64(string source, Encoding code_type = null)
        {
            if (string.IsNullOrEmpty(source)) return "";
            if (code_type == null)
            {
                code_type = Encoding.UTF8;
            }
            string decode;
            try
            {
                byte[] bytes = Convert.FromBase64String(source);
                decode = code_type.GetString(bytes);
            }
            catch
            {
                decode = source;
            }
            return decode;
        }

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncryptStringMD5(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
    }
}
