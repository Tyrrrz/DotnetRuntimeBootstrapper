using System;
using System.IO;

namespace DotnetRuntimeBootstrapper.AppHost.Cli.Utils;

internal class ConsoleProgress : IDisposable
{
    private readonly TextWriter _writer;

    private int _lastLength;

    public ConsoleProgress(TextWriter writer) => _writer = writer;

    private void EraseLast()
    {
        if (_lastLength > 0)
        {
            // Go back
            _writer.Write(new string('\b', _lastLength));

            // Overwrite with whitespace
            _writer.Write(new string(' ', _lastLength));

            // Go back again
            _writer.Write(new string('\b', _lastLength));
        }
    }

    private void Write(string text)
    {
        EraseLast();
        _writer.Write(text);
        _lastLength = text.Length;
    }

    public void Report(double progress) => Write($"{progress:P1}");

    public void Dispose() => EraseLast();
}
