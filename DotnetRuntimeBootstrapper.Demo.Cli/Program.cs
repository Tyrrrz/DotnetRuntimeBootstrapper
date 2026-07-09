using System;
using System.Linq;

namespace DotnetRuntimeBootstrapper.Demo.Cli;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello world!");

        // Show command line arguments
        if (args.Any())
        {
            Console.WriteLine("Command line arguments:");
            Console.WriteLine(string.Join(" ", args));
            Console.WriteLine();
        }
    }
}
