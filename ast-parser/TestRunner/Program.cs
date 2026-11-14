using NUnit.Common;
using NUnit.Framework;
using NUnitLite;
using System;
using System.Reflection;
using Tests;

namespace TestRunner
{
    internal class Program
    {
        static int Main(string[] args)
        {
            return new AutoRun(typeof(TestASTParser).GetTypeInfo().Assembly)
                .Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);
        }
    }
}