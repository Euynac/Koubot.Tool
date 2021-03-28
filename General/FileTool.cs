using Koubot.Tool.Expand;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Koubot.Tool.General
{
    /// <summary>
    /// 文件操作的工具
    /// </summary>
    public static class FileTool
    {
        /// <summary>
        /// 用不会占用文件的方式读取程序集文件
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public static Assembly LoadAssembly(string fileUrl)
        {
            byte[] dllFileData = File.ReadAllBytes(fileUrl);//这样加载之后不会占用dll
            return Assembly.Load(dllFileData);
        }
        /// <summary>
        /// 得到调用此方法的程序集的嵌入的资源文件的所有文本（注意需要修改获取的文件为嵌入的资源）
        /// </summary>
        /// <param name="fileURI">要读取的文件路径。格式：文件夹.文件名.拓展名</param>
        /// <returns></returns>
        public static string ReadEmbeddedResource(string fileURI)
        {
            Stream stream = GetEmbeddedResourceStream(fileURI);
            if (stream == null) return null;
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 以Stream形式获取某程序集嵌入的资源文件
        /// </summary>
        /// <param name="fileURI">命名空间（程序集名）.文件夹名.文件名(包含扩展名) 若不加命名空间默认的是Koubot.SDK这个程序集</param>
        /// <returns></returns>
        public static Stream GetEmbeddedResourceStream(string fileURI)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            var result = assembly?.GetManifestResourceStream(fileURI);
            if (result != null) return result;
            result = assembly?.GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name}.{fileURI}");
            if (result != null) return result;
            Console.WriteLine("找不到嵌入的资源:" + fileURI);
            return null;
        }

        /// <summary>
        /// 根据目录获取文件名
        /// </summary>
        /// <param name="path">文件所在目录</param>
        /// <param name="needExtension">需要文件拓展名</param>
        /// <returns></returns>
        public static string GetFileName(string path, bool needExtension = true)
        {
            return needExtension ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
        }
        /// <summary>
        /// 检查路径是否是合法路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// 注意""被转义成了"
        /// 有用到具名捕获组，该模式串能匹配出path、filename、name、ext，不能匹配文件夹及文件名为.开头的，以及\\这样的存在
        public static bool IsPath(string path)
        {
            return path.IsMatch(@"^(?<path>(?:[a-zA-Z]:)?\\?(?:(?!\.)[^\\\?\/\*\|<>:""]+\\?)*?)(?:(?<filename>(?<name>[^\\\?\/\*\|<>:""]+?)\.(?<ext>[^.\\\?\/\*\|<>:""]+)))?$");
        }
        /// <summary>
        /// 根据文件路径获取当前目录名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDirectoryName(string path)//当前目录名
        {
            return Directory.GetParent(path).Name;
        }
        /// <summary>
        /// 根据文件路径获取当前目录路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDirectoryPath(string path)//目录路径
        {
            return Path.GetDirectoryName(path);
        }
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 合并目录路径与文件名（即自动处理是否末尾有\的情况）或使用Path.Combine
        /// </summary>
        /// <returns></returns>
        public static string CombineDirectoryWithFileName(string directory, string fileName)
        {
            return directory.EndsWith("\\") ? directory + fileName : directory + '\\' + fileName;
        }
        /// <summary>
        /// 向文件末尾追加写入
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void AppendFile(string path, StringBuilder content)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Append))
            {
                StreamWriter writer = new StreamWriter(fileStream);
                writer.Write(content);
                writer.Flush();
                writer.Close();
            }
        }
        /// <summary>
        /// 向文件末尾追加写入
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void AppendFile(string path, string content)
        {
            StringBuilder stringBuilder = new StringBuilder(content);
            AppendFile(path, stringBuilder);
        }
        /// <summary>
        /// 覆盖写入文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void WriteFile(string path, StringBuilder content)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(fileStream);
                writer.Write(content);
                writer.Flush();
                writer.Close();
            }
        }
        /// <summary>
        /// 覆盖写入文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void WriteFile(string path, string content)
        {
            StringBuilder stringBuilder = new StringBuilder(content);
            AppendFile(path, stringBuilder);
        }
        /// <summary>
        /// 读取文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static StringBuilder ReadFile(string path) //读取一般大小文件（未测试多大）
        {
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    StreamReader reader = new StreamReader(fileStream);
                    StringBuilder result = new StringBuilder(reader.ReadToEnd());
                    reader.Close();
                    return result;
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}
