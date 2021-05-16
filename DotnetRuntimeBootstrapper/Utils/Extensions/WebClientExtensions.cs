using System;
using System.ComponentModel;
using System.Net;

namespace DotnetRuntimeBootstrapper.Utils.Extensions
{
    internal static class WebClientExtensions
    {
        public static void DownloadFileAsync(
            this WebClient webClient,
            string url,
            string filePath,
            Action<DownloadProgressChangedEventArgs> handleProgress,
            Action<AsyncCompletedEventArgs> handleCompletion)
        {
            void HandleProgressEvent(object sender, DownloadProgressChangedEventArgs args)
            {
                handleProgress(args);
            }

            void HandleCompletionEvent(object sender, AsyncCompletedEventArgs args)
            {
                webClient.DownloadProgressChanged -= HandleProgressEvent;
                webClient.DownloadFileCompleted -= HandleCompletionEvent;

                handleCompletion(args);
            }

            webClient.DownloadProgressChanged += HandleProgressEvent;
            webClient.DownloadFileCompleted += HandleCompletionEvent;

            webClient.DownloadFileAsync(new Uri(url), filePath);
        }
    }
}