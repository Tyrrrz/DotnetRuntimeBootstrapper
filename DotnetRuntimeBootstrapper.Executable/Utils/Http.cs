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

        private static Stream GetContentStream(string url, out long length)
        {
            var request = CreateRequest(url);
            var response = request.GetResponse();

            length = response.ContentLength;
            return response.GetResponseStream() ?? Stream.Null;
        }

        public static Stream GetContentStream(string url) =>
            GetContentStream(url, out _);

        public static string GetContentString(string url)
        {
            using var stream = GetContentStream(url);
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static void DownloadFile(string url, string outputFilePath, Action<double>? handleProgress)
        {
            using var source = GetContentStream(url, out var contentLength);
            using var destination = File.Create(outputFilePath);

            var buffer = new byte[81920];

            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                // Copy data
                bytesCopied = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, bytesCopied);

                // Report progress
                totalBytesCopied += bytesCopied;
                handleProgress?.Invoke(1.0 * totalBytesCopied / contentLength);
            } while (bytesCopied > 0);
        }
    }
}