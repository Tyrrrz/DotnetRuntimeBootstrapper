using System;
using System.IO;
using System.Net;

namespace DotnetRuntimeBootstrapper.AppHost.Utils;

internal static class Http
{
    static Http()
    {
        try
        {
            // Disable certificate validation (valid certificate may fail on old operating systems)
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;

            // Try to enable TLS1.2 if it's supported.
            // This is not required now, but may be in the future if one of the CDNs we rely on
            // decides to limit traffic to TLS1.2+ only.
            // Windows 7 doesn't have support for TLS1.2 out of the box, so this will always fail
            // unless the user has installed the corresponding Windows update (we can't install
            // it ourselves because it would require a reboot in the middle of installation).
            // On Windows 8 and higher this should succeed.
            ServicePointManager.SecurityProtocol =
                (SecurityProtocolType) 0x00000C00 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Ssl3;
        }
        catch
        {
            // This can fail if the protocol is not available
        }
    }

    private static HttpWebRequest CreateRequest(string url, string method = "GET")
    {
        var request = (HttpWebRequest) WebRequest.Create(url);
        request.Method = method;

        return request;
    }

    private static Stream GetContentStream(string url, out long length)
    {
        try
        {
            var request = CreateRequest(url);
            var response = request.GetResponse();

            length = response.ContentLength;
            return response.GetResponseStream() ?? Stream.Null;
        }
        catch (WebException ex)
        {
            throw new WebException(
                $"Failed to send HTTP request to '{url}'.",
                ex
            );
        }
    }

    public static Stream GetContentStream(string url) =>
        GetContentStream(url, out _);

    public static string GetContentString(string url)
    {
        using var stream = GetContentStream(url);
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static void DownloadFile(string url, string outputFilePath, Action<double>? handleProgress = null)
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