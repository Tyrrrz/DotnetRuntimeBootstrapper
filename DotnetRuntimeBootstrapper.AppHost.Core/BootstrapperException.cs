using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core;

public class BootstrapperException : Exception
{
    public BootstrapperException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}
