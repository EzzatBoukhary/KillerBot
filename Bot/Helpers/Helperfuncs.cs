using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using System.IO;
using System.Net.Http;

namespace Bot.Helpers
{
    public class HelperFuncs
    {
        public static readonly HttpClient httpClient = new HttpClient();
        public static string GetUpTime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        public static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
        public static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
            {
                yield return str.Substring((i < 0) ? 0 : i, Math.Min(maxChunkSize, str.Length - i));
            }
        }
        public static async Task<byte[]> DownloadFileBytesAsync(string url)
        {
            try
            {
                var webRequest = HttpWebRequest.Create(url);
                webRequest.Method = "HEAD";

                using (var webResponse = await webRequest.GetResponseAsync())
                {
                    var fileSize = webResponse.Headers.Get("Content-Length");
                    // is too big?
                    if (Convert.ToInt64(fileSize) / 5000000 == 0)
                    {
                        // The file is ok, less than 5mb decimal
                        var request = await httpClient.GetByteArrayAsync(url);
                        return request;
                    }
                }
                return null;


            }
            catch (Exception)//e)
            {
                return null;
            }
        }


        public static ImageFactory GetImageFactory(MemoryStream imgStream)
        {
            return new ImageFactory(preserveExifData: true).Load(imgStream);
        }
    }
}