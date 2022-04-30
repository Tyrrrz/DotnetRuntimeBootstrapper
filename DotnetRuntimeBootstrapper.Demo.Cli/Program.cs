using System;
using System.Linq;

namespace DotnetRuntimeBootstrapper.Demo.Cli;

public static class Program
{
    public static void Main(string[] args)
    {
        // Show routed command line arguments
        if (args.Any())
        {
            Console.WriteLine("Routed command line arguments:");
            Console.WriteLine(string.Join(" ", args));
        }

        Console.WriteLine();
        Console.WriteLine("Hello world!");
    }
}