using System;
using System.IO;

namespace DotnetRuntimeBootstrapper.AppHost.Cli.Utils;

internal class ConsoleProgress : IDisposable
{
    private readonly TextWriter _writer;
    private readonly int _posX;
    private readonly int _posY;

    private int _lastLength;

    public ConsoleProgress(TextWriter writer)
    {
        _writer = writer;
        _posX = Console.CursorLeft;
        _posY = Console.CursorTop;
    }

    private void EraseLast()
    {
        if (_lastLength > 0)
        {
            Console.SetCursorPosition(_posX, _posY);
            _writer.Write(new string(' ', _lastLength));
            Console.SetCursorPosition(_posX, _posY);
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