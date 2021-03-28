using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Koubot.Tool.Web
{
    /// <summary>
    /// 图片工具
    /// </summary>
    public class ImageTool
    {
        /// <summary>
        /// 异步下载图片文件
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileName"></param>
        /// <param name="uri"></param>
        public async void DownloadImageAsync(string directoryPath, string fileName, Uri uri)
        {
            await Task.Factory.StartNew(() => DownloadImage(directoryPath, fileName, uri));
        }
        private async void DownloadImage(string directoryPath, string fileName, Uri uri)
        {
            using var httpClient = new HttpClient();

            // Get the file extension
            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);//去除uri中图片文件不必要的query参数
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            // Create file path and ensure directory exists
            var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");
            Directory.CreateDirectory(directoryPath);

            // Download the image and write to the file
            var imageBytes = await httpClient.GetByteArrayAsync(uri);
            File.WriteAllBytes(path, imageBytes);
        }
    }
}