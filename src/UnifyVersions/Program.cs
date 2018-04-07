using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UnifyVersions
{
    public static class Program
    {
        public const string ApplicationName = "UnifyVersions";

        public static void Main(string[] args)
        {
            // UnifyVersions.exe -srcDir <srcDir> [-checkOnly]

            CommandLineApplication commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                FullName = ApplicationName
            };

            commandLineApplication.HelpOption("-? | -h | --help");
            var srcDirOption = commandLineApplication.Option("-srcDir <srcDir>", "The absolute or relative path to the src directory.", CommandOptionType.SingleValue);
            var checkOnlyOption = commandLineApplication.Option("-checkOnly", "Only check if all projects have unified versions without modifying '.csproj' file.", CommandOptionType.NoValue);

            commandLineApplication.Execute(args);


            if (!srcDirOption.HasValue())
            {
                Console.Error.WriteLine("srcDir is blank.");
                return;
            }

            if (!Directory.Exists(srcDirOption.Value()))
            {
                Console.Error.WriteLine($"The source dir doesn't exist: {srcDirOption.Value()}");
                return;
            }

            var checkResult = UnifyVersionsWorker.UnifyVersion(srcDirOption.Value(), checkOnlyOption.HasValue());

            if (checkOnlyOption.HasValue())
            {
                string rstStr = checkResult ? "Success" : "Failed";
                Console.WriteLine($"Checking result:{rstStr}");
                if (!checkResult)
                {
                    Console.WriteLine("Please run 'UnifyVersions.exe' without '-checkOnly' to fix the issue.");
                }
            }
            else
            {
                Console.Read();
            }

        }

    }
}
