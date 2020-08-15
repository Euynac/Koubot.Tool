using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Koubot.Tool.Expand;

namespace Koubot.Tool.Web
{
    /// <summary>
    /// 提供Http请求快速获得响应等Web功能
    /// </summary>
    public class WebHelper
    {
        /// <summary>
        /// 常用ContentType
        /// </summary>
        public enum WebContentType
        {
            /// <summary>
            /// application/x-www-form-urlencoded 即?key1=value1＆key2=value2形式，在发送前要自行编码key和value
            /// </summary>
            [Description("application/x-www-form-urlencoded")]
            General,
            /// <summary>
            /// application/json 是POST请求以JSON的格式向服务请求发起请求或者请求返回JSON格式的响应内容，服务端接受到数据后对JSON进行解析拿到所需要的参数。自行处理数据为{"title":"test","sub":[1,2,3]}这种格式
            /// </summary>
            [Description("application/json")]
            Json,
            /// <summary>
            /// text/plain	空格要转换为 "+" 加号，但不用对特殊字符编码。
            /// </summary>
            [Description("text/plain")]
            Plain,
            /// <summary>
            /// multipart/form-data 是使用POST请求上传文件，如果上传照片，文件等，不用对字符编码。
            /// </summary>
            [Description("multipart/form-data")]
            Upload,
            /// <summary>
            /// text/html
            /// </summary>
            [Description("text/html")]
            Html,
        }

        public enum WebCharSet
        {
            /// <summary>
            /// 世界通用语言编码
            /// </summary>
            [Description("charset=utf-8")]
            UTF8,
            /// <summary>
            /// 中文编码
            /// </summary>
            [Description("charset=gb2312")]
            GB2312,
            /// <summary>
            /// 繁体中文编码
            /// </summary>
            [Description("charset=big5")]
            BIG5,
            /// <summary>
            /// 西欧的编码，英文编码
            /// </summary>
            [Description("charset=iso-8859-1")]
            ISO88591,
        }

        /// <summary>
        /// 模拟GET方法（默认编码UTF-8）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contentType">默认test/html</param>
        /// <param name="timeout">超时值 默认6000ms</param>
        /// <returns></returns>
        public static string HttpGet(string url, WebContentType contentType = WebContentType.Html, int timeout = 6000)
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = contentType.GetDescription() + ";charset=UTF-8";
            request.Timeout = timeout;
            var response = ((HttpWebResponse)request.GetResponse()).GetResponseStream();
            if (response == null) return null;
            using StreamReader reader = new StreamReader(response, encoding);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 模拟POST方法（默认编码UTF-8）
        /// </summary>
        /// <param name="url">请求url</param>
        /// <param name="body">post内容</param>
        /// <param name="contentType">所发消息的ContentType</param>
        /// <returns></returns>
        public static string HttpPost(string url, string body, WebContentType contentType = WebContentType.General)
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            //request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = contentType.GetDescription();

            byte[] buffer = encoding.GetBytes(body);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            var response = ((HttpWebResponse)request.GetResponse()).GetResponseStream();
            if (response == null) return null;
            using StreamReader reader = new StreamReader(response, encoding);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 匹配字符串中的包含的网址信息并读取出来（仅一次匹配）（若要检测是否是url可以只传url若返回""则不是url）
        /// </summary>
        /// <param name="url">包含url的字符串</param>
        /// <param name="isStrict">是否严格匹配</param>
        /// <returns></returns>
        public static string MatchUrl(string url, bool isStrict = false)
        {
            return url.Match(isStrict ? @"[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?"
                : @"([hH][tT]{2}[pP]://|[hH][tT]{2}[pP][sS]://|[wW]{3}.|[wW][aA][pP].|[fF][tT][pP].|[fF][iI][lL][eE].)[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
        }


        public static List<string> SendWebSocketRequest(string msg, string serverLocation, out Exception exception)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            ClientWebSocket a = new ClientWebSocket();
            a.ConnectAsync(new Uri(serverLocation), new CancellationToken()).Wait();
            Console.WriteLine("成功");
            exception = new Exception();
            return null;
            //var allSockets = new List<IWebSocketConnection>();
            //var server = new WebSocketServer(serverLocation);
            //server.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2();
            //List<string> resultList = new List<string>();
            //Exception tmp = new Exception();
            //bool isClosed = false;
            //server.Start(socket =>
            //{
            //    socket.OnOpen = () =>
            //    {
            //        Console.WriteLine("握手！");
            //        allSockets.Add(socket);
            //    };
            //    socket.OnClose = () =>
            //    {
            //        Console.WriteLine("Close!");
            //        allSockets.Remove(socket);
            //        isClosed = true;
            //    };
            //    socket.OnMessage = message =>
            //    {
            //        Console.WriteLine("信息：" + message);
            //        resultList.Add(message);
            //    };
            //    socket.OnError = error =>
            //    {
            //        tmp = error;
            //        Console.WriteLine("出错！" + error.Message);
            //    };
            //});
            //while (true)
            //{
            //    if (isClosed)
            //    {
            //        exception = tmp;
            //        return resultList;
            //    }
            //    Thread.Sleep(100);
            //}
        }
    }
}
