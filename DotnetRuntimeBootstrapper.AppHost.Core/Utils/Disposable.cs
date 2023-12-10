using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils;

internal class Disposable(Action dispose) : IDisposable
{
    public void Dispose() => dispose();

    public static IDisposable Create(Action dispose) => new Disposable(dispose);
}
