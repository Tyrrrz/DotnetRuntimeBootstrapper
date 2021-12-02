using System;

namespace DotnetRuntimeBootstrapper.Executable.Dotnet
{
    internal class DotnetHostException : Exception
    {
        public DotnetHostException(string message)
            : base(message)
        {
        }
    }
}