using System;
using System.ComponentModel;
using System.Net;

namespace DotnetRuntimeBootstrapper.Utils
{
    internal class HttpClient : IDisposable
    {
        private readonly WebClient _webClient = new WebClient();

        public Task DownloadAsync(string url, string outputFilePath, Action<double> handleProgress)
        {
            return Task.Create(source =>
            {
                void HandleProgressEvent(object sender, DownloadProgressChangedEventArgs args)
                {
                    handleProgress(1.0 * args.BytesReceived / args.TotalBytesToReceive);
                }

                void HandleCompletionEvent(object sender, AsyncCompletedEventArgs args)
                {
                    _webClient.DownloadProgressChanged -= HandleProgressEvent;
                    _webClient.DownloadFileCompleted -= HandleCompletionEvent;

                    if (args.Error != null)
                    {
                        source.SetFailed(args.Error);
                    }
                    else if (args.Cancelled)
                    {
                        source.SetFailed(new OperationCanceledException());
                    }
                    else
                    {
                        source.SetSuccessful();
                    }
                }

                _webClient.DownloadProgressChanged += HandleProgressEvent;
                _webClient.DownloadFileCompleted += HandleCompletionEvent;

                _webClient.DownloadFileAsync(new Uri(url), outputFilePath);
            });
        }

        public void Dispose() => _webClient.Dispose();
    }
}