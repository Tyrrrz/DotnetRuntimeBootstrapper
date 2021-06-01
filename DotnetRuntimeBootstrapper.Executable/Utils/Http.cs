using System;
using System.IO;
using System.Net;

namespace DotnetRuntimeBootstrapper.Executable.Utils
{
    internal static class Http
    {
        private static HttpWebRequest CreateRequest(string url, string method = "GET")
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = method;

            return request;
        }

        public static string GetContentString(string url)
        {
            var request = CreateRequest(url);
            var response = request.GetResponse();

            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream ?? Stream.Null);

            return reader.ReadToEnd();
        }

        public static void DownloadFile(string url, string outputFilePath, Action<double>? handleProgress)
        {
            using var destination = File.Create(outputFilePath);

            var request = CreateRequest(url);
            var response = request.GetResponse();

            using var source = response.GetResponseStream();
            if (source is null)
                return;

            var buffer = new byte[81920];

            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                bytesCopied = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, bytesCopied);

                totalBytesCopied += bytesCopied;
                handleProgress?.Invoke(1.0 * totalBytesCopied / response.ContentLength);
            } while (bytesCopied > 0);
        }
    }
}