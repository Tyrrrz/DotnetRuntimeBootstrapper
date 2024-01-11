using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core;

public class BootstrapperException(string message, Exception? innerException = null)
    : Exception(message, innerException);
