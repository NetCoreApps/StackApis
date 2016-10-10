using NUnitLite;
using NUnit.Common;
using System.Reflection;
using ServiceStack.Text;
using System;

namespace StackApis.Tests
{
    public class NetCoreTestsRunner
    {
        /// <summary>
        /// The main program executes the tests. Output may be routed to
        /// various locations, depending on the arguments passed.
        /// </summary>
        /// <remarks>Run with --help for a full list of arguments supported</remarks>
        /// <param name="args"></param>
        public static int Main(string[] args)
        {
            var writer = new ExtendedTextWrapper(Console.Out);
            var result = new AutoRun(((IReflectableType)typeof(NetCoreTestsRunner)).GetTypeInfo().Assembly).Execute(args, writer, Console.In);

#if DEBUG
            "Press Any Key to Quit.".Print();
            Console.Read();
#endif
            return result;
        }
    }
}