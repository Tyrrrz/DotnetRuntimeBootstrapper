using System;
using System.IO;

namespace DotnetRuntimeBootstrapper.AppHost.Cli.Utils;

internal class ConsoleProgress(TextWriter writer) : IDisposable
{
    private int _lastLength;

    private void EraseLast()
    {
        if (_lastLength > 0)
        {
            // Go back
            writer.Write(new string('\b', _lastLength));

            // Overwrite with whitespace
            writer.Write(new string(' ', _lastLength));

            // Go back again
            writer.Write(new string('\b', _lastLength));
        }
    }

    private void Write(string text)
    {
        EraseLast();
        writer.Write(text);
        _lastLength = text.Length;
    }

    public void Report(double progress) => Write($"{progress:P1}");

    public void Dispose() => EraseLast();
}
