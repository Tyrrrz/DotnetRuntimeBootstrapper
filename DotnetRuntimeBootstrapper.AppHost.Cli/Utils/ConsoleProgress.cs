using System;
using System.IO;

namespace DotnetRuntimeBootstrapper.AppHost.Cli.Utils;

internal class ConsoleProgress : IDisposable
{
    private readonly TextWriter _writer;
    private readonly int _posX;
    private readonly int _posY;

    public ConsoleProgress(TextWriter writer)
    {
        _writer = writer;
        _posX = Console.CursorLeft;
        _posY = Console.CursorTop;
    }

    public void Report(double progress)
    {
        Console.SetCursorPosition(_posX, _posY);
        _writer.Write($"{progress:P1}");
    }

    public void Dispose()
    {
        Console.SetCursorPosition(_posX, _posY);
        _writer.Write("Done ✓");
    }
}